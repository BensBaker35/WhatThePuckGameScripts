using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
[SelectionBase]
[RequireComponent(typeof(Waypoint))]
public class CubeEditor : MonoBehaviour
{
   

    TextMesh textMesh;

    

    Waypoint waypoint;

    int gridSize;

    private void Awake()
    {

        waypoint = GetComponent<Waypoint>();
        gridSize = waypoint.GetGridSize();
    }

    void Update()
    {
        float xPos = waypoint.GetGridPos().x * gridSize;
        float zPos = waypoint.GetGridPos().y * gridSize;
        transform.position = new Vector3(
            xPos,
            0f,
            zPos
            );

        //textMesh = GetComponentInChildren<TextMesh>();

        
        string textLabel = xPos / gridSize + "," + zPos / gridSize;
        //textMesh.text = textLabel;
        gameObject.name = textLabel;
        
    }

}