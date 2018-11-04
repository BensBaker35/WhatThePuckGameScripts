using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//Made with the help of Ben Tristem and Learn How to make games in Unity Udemy Course
public class Pathfinder : MonoBehaviour {

    Dictionary<Vector2Int, Waypoint> grid = new Dictionary<Vector2Int, Waypoint>();
    Queue<Waypoint> queue;
    
    public  List<Waypoint> path;
    List<Waypoint> explored = new List<Waypoint>();
    bool isRunning = true;
    bool firstLoadCompleted = false;
    Waypoint searchCenter;
    Waypoint[] waypoints;
    Vector2Int[] directions =
    {
        Vector2Int.up,
        Vector2Int.right,
        Vector2Int.down,
        Vector2Int.left
        
    };

    
    [SerializeField] Waypoint startWaypoint, endWaypoint;

    private void Start()
    {
        LoadBlocks();
        
    }

    

    public Waypoint GetClosestWaypoint(Waypoint startPoint)
    {
        Waypoint closestWaypoint = null;
        ResetPathFinder(startPoint, null);
        //Vector2Int startPointGridPos = startPoint.GetGridPos();
        

        foreach(Vector2Int direction in directions)
        {
            Vector2Int closestWaypointGridPos = startPoint.GetGridPos() + direction;
            if (grid[closestWaypointGridPos].walkable)
            {
                closestWaypoint = grid[closestWaypointGridPos];
            }
        }
        return closestWaypoint;
    }

    

    private void ResetPathFinder(Waypoint startPoint, Waypoint endPoint)
    {
        isRunning = true;

        path = new List<Waypoint>();
        queue = new Queue<Waypoint>();
        path.Clear();
        startWaypoint = startPoint;
        endWaypoint = endPoint;
        LoadBlocks();
    }
    public List<Waypoint> GetPath(Waypoint startPoint, Waypoint endPoint)
    {
        ResetPathFinder(startPoint, endPoint);
       
        /*
        startWaypoint.SetTopColor(Color.cyan);
        endWaypoint.SetTopColor(Color.red);
        */
        BreadthFirstSearch();
        ResetBlocks();
        return path;
    }

    private void ResetBlocks()
    {
        foreach(Waypoint waypoint in explored)
        {
            waypoint.isExplored = false;
           
        }
        explored.Clear();
    }

    private void BreadthFirstSearch()
    {
        queue.Enqueue(startWaypoint);

        while (queue.Count > 0 && isRunning)
        {
            searchCenter = queue.Dequeue();

            StopIfEndPointFound();
            ExploreNeighbours();
            searchCenter.isExplored = true;
            explored.Add(searchCenter);
        }

        GeneratePath();
    }

    private void GeneratePath()
    {
        path.Add(endWaypoint);

        Waypoint previous = endWaypoint.exploredFrom;

        while(previous != startWaypoint)
        {
            path.Add(previous);
            
            previous = previous.exploredFrom;
        }

        path.Add(startWaypoint);
        path.Reverse();
    }

    private void StopIfEndPointFound()
    {
        if (searchCenter.Equals(endWaypoint))
        {
            
            
            isRunning = false;

        }
    }

    private void ExploreNeighbours()
    {
        if (!isRunning) { return; }

        foreach(Vector2Int direction in directions)
        {
            Vector2Int neighbourCoordinates = searchCenter.GetGridPos() + direction;

            if(grid.ContainsKey(neighbourCoordinates))
            {
                QueueNewNeighbours(neighbourCoordinates);
            }
            
        }
    }

    private void QueueNewNeighbours(Vector2Int neighbourCoordinates)
    {
        Waypoint neighbour = grid[neighbourCoordinates];

        if (neighbour.isExplored || queue.Contains(neighbour))
        {

        }
        else
        {

            queue.Enqueue(neighbour);
            neighbour.exploredFrom = searchCenter;

        }
    }

    public void LoadBlocks()
    {
        waypoints = FindObjectsOfType<Waypoint>();
        foreach(Waypoint waypoint in waypoints)
        {
            if (grid.ContainsKey(waypoint.GetGridPos()))
            {
                if(!firstLoadCompleted)
                { 
                    Debug.Log("Duplicate at " + waypoint);
                    DestroyObject(waypoint.gameObject);
                    
                }
            }
            else if (!waypoint.walkable)
            {
                //Debug.LogWarning("Something is Here Already" + waypoint);
            }
            else
            {
                grid.Add(waypoint.GetGridPos(), waypoint);
            }
            waypoint.exploredFrom = null;
        }
        firstLoadCompleted = true;
    }

    

    public Waypoint MissedAction(Waypoint currentWaypoint, Waypoint waypointToMoveTo)
    {
        int[] directionsToChooseFrom = { 0, 2 };
        int directionChooser = UnityEngine.Random.Range(0,2);
        int directionIndicator = directionsToChooseFrom[directionChooser];
        int randomDistance = UnityEngine.Random.Range(1, 4);

        Waypoint tempWaypoint = currentWaypoint;
        var gridTemp = currentWaypoint.GetGridPos() + directions[directionIndicator];
        for (int i = 0; i < randomDistance; i++)
        {
            if (grid.ContainsKey(gridTemp))
            {
                tempWaypoint = grid[gridTemp];
            }
            else
            {
                break;
            }
            gridTemp = tempWaypoint.GetGridPos() + directions[directionIndicator];
            }
        return tempWaypoint;
    }

    public Waypoint MissedAction(Vector2Int waypointToLookUp,  Waypoint waypointToMoveTo)
    {
        int[] directionsToChooseFrom = { 0, 2 };
        int directionChooser = UnityEngine.Random.Range(0, 2);
        int directionIndicator = directionsToChooseFrom[directionChooser];
        int randomDistance = UnityEngine.Random.Range(1, 4);

        Waypoint tempWaypoint = grid[waypointToLookUp];
        var gridTemp = tempWaypoint.GetGridPos() + directions[directionIndicator];
        for (int i = 0; i < randomDistance; i++)
        {
            if (grid.ContainsKey(gridTemp))
            {
                tempWaypoint = grid[gridTemp];
            }
            else
            {
                break;
            }
            gridTemp = tempWaypoint.GetGridPos() + directions[directionIndicator];
        }
        return tempWaypoint;
    }

    // Update is called once per frame
    void Update () {
		
	}
}
