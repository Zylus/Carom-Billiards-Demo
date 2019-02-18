using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public GameObject masterVolumeSlider; // Reference to the slider controlling master volume
    public GameObject aimAssistCheckbox; // Reference to the checkbox controlling aim assistance
    public GameObject hypeModeCheckbox; // Reference to the checkbox controlling hype mode
    public AudioMixer mainMixer; // Reference to the main AudioMixer
    public AudioSource demoSound; // Reference to the AudioSource containing the demo sound to play in the options menu

    GameObject aimAssist; // Reference to the GameObject that displays the aim assist line
    DataManager dataManager; // Reference to the Data Manager
    PlayerBall playerBallScript; // Reference to the script attached to the player ball object
    bool aimAssistActive = true; // Determines whether aim assistance is displayed
    bool hypeModeAllowed = true; // Determines whether Hype Mode is allowed (zooming and sound effects while the player ball is approaching the second ball needed to hit)
    float volume = 1f; // Master volume
    string currentScene; // Name of the current scene
    string settingsFilePath = "settings.json"; // Path to be appended to the Data Manager's base path
    bool duringSetup; // Determines whether the options are still being set up

    void Start()
    {
        duringSetup = true;
        dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        // Read settings from file
        if(dataManager.FileExists(settingsFilePath))
        {
            LoadSettingsFromFile(settingsFilePath);
        }
        currentScene = SceneManager.GetActiveScene().name;
        if(currentScene == "MainGame")
        {
            // Assign references
            aimAssist = GameObject.Find("DirectionIndicator");
            playerBallScript = GameObject.Find("PlayerBall").GetComponent<PlayerBall>();
            ApplySettings();
        }
        // Disable the options menu so it doesn't show up for the player
        gameObject.SetActive(false);
        duringSetup = false;
    }

    public void LoadSettingsFromFile(string path)
    {
        // Read settings
        string data = dataManager.ReadFromFile(path);
        PlayerSettings loadedSettings = JsonUtility.FromJson<PlayerSettings>(data);

        // Set variables
        aimAssistActive = loadedSettings.aimAssistActive;
        volume = loadedSettings.volume;
        hypeModeAllowed = loadedSettings.hypeModeAllowed;

        // Change UI elements to display the correct settings
        aimAssistCheckbox.GetComponent<Toggle>().isOn = aimAssistActive;
        masterVolumeSlider.GetComponent<Slider>().value = volume;
        hypeModeCheckbox.GetComponent<Toggle>().isOn = hypeModeAllowed;
    }

    public void ToggleOptionsMenu()
    {
        // Toggles active state of the Options Menu
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            // Stores current settings whenever the Options Menu is closed
            WriteSettingsToFile(settingsFilePath);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    void WriteSettingsToFile(string path)
    {
        // Create a PlayerSettings object with current settings, then save it
        PlayerSettings currentSettings = new PlayerSettings(aimAssistActive, volume, hypeModeAllowed);
        dataManager.WriteToFile(path,JsonUtility.ToJson(currentSettings));
    }

    public void ApplySettings()
    {
        if(currentScene == "MainGame")
        {
            // Activate the aim assist object if the player has enabled it
            aimAssist.SetActive(aimAssistActive);
            // Set the bool in the player ball script that controls Hype Mode
            playerBallScript.hypeModeAllowed = hypeModeAllowed;
        }
    }

    public void ToggleAimAssist() // Called by clicking the checkbox
    {
        if(!duringSetup) // This prevents the function from executing when we set aim assist manually during the setup phase
        {
            aimAssistActive = !aimAssistActive;
            ApplySettings();
        }
    }

    public void ToggleHypeAllowed() // Called by clicking the checkbox
    {
        if(!duringSetup)
        {
            hypeModeAllowed = !hypeModeAllowed;
            ApplySettings();
        }
    }

    public void SetVolume(float volume) // Called by changing the value of the slider
    {
        mainMixer.SetFloat("masterVolume", Mathf.Log(volume)*20);
        this.volume = volume;
        PlayDemoSound();
    }

    public void PlayDemoSound()
    {
        // Play sound effect while slider is moved so the effects of changing volume can be observed
        if (!demoSound.isPlaying && gameObject.activeSelf)
            demoSound.Play();
    }
}
