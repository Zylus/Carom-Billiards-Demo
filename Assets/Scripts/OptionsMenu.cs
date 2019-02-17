using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    GameObject aimAssist;
    bool aimAssistActive = true;
    bool hypeModeActive = true;
    float volume = 1f;
    string currentScene;
    public AudioMixer mainMixer; // Reference to the main AudioMixer
    public AudioSource demoSound; // Reference to the AudioSource containing the demo sound to play in the options menu
    DataManager dataManager;
    string settingsFilePath;
    public GameObject masterVolumeSlider;
    public GameObject aimAssistCheckbox;
    public GameObject hypeModeCheckbox;
    bool duringSetup;

    void Start()
    {
        duringSetup = true;
        dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
        settingsFilePath = dataManager.baseFilePath + "settings.json";
        // read settings from file and initialise UI
        if(dataManager.FileExists(settingsFilePath))
        {
            LoadSettingsFromFile(settingsFilePath);
        }
        currentScene = SceneManager.GetActiveScene().name;
        if(currentScene == "MainGame")
        {
            aimAssist = GameObject.Find("DirectionIndicator");
            ApplySettings();
        }
        gameObject.SetActive(false);
        duringSetup = false;
    }

    public void LoadSettingsFromFile(string path)
    {
        string data = dataManager.ReadFromFile(path);
        PlayerSettings loadedSettings = JsonUtility.FromJson<PlayerSettings>(data);

        aimAssistActive = loadedSettings.aimAssistActive;
        volume = loadedSettings.volume;
        hypeModeActive = loadedSettings.hypeModeAllowed;
        aimAssistCheckbox.GetComponent<Toggle>().isOn = aimAssistActive;
        masterVolumeSlider.GetComponent<Slider>().value = volume;
        hypeModeCheckbox.GetComponent<Toggle>().isOn = hypeModeActive;
    }

    public void ToggleOptionsMenu()
    {
        if(gameObject.activeSelf)
        {
            gameObject.SetActive(false);
            WriteSettingsToFile(settingsFilePath);
        }
        else
        {
            gameObject.SetActive(true);
        }
    }

    void WriteSettingsToFile(string path)
    {
        PlayerSettings currentSettings = new PlayerSettings(aimAssistActive, volume, hypeModeActive);
        dataManager.WriteToFile(path,JsonUtility.ToJson(currentSettings));
    }

    public void ApplySettings() {
        if(currentScene == "MainGame")
        {
            if(aimAssistActive)
            {
                aimAssist.SetActive(true);
            }
            else
            {
                aimAssist.SetActive(false);
            }
        }


    }

    public void ToggleAimAssist()
    {
        if(!duringSetup)
        {
            aimAssistActive = !aimAssistActive;
            ApplySettings();
        }
    }

    public void ToggleHypeAllowed()
    {
        if(!duringSetup)
            hypeModeActive = !hypeModeActive;
    }

    public void SetVolume(float volume)
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
