using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mainMixer; // Reference to the main AudioMixer
    public AudioSource demoSound; // Reference to the AudioSource containing the demo sound to play in the main menu

    public void setVolume(float volume)
    {
        mainMixer.SetFloat("masterVolume", Mathf.Log(volume)*20);
        playDemoSound();
    }

    public void playDemoSound()
    {
        // Play sound effect while slider is moved so the effects of changing volume can be observed
        if (!demoSound.isPlaying)
            demoSound.Play();
    }
}
