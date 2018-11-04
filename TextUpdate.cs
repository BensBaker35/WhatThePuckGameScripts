using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TextUpdate : MonoBehaviour {
    [SerializeField] Text turnCountText;
    [SerializeField] Text currentActionText;
    [SerializeField] Text currentTeamText;
    [SerializeField] Text team1ScoreText;
    [SerializeField] Text team2ScoreText;
    [SerializeField] Text winText;
    [SerializeField] InputField faceOffInputTeam1;
    [SerializeField] InputField faceOffInputTeam2;
    [SerializeField] Text secondaryActionText;

    bool showErrorText = false;
    int[] faceoffGuesses = new int[2];

    int team1Score = 0;
    
    public int Team1Score
    {
        get { return team1Score; }
    }
    int team2Score = 0;
    public int Team2Score
    {
        get { return team2Score; }
    }

    int turnCount = 1;
    public int TurnCount
    {
        get { return turnCount; }
    }

    bool upDatedScoreAlready = false;
    public bool UpdatedScoreAlready
    {
        set { UpdatedScoreAlready = upDatedScoreAlready; }
    }

    public void Start()
    {
        turnCountText.text = "Turn: " + turnCount;
        team1ScoreText.text = team1Score + "";
        team2ScoreText.text = team2Score + "";
        winText.enabled = false;
        secondaryActionText.enabled = false;
        faceOffInputTeam1.gameObject.SetActive(false);
        //FaceOffInputTeam1.enabled = false;
        faceOffInputTeam1.contentType = InputField.ContentType.DecimalNumber;
        faceOffInputTeam2.gameObject.SetActive(false);
        //FaceOffInputTeam2.enabled = false;
        faceOffInputTeam2.contentType = InputField.ContentType.DecimalNumber;
    }

    public void UpdateTurnCountText()
    {
        turnCount++;
        turnCountText.text = "Turn: " + turnCount;

    }

    public void UpdateCurrentTeamText(Turn currentTeam)
    {
        if (currentTeam.Equals(Turn.team1))
        {
            currentTeamText.text = "Team 1";
        }
        else
        {
            currentTeamText.text = "Team 2";
        }
    }
    public void UpdateCurrentGameAction(GameState gameState)
    {
        switch (gameState)
        {
            case GameState.pickPlayer:
                currentActionText.text = "Pick a Player";
                break;
            case GameState.pickWaypoint:
                currentActionText.text = "Pick a Waypoint";
                break;
            case GameState.chooseActions:
                currentActionText.text = "Choose an Action";
                break;

        }
    }

    public void UpdateScoreText(Turn scoringTeam)
    {
        if (!upDatedScoreAlready)
        {
            switch (scoringTeam)
            {
                case Turn.team1:
                    team1Score++;
                    break;
                case Turn.team2:
                    team2Score++;
                    break;
            }
            
        }
        Debug.Log("Scored");
        team1ScoreText.text = team1Score + "";
        team2ScoreText.text = team2Score + "";
    }

    public void UpdateWinnerText(Turn winner)
    {
        
        string tempText = winner.ToString();

        tempText = tempText.Replace('t', 'T');

        if (winner.Equals(Turn.team1))
        {
            tempText = tempText.Replace('1', ' ');
            tempText += "1";
        }
        else
        {
            tempText = tempText.Replace('2', ' ');
            tempText += "2";
        }

        tempText += " Wins!";

        winText.enabled = true;
        winText.text = tempText;
       
       
    }

    public void DisplayFaceOffComponents()
    {
        faceOffInputTeam1.gameObject.SetActive(true);
        
       
    }

    public void FaceOffTemp(int control)
    {
        int inputInt;
        if (control == 1)
        {
            inputInt = int.Parse(faceOffInputTeam1.text);
        }
        else
        {
            inputInt = int.Parse(faceOffInputTeam2.text);
        }
       

        if(inputInt > 5 || inputInt < 1)
        {
            UpdateSecondaryText("Enter a new number between 1 and 5");
        }
        else
        {
            secondaryActionText.enabled = false;
            if (control == 1)
            {
                faceoffGuesses[0] = inputInt;
                faceOffInputTeam1.text = null;
                Debug.Log(faceoffGuesses[0]);
            }
            else
            {
                
                faceoffGuesses[1] = inputInt;
                Debug.Log(faceoffGuesses[1]);
                GameController gameController = FindObjectOfType<GameController>();
                gameController.gameState = GameState.faceOffSecondary;
                gameController.TakeFaceOff(faceoffGuesses);
                faceOffInputTeam2.text = null;



            }
        }

        

        

    }
    
    public void ToggleFaceOff(bool toggle)
    {
        faceOffInputTeam1.enabled = toggle;
    }

    public IEnumerator UpdateSecondaryText(int waitTime, string message)
    {
        secondaryActionText.enabled = true;
        secondaryActionText.text = message;
        yield return new WaitForSeconds(waitTime);
        secondaryActionText.enabled = false;
    }

    public void UpdateSecondaryText(string message)
    {
        secondaryActionText.enabled = true;
        secondaryActionText.text = message;
    }
}
