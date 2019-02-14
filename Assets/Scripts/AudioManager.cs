using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public AudioMixer mainMixer;
    public AudioSource demoSound;

    public void setVolume(float volume) {
        mainMixer.SetFloat("masterVolume", Mathf.Log(volume)*20);
        playDemoSound();
    }

    public void playDemoSound() {
        if (!demoSound.isPlaying) {
            demoSound.Play();
        }
    }
}
