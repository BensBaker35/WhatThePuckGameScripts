using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class Waypoint : MonoBehaviour
{
    const int gridSize = 5;
    public int GetGridSize() {return gridSize;}

    Vector2Int gridPos;

    public bool isExplored = false;
    public bool walkable = true;
    bool hasPuck = false;
    public bool HasPuck
    {
        get { return hasPuck; }
    }
    //Color origionalColor;
    GameController gameController;

    Camera camera;
    [SerializeField] Material nonHighLightMaterial;
    [SerializeField] Material highLightMaterial;

    

    PlayerController playerCovering;
    public PlayerController PlayerCovering
    {
        get { return playerCovering; }
    }
     public Waypoint exploredFrom;

    public Vector2Int GetGridPos()
    {
        return new Vector2Int(//Give this control to hte player 
            Mathf.RoundToInt(transform.position.x / gridSize)  ,
            Mathf.RoundToInt(transform.position.z / gridSize)
            );
        
    }
    public void SetHighlight(bool isHighlightOn)
    {
        if (!isHighlightOn)
        {
            
            SetTopMaterial(nonHighLightMaterial);
        }
        else
        {
            
            SetTopMaterial(highLightMaterial);
        }
    }
    public void SetTopMaterial(Material material) 
    {

        MeshRenderer topMeshRenderer = transform.Find("Top").GetComponent<MeshRenderer>();
       
        topMeshRenderer.material = material;
    }

    

    private void Start()
    {
        camera = FindObjectOfType<Camera>();
        gameController = FindObjectOfType<GameController>();
       
    }

    void Update()
    {
       
        GetMouseInput();
    }

    

    private void OnTriggerEnter(Collider other)
    {

        
        //print(other.tag);
        if (other.tag.Equals("Puck"))
        {
            other.GetComponent<PuckController>().CoveringWaypoint = this;
            hasPuck = true;
        }
        else
        {
            
            walkable = false;
            playerCovering = other.GetComponent<PlayerController>();
            playerCovering.wayPointCovering = this;

        }
    }
    private void OnTriggerExit(Collider other)
    {
        hasPuck = false;
        walkable = true;
    }

    private void GetMouseInput()
    {
        //Turn currentTurn = gameController.GetCurrentTurn();
        GameState currentGameState = gameController.gameState;

        if (currentGameState.Equals(GameState.pickWaypoint))
        {
            if (Input.GetMouseButton(0))
            {
                Ray ray = camera.ScreenPointToRay(Input.mousePosition);
                RaycastHit rayCastHit;

                if (Physics.Raycast(ray, out rayCastHit))
                {
                    if (rayCastHit.collider.Equals(GetComponent<Collider>()))
                    {
                        gameController.selectedWaypoint = this;
                        print("Selected Waypoint" + this.name);
                        SetHighlight(true);
                        gameController.gameState = GameState.chooseActions;
                    }
                }
            }
        }
    }

  
}