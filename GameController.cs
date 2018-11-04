using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class GameController : MonoBehaviour {
    private const int WaitTime = 5;
    Pathfinder pathfinder;
    TextUpdate textUpdate;

    
    Waypoint[] team1Goal;
    Waypoint[] team2Goal;
    
    public GameState gameState;
    Turn currentTurn;
    
    List<PlayerController> team1 = new List<PlayerController>();
    List<PlayerController> team2 = new List<PlayerController>();
    

    public PlayerController selectedPlayer;
    public Waypoint selectedWaypoint;

    PuckController puck;
    [SerializeField] Waypoint puckStart;
    
    int turnCount;
    public int TurnCount
    {
        get { return turnCount; }
    }

    bool team1Done = false;
    bool team2Done = false;

    public bool actionBlocked = false;
    public bool actionCompleted = false;

    [SerializeField] int maxTurns = 20;

    [SerializeField] AudioSource ambientSource;
    [SerializeField] AudioSource actionSource;

    [SerializeField] AudioClip ambientSound;
    [SerializeField] AudioClip blockedShotSound;//Credit To http://www.freesfx.co.uk for the Sound
    [SerializeField] AudioClip Shot_PassSound;
    [SerializeField] AudioClip moveSound;
    [SerializeField] AudioClip missedShotSound;
    [SerializeField] AudioClip goalHornSound1;//Chicago Blackhawks
    [SerializeField] AudioClip goalHornSound2;//Vegas Golden Knights
    // Use this for initialization
    void Start () {

        
        team1Goal = GetComponent<GoalWaypointLocation>().team1GoalPosts;
        team2Goal = GetComponent<GoalWaypointLocation>().team2GoalPosts;
        puck = FindObjectOfType<PuckController>();
        textUpdate = FindObjectOfType<TextUpdate>();
        currentTurn = Turn.team2;
        pathfinder = FindObjectOfType<Pathfinder>();
        gameState = GameState.pickPlayer;
        LoadTeams();
        LineUpForFaceOff();
        ambientSource.clip = ambientSound;
        ambientSource.Play();
	}

    private void LoadTeams()
    {
        PlayerController[] players = FindObjectsOfType<PlayerController>();
        foreach(PlayerController player in players)
        {
            if (player.tag.Equals("Team1"))
            {
                team1.Add(player);
            }
            else if(player.tag.Equals("Team2"))
            {
                team2.Add(player);
            }
        }
    }
    public Turn GetCurrentTurn()
    {
        return currentTurn;
    }

    public void NextTurn()
    {
        //Called By the Next Turn Button
        if (currentTurn.Equals(Turn.team1))
        {
            team1Done = true;
            currentTurn = Turn.team2;
        }
        else
        {
            team2Done = true;
            currentTurn = Turn.team1;
        }
        if (team1Done && team2Done)
        {
            textUpdate.UpdateTurnCountText();
            team1Done = false;
            team2Done = false;
        }
        actionBlocked = false;
        ResetHighLights();
        gameState = GameState.startTurn;
    }

    public void MoveButton()
    {
        if (gameState.Equals(GameState.chooseActions))
        {
            MovePlayer();
        }
        
        gameState = GameState.checkForMorePlayers;
    }
    public void PlayMoveSounds()
    {
        actionSource.clip = moveSound;
        actionSource.Play();
    }
    private void MovePlayer()
    {
        actionCompleted = false;
        if (selectedWaypoint.walkable)
        {
            if (Math.Abs(selectedWaypoint.GetGridPos().x - selectedPlayer.PlayerGridPos().x) < selectedPlayer.GetMoveRange())
            {
                if (Math.Abs(selectedWaypoint.GetGridPos().y - selectedPlayer.PlayerGridPos().y) < selectedPlayer.GetMoveRange())
                {

                    var path = pathfinder.GetPath(selectedPlayer.wayPointCovering, selectedWaypoint);
                    if (selectedPlayer.HasPuck)
                    {
                        puck.FollowPlayer();
                    }
                    selectedPlayer.SetMovePath(path);

                    selectedPlayer.hasCompletedAction = true;
                    


                }
                else
                {
                    gameState = GameState.pickWaypoint;
                    StartCoroutine(textUpdate.UpdateSecondaryText(WaitTime, "Waypoint out of range, select again"));
                    return;
                }
            }
            else
            {
                gameState = GameState.pickWaypoint;
                StartCoroutine(textUpdate.UpdateSecondaryText(WaitTime, "Waypoint out of range, select again"));
                return;
            }
            
        }
        else
        {

            StartCoroutine(textUpdate.UpdateSecondaryText(WaitTime, "Can't move to selected location, try again"));
            selectedPlayer.hasCompletedAction = false;
            selectedPlayer = null;
            selectedWaypoint = null;
            gameState = GameState.pickPlayer;
           
        }
    }

    
    
    // Update is called once per frame
    void Update ()
    {

        turnCount = textUpdate.TurnCount;
        
        GameStateControl();
        textUpdate.UpdateCurrentGameAction(gameState);
        textUpdate.UpdateCurrentTeamText(currentTurn);
        LookForWinner();

    }

    private void GameStateControl()
    {
        switch (gameState)
        {
            case GameState.pickPlayer:
                actionCompleted = false;
                break;
            case GameState.pickWaypoint:
                break;
            
            case GameState.checkForMorePlayers:
                CheckForHasMoved();
                break;
            case GameState.chooseActions:

                break;

            case GameState.startTurn:
                ResetCurrentTurn();
                gameState = GameState.pickPlayer;
                break;
            case GameState.nextTurn:

                selectedPlayer = null;
                selectedWaypoint = null;

                NextTurn();
                break;
            case GameState.scored:
                selectedPlayer.SetHighLight(false);
                textUpdate.UpdateTurnCountText();
                LineUpForFaceOff();
                break;
            case GameState.faceoff:
                textUpdate.DisplayFaceOffComponents();
                break;
            case GameState.faceOffSecondary:
                break;
            case GameState.endGame:
                break;
        }
    }

    private void LookForWinner()
    {
        if (maxTurns <= textUpdate.TurnCount)
        {
            Turn winner;
            switch (textUpdate.Team1Score < textUpdate.Team2Score)
            {
                case true:
                    winner = Turn.team2;
                    break;
                case false:
                    winner = Turn.team1;
                    break;
                default:
                    winner = Turn.team1;
                    break;
            }
            selectedPlayer = null;
            selectedWaypoint = null;
            Debug.Log(winner);
            textUpdate.UpdateWinnerText(winner);
            gameState = GameState.endGame;

        }
    }

    private void LineUpForFaceOff()
    {
        puck.PlayerHolding = null;
        puck.CanBePickedUp = true;
        foreach(PlayerController pc in team1)
        {
                   
            Waypoint startWaypoint = pc.StartWaypoint;
            pc.transform.position = new Vector3(startWaypoint.transform.position.x, pc.transform.position.y, startWaypoint.transform.position.z);
            pc.hasCompletedAction = false;

        }
        
        foreach (PlayerController pc in team2)
        {
            Waypoint startWaypoint = pc.StartWaypoint;
            pc.transform.position = new Vector3(startWaypoint.transform.position.x, pc.transform.position.y, startWaypoint.transform.position.z);
            pc.hasCompletedAction = false;
        }
        puck.transform.position = new Vector3(puckStart.transform.position.x, puck.transform.position.y, puckStart.transform.position.z); 
       
        //Random Number Guessing Game To decide who win the face off

        
        gameState = GameState.faceoff;
    }

    

    public void TakeFaceOff(int[] faceOffGuesses)
    {
        int randNum = UnityEngine.Random.Range(1, 5);
        Debug.Log(randNum);

        int team1Guess = randNum - faceOffGuesses[0];
        int team2Guess = randNum - faceOffGuesses[1];

        Debug.Log(team1Guess + ", " + team2Guess);
        if (team1Guess < team2Guess)
        {
            Debug.Log("Team 1");
            currentTurn = Turn.team1;
            textUpdate.UpdateCurrentTeamText(currentTurn);
        }
        else
        {
            Debug.Log("Team 2");
            currentTurn = Turn.team2;
            textUpdate.UpdateCurrentTeamText(currentTurn);
        }

        gameState = GameState.pickPlayer;
    }

    private void ResetCurrentTurn()
    {
        if (currentTurn.Equals(Turn.team1))
        {
            foreach(PlayerController player in team1)
            {
                player.hasCompletedAction = false;
                var colliderTest = player.GetComponent<Collider>();
                colliderTest.enabled = false;
                colliderTest.enabled = true;
                
            }
        }
        else
        {
            foreach(PlayerController player in team2)
            {

                player.hasCompletedAction = false;
                var colliderTest = player.GetComponent<Collider>();
                colliderTest.enabled = false;
                colliderTest.enabled = true;

            }
        }
    }

    private void CheckForHasMoved()
    {
        ResetHighLights();

        int team = ChooseTeamList();
        switch (team)
        {
            case 0:
                CheckTeamList(team1);
                break;
            case 1:
                CheckTeamList(team2);
                break;
            case -1:
                Debug.LogError("Team Could not be Choosen");
                break;
        }
    }

    private void ResetHighLights()
    {
        if(selectedPlayer != null)
        {
            selectedPlayer.SetHighLight(false);
        }
        if (selectedWaypoint != null)
        {
            selectedWaypoint.SetHighlight(false);
        }
    }

    private void CheckTeamList(List<PlayerController> teamList)
    {
        List<PlayerController> playersLeft = new List<PlayerController>();
        foreach(PlayerController player in teamList)
        {
            if (!player.hasCompletedAction)
            {
                playersLeft.Add(player);
            }
        }
        if(playersLeft.Count > 0)
        {
            /* if (playersLeft.Count == 1 && !playersLeft[0].hasCompletedAction)
             {
                 selectedPlayer = playersLeft[0];
                 gameState = GameState.pickWaypoint;
                 print("Last Player selected");
             }
             else
             {
                 gameState = GameState.pickPlayer;
             }
             */
            selectedPlayer = null;
            selectedWaypoint = null;
            gameState = GameState.pickPlayer;
        }
        else
        {
            if (currentTurn.Equals(Turn.team1))
            {
                team1Done = true;
            }
            else
            {
                team2Done = true;
            }
            gameState = GameState.nextTurn;
        }
    }

    private int ChooseTeamList()
    {
        int teamInt = -1;
        switch (currentTurn)
        {
            case Turn.team1:
                teamInt = 0;
                break;
            case Turn.team2:
                teamInt = 1;
                break;
        }
        return teamInt;
    }

    public void BodyCheck()
    {
        
        
        if (!selectedPlayer.HasBeenBodyChecked)
        {
            PlayerController otherPlayer = selectedWaypoint.PlayerCovering;
            if (otherPlayer.HasPuck)
            {
                puck.ChangePlayers(selectedPlayer);
                Waypoint closestWaypoint = pathfinder.GetClosestWaypoint(otherPlayer.wayPointCovering);
                Waypoint otherPlayerWaypoint = otherPlayer.wayPointCovering;
                Waypoint selectedPlayerWaypoint = selectedPlayer.wayPointCovering;
                if (closestWaypoint.Equals(null))
                {
                    otherPlayer.SetMovePath(pathfinder.GetPath(otherPlayerWaypoint, selectedPlayerWaypoint));
                    selectedPlayer.SetMovePath(pathfinder.GetPath(selectedPlayerWaypoint, otherPlayerWaypoint));
                    puck.FollowPlayer();
                }
                else
                {
                    otherPlayer.SetMovePath(pathfinder.GetPath(otherPlayerWaypoint, closestWaypoint));
                    selectedPlayer.SetMovePath(pathfinder.GetPath(selectedPlayerWaypoint, otherPlayerWaypoint));
                    puck.FollowPlayer();
                }

                puck.FollowPlayer();
                otherPlayer.BodyChecked(turnCount);
                selectedPlayer.hasCompletedAction = true;

                gameState = GameState.checkForMorePlayers;
            }
            else
            {
                StartCoroutine(textUpdate.UpdateSecondaryText(WaitTime, "The player does not have the puck, select again"));
                gameState = GameState.chooseActions;
            }
           
        }
       
    }
    
    public void Block()
    {
        selectedPlayer.Block(turnCount);
        gameState = GameState.checkForMorePlayers;
    }
   
    public void ShootPuck()
    {
        actionCompleted = false;
        
        float distanceToGoal;
        Waypoint goalWaypoint;
       

        switch (currentTurn)
        {
            case Turn.team1:
                distanceToGoal = Vector2Int.Distance(selectedPlayer.PlayerGridPos(), team2Goal[1].GetGridPos());
                goalWaypoint = team2Goal[1];
                break;
            case Turn.team2:
                distanceToGoal = Vector2Int.Distance(selectedPlayer.PlayerGridPos(), team1Goal[1].GetGridPos());
                goalWaypoint = team1Goal[1];
                break;
            default:
                distanceToGoal = 0;
                goalWaypoint = selectedPlayer.wayPointCovering;
                Debug.LogError("No Team(turn) Enum could be found");
                break;
        }
       

        bool onTarget = CheckOdds(distanceToGoal, selectedPlayer.ShotAccuracy, selectedPlayer.AccurateDistance);
        Debug.Log(onTarget);

        if(onTarget)
        {
            
            //completedShot = true;
            StartCoroutine(puck.MovePuck(goalWaypoint,selectedPlayer.wayPointCovering, false));
            StartCoroutine(WaitForPuckToScore(onTarget));
            Debug.Log("Scored");
        }
        else
        {
            selectedPlayer.HasPuck = false;
            Vector2Int waypointBehindGoalVInt = goalWaypoint.GetGridPos();
            Vector2Int vectorDouble = new Vector2Int(2, 2);
            switch (currentTurn)
            {
                case Turn.team1:
                    waypointBehindGoalVInt += Vector2Int.Scale(Vector2Int.right, vectorDouble);
                    break;
                case Turn.team2:
                    waypointBehindGoalVInt += Vector2Int.Scale(Vector2Int.left, vectorDouble);
                    break;

            }

            goalWaypoint = pathfinder.MissedAction(waypointBehindGoalVInt, goalWaypoint);
            StartCoroutine(puck.MovePuck(goalWaypoint,selectedPlayer.wayPointCovering));
            StartCoroutine(WaitForPuckToScore(onTarget));
            Debug.Log("missed");
        }

        actionSource.clip = Shot_PassSound;
        actionSource.Play();
       
        selectedPlayer.hasCompletedAction = true;
        
    }

    private IEnumerator WaitForPuckToScore(bool onTarget)
    {
        yield return new WaitUntil(() => actionCompleted == true);
        if(actionBlocked || !onTarget)
        {
            if (actionBlocked)
            {
                actionSource.clip = blockedShotSound;
                actionSource.Play();
            }
            else
            {
                actionSource.clip = missedShotSound;
                actionSource.Play();
            }

            gameState = GameState.checkForMorePlayers;
        }
        else
        {
            PlayTeamGoalSong();
            textUpdate.UpdateScoreText(currentTurn);
            gameState = GameState.scored;

            
        }
        
       
    }

    private void PlayTeamGoalSong()
    {
        switch (currentTurn)
        {
            case Turn.team1:
                actionSource.clip = goalHornSound1;
                break;
            case Turn.team2:
                actionSource.clip = goalHornSound2;
                break;
        }
        actionSource.Play();
    }

  

    public void PassPuck()
    {
        actionCompleted = false;
        actionBlocked = false;
        //puck.MovePuck(selectedWaypoint);
        float distanceToTarget = Mathf.Abs(Vector2Int.Distance(selectedPlayer.wayPointCovering.GetGridPos(), selectedWaypoint.GetGridPos()));
        bool onTarget = CheckOdds(distanceToTarget, selectedPlayer.PassAccuracy, selectedPlayer.PassDistance);
        puck.ChangePlayers(selectedWaypoint.PlayerCovering, true);
        Debug.Log(onTarget);
        if (onTarget)
        {
            
            StartCoroutine(puck.MovePuck(selectedWaypoint,selectedPlayer.wayPointCovering));
            StartCoroutine(WaitForPuckToMove(onTarget));
        }
        else
        {
            Vector2Int missedIndex;
            if(selectedWaypoint.GetGridPos().y >= 3)
            {
                missedIndex = selectedWaypoint.GetGridPos() + Vector2Int.down;
            }
            else
            {
                missedIndex =  selectedWaypoint.GetGridPos() + Vector2Int.up;
            }
            
            Waypoint newTargetWaypoint = pathfinder.MissedAction(missedIndex, selectedWaypoint);
            StartCoroutine(puck.MovePuck(newTargetWaypoint, selectedPlayer.wayPointCovering));
            
        }
        selectedWaypoint.PlayerCovering.hasCompletedAction = true;
        selectedPlayer.hasCompletedAction = true;
        gameState = GameState.checkForMorePlayers;
    }

    private IEnumerator WaitForPuckToMove(bool onTarget)
    {
        yield return new WaitUntil(() => actionCompleted == true);
        if (onTarget)
        {
            //puck.ChangePlayers(selectedWaypoint.PlayerCovering);
            //Debug.Log(selectedWaypoint.PlayerCovering.HasPuck);
            puck.FollowPlayer();
        }
    }
    
    private bool CheckOdds(float distanceToGoal, float playerBaseAccuracy, int accurateDistance)
    {
        float threshold;
        if (distanceToGoal > accurateDistance)
        {
            threshold = distanceToGoal * (playerBaseAccuracy / 2);
        }
        else
        {
            threshold = distanceToGoal * playerBaseAccuracy;
        }
        
        float randomNum = UnityEngine.Random.Range(0, distanceToGoal);
        Debug.Log("Threshold  = " + threshold + "randomNum = " + randomNum);
        if (randomNum <= threshold)
        {
            return true;
        }
        else
        {
            return false;
        }
    }
    

    public void UndoButton()
    {
        if (gameState.Equals(GameState.pickWaypoint))
        {
            selectedPlayer.SetHighLight(false);
            selectedPlayer = null;
            gameState = GameState.pickPlayer;
        }
        else if (gameState.Equals(GameState.chooseActions))
        {
            selectedWaypoint.SetHighlight(false);
            selectedWaypoint = null;
            gameState = GameState.pickWaypoint;
        }
        

    }


}
