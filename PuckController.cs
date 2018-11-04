using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuckController : MonoBehaviour {

    GameController gameController;
    Pathfinder pathfinder;

    Waypoint[] team1GoalPosts;
    Waypoint[] team2GoalPosts;

    private Waypoint coveringWaypoint;
    public Waypoint CoveringWaypoint
    {
        get { return coveringWaypoint; }
        set { coveringWaypoint = value; }
        
    }
    

    private PlayerController playerHolding = null;
    public PlayerController PlayerHolding
    {
        get { return playerHolding; }
        set { playerHolding = value; }
    }
    
    private bool canBePickedUp = true;
    public bool CanBePickedUp
    {
        get { return canBePickedUp; }
        set { canBePickedUp = value; }
    }

    bool waitToFollow = false;
    
    // Use this for initialization
    void Start () {
        GoalWaypointLocation goalWaypointLocation = FindObjectOfType<GoalWaypointLocation>();
        team1GoalPosts = goalWaypointLocation.team1GoalPosts;
        team2GoalPosts = goalWaypointLocation.team2GoalPosts;
        gameController = FindObjectOfType<GameController>() ;
        pathfinder = FindObjectOfType<Pathfinder>();

        
	}
	
	// Update is called once per frame
	void Update ()
    {
        if (playerHolding != null )
        {
            MatchPositionWithPlayer();
            Debug.Log(String.Format("Player {0} has puck = {1}", playerHolding, playerHolding.HasPuck));
        }
        else
        {
            if (!waitToFollow)
            {
                canBePickedUp = true;
            }
            
          
        }

        

	}

    private void MatchPositionWithPlayer()
    {

        if (coveringWaypoint != playerHolding.wayPointCovering && !waitToFollow)
        {
            FollowPlayer();
                  
        }

    }

    public IEnumerator MovePuck(Waypoint targetWaypoint,Waypoint startingWaypoint, bool passOverGoal = true)
    {
        
        Waypoint team1Goal = team1GoalPosts[1];
        Waypoint team2Goal = team2GoalPosts[2];

        //Returns true if shot is not blocked
        List<Waypoint> path = pathfinder.GetPath(startingWaypoint, targetWaypoint);
        playerHolding = null;
        bool actionBlocked = false;
        for (int i = 0; i < path.Count; i++)
        {
            Waypoint waypoint = path[i];
           
            if (!passOverGoal && (waypoint.Equals(team1Goal) || waypoint.Equals(team2Goal)))
            {
                path.Remove(waypoint);
                i--;
            }
            if (waypoint.PlayerCovering != null)
            {
                actionBlocked = WaypointBlocking(waypoint);
            }
            bool hitGoalPost = CheckGoalPosts(waypoint);
            if (!hitGoalPost)
            {
                transform.position = new Vector3(waypoint.transform.position.x, transform.position.y, waypoint.transform.position.z );
                //SetPuckFacing();
            }
            else
            {
                playerHolding = null;
                break;
            }
           
            if (actionBlocked)
            {
                ChangePlayers(waypoint.PlayerCovering);
                actionBlocked = true;
                break;
            }
            
            yield return new WaitForSeconds(.25f);
        }
        if(coveringWaypoint.PlayerCovering != null)
        {
            playerHolding = coveringWaypoint.PlayerCovering;
            SetPuckFacing();
        }
        gameController.actionCompleted = true;
        gameController.actionBlocked = actionBlocked;
        waitToFollow = false;

    }
    /*
    public IEnumerator MovePuck(List<Waypoint> path)
    {
        playerHolding = null;

        foreach(Waypoint waypoint in path)
        {
            transform.position = new Vector3(waypoint.transform.position.x, transform.position.y, waypoint.transform.position.z);
            SetPuckFacing();

            yield return new WaitForSeconds(.25f);
                                    
        }
        gameController.actionCompleted = true;

    }
    */
    private bool WaypointBlocking(Waypoint waypoint)
    {
        if (waypoint.PlayerCovering.IsBlocking)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void FollowPlayer()
    {
        transform.position = new Vector3(playerHolding.transform.position.x,
                playerHolding.transform.position.y - 1f,
                playerHolding.transform.position.z);
        SetPuckFacing();
    }
    private bool CheckGoalPosts(Waypoint waypoint)
    {
        if(waypoint.Equals(team1GoalPosts[0]) || waypoint.Equals(team1GoalPosts[2]) ||
            waypoint.Equals(team2GoalPosts[0])  || waypoint.Equals(team2GoalPosts[2]))
        {
            return true;
        }
        return false;
    }

    public void SetPuckFacing()
    {
        
        if (true) //playerHolding.wayPointCovering.Equals(coveringWaypoint)
        {
            if (playerHolding.tag.EndsWith("2"))
            {
                transform.position = new Vector3(playerHolding.transform.position.x - .750f,
                playerHolding.transform.position.y - 1f,
                playerHolding.transform.position.z);
            }
            else
            {
                transform.position = new Vector3(playerHolding.transform.position.x + .750f,
                playerHolding.transform.position.y - 1f,
                playerHolding.transform.position.z);
            }
        }

    }

    public void ChangePlayers(PlayerController playerController, bool waitToMatch = false)
    {
        waitToFollow = waitToMatch;
        if(playerHolding != null)
        {
            playerHolding.HasPuck = false;
        }
        
        //playerController.HasPuck = true;

        playerHolding = playerController;
        playerHolding.HasPuck = true;
        Debug.Log(playerController.HasPuck);
        
        //SetPuckFacing();
    }
}
