using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class PlayerController : MonoBehaviour {

    const int gridSize = 5;
    public Waypoint wayPointCovering;
    List<Waypoint> movePath;
    GameController gameController;
    MeshRenderer meshRenderer;
    Turn team;
    Camera camera;
    public bool hasCompletedAction = false;
    [SerializeField] Material nonHighlightMaterial;
    [SerializeField] Material highlightMaterial;
    [SerializeField] int playerRange = 5;
    [SerializeField] int blockLength = 2;
    [SerializeField] float shotAccuracy = .7f;

    public float ShotAccuracy
    {
        get { return shotAccuracy; }
    }
    [SerializeField] int passDistance = 5;
    public int PassDistance
    {
        get { return passDistance; }
    }
    [SerializeField] float passAccuracy = .9f;
    public float PassAccuracy
    {
        get { return passAccuracy; }
    }

    [SerializeField] int accurateDistance = 2;
    public int AccurateDistance
    {
        get { return accurateDistance; }
    }
    PuckController puck;

    bool hasPuck = false;
    public bool HasPuck
    {
        get { return hasPuck; }
        set { hasPuck = value; }
    }

    private bool hasBeenBodyChecked = false;
    public bool HasBeenBodyChecked
    {
        get { return hasBeenBodyChecked; }
    }
    private int turnBodyChecked = -1;

    private bool isBlocking = false;
    public bool IsBlocking
    {
        get { return isBlocking; }
    }
    private int blockingStartTurn;

    [SerializeField]Waypoint startPoint = null;
    public Waypoint StartWaypoint
    {
        get { return startPoint; }
    }

    

    void Start()
    {
        puck = FindObjectOfType<PuckController>();
        
        SelectTeam();

        gameController = FindObjectOfType<GameController>();

        camera = FindObjectOfType<Camera>();

        meshRenderer = GetComponent<MeshRenderer>();
    }

    private void SelectTeam()
    {
        if (tag.Equals("Team1"))
        {
            team = Turn.team1;
        }    
        else if (tag.Equals("Team2"))
        {
            team = Turn.team2;
        }
        else
        {
            Debug.LogError("Can't find tag on" + this.name);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
        GetMouseInput();
        
        if (isBlocking && gameController.TurnCount == blockingStartTurn + blockLength)
        {
            isBlocking = false;
        }
        else if (isBlocking)
        {
            hasCompletedAction = true;
        }

        if (hasBeenBodyChecked)
        {
            if (gameController.TurnCount == turnBodyChecked + 2)
            {
                hasBeenBodyChecked = false;
            }
        }

    }
    public void Block(int startTurn)
    {
        blockingStartTurn = startTurn;
        isBlocking = true;
    }
    
    private void LookForPuck()
    {
        if (!hasPuck)
        {
            if (puck.CanBePickedUp && wayPointCovering.HasPuck)
            {

                hasPuck = true;
                puck.CanBePickedUp = false;
                puck.PlayerHolding = this;
                puck.SetPuckFacing();

            }
            else
            {
                hasPuck = false;
            }
        }


    }

    

    private void GetMouseInput()
    {
        Turn currentTurn = gameController.GetCurrentTurn();
        GameState currentGameState = gameController.gameState;

        if(currentTurn.Equals(team) && currentGameState.Equals(GameState.pickPlayer) && !hasCompletedAction)
        {

            if (Input.GetMouseButton(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayCastHit;
                
                if(Physics.Raycast(ray,out rayCastHit))
                {
                    if (rayCastHit.collider.Equals(GetComponent<Collider>()))
                    {
                        gameController.selectedPlayer = this;
                        print("Selected Player " + this.name);
                        SetHighLight(true);
                        gameController.gameState = GameState.pickWaypoint;
                        
                    }
                }
            }
        }
    }

    public void SetHighLight(bool isHighlightOn)
    {
        if (!isHighlightOn)
        {
            
            meshRenderer.material = nonHighlightMaterial;
        }
        else
        {
            
            meshRenderer.material = highlightMaterial;
        }
    }

    public void SetMovePath(List<Waypoint> path)
    {
        movePath = path;
        StartCoroutine(MovePlayer());
    }

    public Vector2Int PlayerGridPos()
    {
        return new Vector2Int( 
            Mathf.RoundToInt(transform.position.x / gridSize),
            Mathf.RoundToInt(transform.position.z / gridSize)
            );
    }

    public IEnumerator MovePlayer()
    {
        
        foreach (Waypoint waypoint in movePath)
        {
            
            transform.position = new Vector3(waypoint.transform.position.x, transform.position.y, waypoint.transform.position.z);
            gameController.PlayMoveSounds();
            LookForPuck();
            if (hasPuck)
            {
                puck.FollowPlayer();
            }
            yield return new WaitForSeconds(.25f);
            LookForPuck();
            gameController.actionCompleted = true;
        }
        
    }

    public int GetMoveRange()
    {
        return playerRange;
    }

    public void BodyChecked(int turnNumber)
    {
        turnBodyChecked = turnNumber;
        hasBeenBodyChecked = true;
    }
}
