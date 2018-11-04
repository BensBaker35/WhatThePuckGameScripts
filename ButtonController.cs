using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

//Todo Fix score obejct to update correct panel
public class ButtonController : MonoBehaviour {
    private enum ButtonType
    {
        shoot,
        pass,
        block,
        bodyCheck,
        move,
        undo,
        nextTurn,
        submit
    }

    GameController gameController;
    [SerializeField] ButtonType buttonType;
    Button button;
    PlayerController currentPlayer = null;
    Waypoint currentWaypoint = null;


    private void Start()
    {
        gameController = FindObjectOfType<GameController>();
        button = GetComponent<Button>();
    }

    private void Update()
    {
      
        if ((gameController.selectedPlayer == null && (!buttonType.Equals(ButtonType.nextTurn))) && !buttonType.Equals(ButtonType.submit))
        {
            
            button.interactable = false;
        }
        else
        {
            
            currentPlayer = gameController.selectedPlayer;
            currentWaypoint = gameController.selectedWaypoint;

            switch (buttonType)
            {

                case ButtonType.shoot:
                    ShootButtonCheck();
                    break;
                case ButtonType.pass:
                    PassButtonCheck();
                    break;
                case ButtonType.block:
                    BlockCheck();
                    break;
                case ButtonType.bodyCheck:
                    BodyHitCheck();
                    break;
                case ButtonType.move:
                    MoveButtonCheck();
                    break;
                case ButtonType.undo:
                    UndoButtonCheck();
                    break;
                case ButtonType.nextTurn:

                    break;
                case ButtonType.submit:
                    FaceOffButtonCheck();
                    break;
                

            }

        }

    }
  

    private void FaceOffButtonCheck()
    {
        if (gameController.gameState.Equals(GameState.faceoff) || gameController.gameState.Equals(GameState.faceOffSecondary))
        {
            button.interactable = true;

        }
        else
        {
            button.interactable = false;
        }
    }

    private void BodyHitCheck()
    {
        if(currentPlayer != null && currentWaypoint != null)
        {
            if (!currentPlayer.HasPuck && !currentPlayer.HasBeenBodyChecked)
            {
                button.interactable = true;
            }
            else
            {
                button.interactable = false;
            }
        }
        else
        {
            button.interactable = false;
        }
    }

    private void UndoButtonCheck()
    {
       if(gameController.gameState.Equals(GameState.chooseActions) || gameController.gameState.Equals(GameState.pickWaypoint))
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    private void MoveButtonCheck()
    {
        if(currentPlayer != null && currentWaypoint != null)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    private void BlockCheck()
    {
        if(currentPlayer != null && !currentPlayer.HasPuck)
        {
            button.interactable = true;
        }
        else
        {
            button.interactable = false;
        }
    }

    private void ShootButtonCheck()
    {
        if (!currentPlayer.Equals(null))
        {
            if(currentPlayer.PlayerGridPos().x > 15 || currentPlayer.PlayerGridPos().x < 3)
            {
                button.interactable = false;
            }
            else
            {
                if (currentPlayer.HasPuck)
                {
                    button.interactable = true;
                }
                else
                {
                    button.interactable = false;

                }
            }
            
        }
        else
        {
            button.interactable = false;
        }
    }

    private void PassButtonCheck()
    {
        if (!currentPlayer.Equals(null) && currentWaypoint != null)
        {
            if(currentWaypoint.PlayerCovering != null)
            {
                if (currentPlayer.HasPuck && !currentWaypoint.PlayerCovering.hasCompletedAction)
                {

                    button.interactable = true;

                }
                else
                {
                    button.interactable = false;
                }
            }
            else
            {
                button.interactable = false;
            }
           
        }
        else
        {
            button.interactable = false;
        }
    }
}


