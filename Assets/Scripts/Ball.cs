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
            PlayImpactAudio(collision.collider,true);
        }
        else if(collision.collider.transform.IsChildOf(tableBoundsGroup.transform)) {
            PlayImpactAudio(collision.collider,false);
            
        }
    }

    void PlayImpactAudio(Collider collider, bool collidingWithRigidbody) {
        //TODO: fix "doubling" effect, use sqrtMagnitude, fix inconsistent velocities
        float velocity;
        bool playOnCollider = false;
        float volume;
        if(collidingWithRigidbody) {
            velocity = Math.Max(rb.velocity.magnitude, collider.GetComponent<Rigidbody>().velocity.magnitude);
            playOnCollider = true;
            print("Velocity is " + velocity.ToString());
        }
        else {
            velocity = rb.velocity.magnitude;
            print("Collider is static, so we're using local velocity at " + velocity.ToString());
        }
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


        if(playOnCollider) {
            collider.GetComponent<Ball>().impactAudio.volume = volume;
            print("Calling " + collider.ToString() + "'s AudioSource from " + gameObject.ToString());
            collider.GetComponent<Ball>().impactAudio.Play();
        }

        else {
            impactAudio.volume = volume;
            print("Playing AudioSource locally at " + gameObject.ToString());
            impactAudio.Play();
        }
    }
}
