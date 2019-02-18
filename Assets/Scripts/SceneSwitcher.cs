using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    // Simple class to handle scene switching and application quitting
    // Functions are called by the relevant button clicks
    
    public void SwitchToMainScene()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainGame");
    }
    
    public void SwitchToMainMenu()
    {
        Time.timeScale = 1;
        SceneManager.LoadScene("MainMenu");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
