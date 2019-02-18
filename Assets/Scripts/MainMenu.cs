using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text lastGameStats; // Reference to the text that displays last game's stats
    DataManager dataManager; // Reference to the DataManager

    void Start()
    {
        dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        GetLastGameStats();
    }

    void GetLastGameStats()
    {
        string path = "lastgame.json";
        if(dataManager.FileExists(path))
        {
            string data = dataManager.ReadFromFile(path);
            GameStats loadedStats = JsonUtility.FromJson<GameStats>(data);
            
            // Set the displayed text based on the loaded stats
            string minutes = Mathf.Floor(loadedStats.timer / 60).ToString("00");
            string seconds = Mathf.Floor(loadedStats.timer % 60).ToString("00");
            lastGameStats.text = "Last game:\nPoints scored: " + loadedStats.score + " | Shots taken: " + loadedStats.shots + " | " + minutes + ":" + seconds + " spent";
        }
        else
        {
            lastGameStats.text = "No previous game found.";
        }
    }

}
