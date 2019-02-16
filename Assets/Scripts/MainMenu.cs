using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour
{
    public Text lastGameStats;
    DataManager dataManager;

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
