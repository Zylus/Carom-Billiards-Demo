using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    public GameObject playerBall; // Reference to the player's ball
    
    private Vector3 offset; // Vector distance between camera object and player's ball
    private int rotationRate = 50;

    void Start()
    {
        // Initialise offset to be based on the positions of the objects in the editor
        offset = transform.position - playerBall.transform.position;
    }

    void LateUpdate()
    {
        // Move the camera with the player's ball
        transform.position = playerBall.transform.position + offset;

        // Handle input for camera movement
        if(Input.GetKey("a"))
        {
            // Turn camera left
            transform.RotateAround(playerBall.transform.position, new Vector3(0,-1,0), rotationRate * Time.deltaTime);
            offset = transform.position - playerBall.transform.position;
            playerBall.transform.Rotate(0, -rotationRate * Time.deltaTime,0);
        }
        else if(Input.GetKey("d"))
        {
            // Turn camera right
            transform.RotateAround(playerBall.transform.position, new Vector3(0,1,0), rotationRate * Time.deltaTime);
            offset = transform.position - playerBall.transform.position;
            playerBall.transform.Rotate(0, rotationRate * Time.deltaTime,0);
        }


        else if(Input.GetMouseButton(1))
        {
            // Turn camera based on mouse movement while RMB is held down
            float yaw = Input.GetAxis("Mouse X");
            transform.RotateAround(playerBall.transform.position, new Vector3(0, yaw, 0), Mathf.Abs(yaw) * rotationRate * Time.deltaTime);
            offset = transform.position - playerBall.transform.position;
            playerBall.transform.Rotate(0, yaw * rotationRate * Time.deltaTime,0);
        }

        
    }
}
