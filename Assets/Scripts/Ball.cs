using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Ball : MonoBehaviour
{

    public AudioSource impactAudio;
    public Rigidbody rb;
    public GameObject tableBoundsGroup;
    public GameObject ballsGroup;
    
    float velocityForMaxVolume;

    void Start() {
        velocityForMaxVolume = ballsGroup.GetComponent<BallManager>().velocityForMaxVolume;
    }

    void OnCollisionEnter(Collision collision) {
        print(gameObject.ToString() + " is colliding with " + collision.collider);
        if(collision.collider.transform.IsChildOf(ballsGroup.transform)) {
            PlayImpactAudio(collision);
        }
        else if(collision.collider.transform.IsChildOf(tableBoundsGroup.transform)) {
            PlayImpactAudio(collision);
            
        }
    }

    void PlayImpactAudio(Collision collision) {
        //TODO: fix "doubling" effect, use sqrtMagnitude
        float velocity;
        float volume;
        velocity = collision.relativeVelocity.magnitude;
        print("velocity: " + velocity.ToString());
        if(velocity >= velocityForMaxVolume) {
            print("Volume: MAX");
            volume = 1f;
            impactAudio.Play();
        }
        else if(velocity > 0) {
            volume = velocity / velocityForMaxVolume;
            print("Volume: " + volume.ToString());
            impactAudio.Play();
        }
        else {
            return;
        }

        impactAudio.volume = volume;
        impactAudio.Play();
    }
}
