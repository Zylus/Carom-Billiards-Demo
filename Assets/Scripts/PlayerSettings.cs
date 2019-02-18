using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerSettings
{
    // Class that stores settings
    
    public bool aimAssistActive = true;
    public float volume = 1f;
    public bool hypeModeAllowed = true;

    public PlayerSettings(bool currentAimAssist, float currentVolume, bool currentHype)
    {
        aimAssistActive = currentAimAssist;
        volume = currentVolume;
        hypeModeAllowed = currentHype;
    }
}
