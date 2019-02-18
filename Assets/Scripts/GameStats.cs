using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameStats
{
    // Class that stores game statistics
    
    public int score;
    public int shots;
    public float timer;

    public GameStats(int savedScore, int savedShots, float savedTimer)
    {
        score = savedScore;
        shots = savedShots;
        timer = savedTimer;
    }
}

