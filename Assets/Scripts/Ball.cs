using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class Ball : MonoBehaviour
{
    public AudioSource impactAudio; // Reference to the AudioSource containing the impact sound effect
    public Rigidbody rb; // Reference to this gameObject's rigidbody
    public GameObject tableBoundsGroup; // Reference to the gameObject which is the parent of all table bounds objects
    public GameObject ballsGroup; // Reference to the gameObject which is the parent of all ball objects
    public int soundPriority; // Determines which ball gets to play an impact sound if two balls collide 
    public float forceStopThreshold = 2f; // Determines what a ball's velocity.sqrMagnitude needs to be to be manually stopped

    float velocityForMaxVolume; // Determines the relative velocity of a collision needed to play the impact sound at full volume

    protected virtual void Start()
    {
        // Get velocityForMaxVolume from the script attached to ballsGroup
        velocityForMaxVolume = ballsGroup.GetComponent<BallManager>().velocityForMaxVolume;
    }

    protected virtual void Update()
    {
        //Force-stop the ball if it's below a certain speed
        if(rb.velocity.sqrMagnitude <= forceStopThreshold && rb.velocity.sqrMagnitude > 0.0001f)
        {
            rb.velocity = Vector3.zero;
        }
    }

    protected virtual void OnCollisionEnter(Collision collision)
    {
        // Check whether we are colliding with the table bounds or another ball
        if(collision.collider.transform.IsChildOf(ballsGroup.transform))
            PlayImpactAudio(collision, true);
        else if(collision.collider.transform.IsChildOf(tableBoundsGroup.transform))
            PlayImpactAudio(collision, false);
    }

    protected void PlayImpactAudio(Collision collision, bool collidingWithBall)
    {
        // Make sure only the ball with higher soundPriority plays its impact sound
        if(collidingWithBall)
            if(collision.collider.GetComponent<Ball>().soundPriority > soundPriority)
                return;

        float velocity = collision.relativeVelocity.sqrMagnitude; // Using .sqrMagnitude as it's much faster than .magnitude
        float volume;

        if(velocity >= velocityForMaxVolume*velocityForMaxVolume)
        {
            // Collision velocity is equal to or greater the velocity we need for a max volume impact sound
            volume = 1f;
        }
        else if(velocity > 0)
        {
            // Volume scales linearly 
            volume = velocity / (velocityForMaxVolume*velocityForMaxVolume);
        }
        else
            return;

        impactAudio.volume = volume;
        impactAudio.Play();
    }
}
