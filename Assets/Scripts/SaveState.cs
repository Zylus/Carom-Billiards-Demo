using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveState
{
    public List<SavedBall> savedBalls;
    public float force;
    public Vector3 cameraPosition;
    public Vector3 cameraAngle;
    public Vector3 cameraOffset;


    public SaveState()
    {
        savedBalls = new List<SavedBall>();
    }

    public void SaveBall(SavedBall ball)
    {
        savedBalls.Add(ball);
    }

    public SavedBall GetSavedBall(string name)
    {
        return savedBalls.Find(item => item.name == name);
    }
}

public class SavedBall
{
    public string name;
    public Vector3 position;
    public Vector3 eulerAngles;

    public SavedBall(string n, Vector3 p, Vector3 e)
    {
        name = n;
        position = p;
        eulerAngles = e;
    }
}
