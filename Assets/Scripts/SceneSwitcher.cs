using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSwitcher : MonoBehaviour
{
    public void switchToMainScene() {
        SceneManager.LoadScene("MainGame");
    }
}
