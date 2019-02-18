using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SaveState
{
    // Class that stores a certain game state for the purposes of Replay Mode

    public List<SavedBall> savedBalls; // List of all balls
    public float force; // Force that the player ball was shot with
    // Camera values are saved as well so it works during Replay
    public Vector3 cameraPosition;
    public Vector3 cameraAngle;
    public Vector3 cameraOffset;

    public SaveState()
    {
        savedBalls = new List<SavedBall>();
    }

    public void SaveBall(SavedBall ball)
    {
        // "Saves" a single ball by adding it to the list of savedBalls
        savedBalls.Add(ball);
    }

    public SavedBall GetSavedBall(string name)
    {
        // Returns a single saved ball by name
        return savedBalls.Find(item => item.name == name);
    }
}

public class SavedBall
{
    // Class that represents a single ball's save state
    
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
