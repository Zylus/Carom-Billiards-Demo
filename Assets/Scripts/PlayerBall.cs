using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBall : Ball
{
    public GameObject mainCamera; // Reference to the camera centered on the player's ball;
    public GameObject forceBar;
    public LineRenderer lr;
    float force;
    int forceModifier = 300;
    bool isCharging = false;
    bool chargeReverse = false;
    bool duringShoot = false;
    List<Rigidbody> ballBodies = new List<Rigidbody>();
    List<GameObject> targetBalls = new List<GameObject>();
    List<GameObject> ballsHitThisShot = new List<GameObject>();
    int score = 0;
    bool scoredThisShot = false;
    public Text scoreText;

    protected override void Start()
    {
        base.Start();
        foreach(Transform ball in ballsGroup.transform)
        {
            ballBodies.Add(ball.gameObject.GetComponent<Rigidbody>());
            if(ball.gameObject != gameObject)
                targetBalls.Add(ball.gameObject);
        }
    }

    protected override void Update()
    {
        base.Update();
        checkObjective();
        checkBallStatus();
        

        // Input handling
        if (Input.GetKey("space"))
        {
            if(!duringShoot)
                chargeForce();
        }
        if (Input.GetKeyUp("space"))
        {
            if(!duringShoot)
                fireBall();
        }

        updateAimAssistLine();
        fadeOutForceBar();        
    }

    void checkBallStatus()
    {
        if(duringShoot && !forceBar.activeSelf)
        {
            bool everyBallStopped = true;
            foreach(Rigidbody body in ballBodies)
            {
                if(body.velocity != Vector3.zero)
                {
                    everyBallStopped = false;
                    break;
                }
            }
            if(everyBallStopped)
            {
                duringShoot = false;
                ballsHitThisShot.Clear();
                scoredThisShot = false;
            }
                
            
        }
    }

    void checkObjective() {
        if(!scoredThisShot && duringShoot)
        {
            bool objectiveReached = true;
            foreach(GameObject target in targetBalls)
            {
                if(!ballsHitThisShot.Contains(target))
                {
                    objectiveReached = false;
                }
            }
            if(objectiveReached)
            {
                score++;
                scoreText.text = "Score: " + score.ToString();
                scoredThisShot = true;
            }
        }
        
    }

    void updateAimAssistLine()
    {
        RaycastHit hit;
        Vector3 position = transform.position;
        Vector3 direction = transform.forward;

        if(Physics.Raycast(position, direction, out hit, Mathf.Infinity))
        {
            lr.SetPosition(1,transform.InverseTransformPoint(hit.point));
        }
    }

    void fadeOutForceBar() {
        if(!isCharging && forceBar.activeSelf)
        {
            forceBar.GetComponent<CanvasGroup>().alpha -= Time.deltaTime / 1f;
            if(forceBar.GetComponent<CanvasGroup>().alpha <= 0) {
                forceBar.SetActive(false);
                forceBar.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }

    void chargeForce()
    {
        // Start charging force
            if(!isCharging)
            {
                force = 0.01f;
                isCharging = true;
                forceBar.SetActive(true);
            }
            else
            {
                if(!chargeReverse)
                {
                    if(force < 1f)
                    {
                        force += .8f * Time.deltaTime;
                    }
                    else
                    {
                        chargeReverse = true;
                    }
                }
                else
                {
                    if(force > 0.01f)
                    {
                        force -= .8f * Time.deltaTime;
                    }
                    else
                    {
                        chargeReverse = false;
                    }
                }
            }
            forceBar.GetComponent<Slider>().value = force;
            forceBar.transform.Find("Text").GetComponent<Text>().text = ((int)(force*100)).ToString() + "%";
    }

    void fireBall()
    {
        rb.AddForce(transform.forward * force * forceModifier,ForceMode.Impulse);
        isCharging = false;
        duringShoot = true;
    }

    public void toggleAimAssist() {
        if(lr.gameObject.activeSelf)
            lr.gameObject.SetActive(false);
        else
            lr.gameObject.SetActive(true);
    }

    protected override void OnCollisionEnter(Collision collision) {
        if(collision.collider.transform.IsChildOf(ballsGroup.transform))
        {
            if(targetBalls.Contains(collision.collider.gameObject) && !ballsHitThisShot.Contains(collision.collider.gameObject))
            {
                ballsHitThisShot.Add(collision.collider.gameObject);
            }
            PlayImpactAudio(collision, true);
        }
        else if(collision.collider.transform.IsChildOf(tableBoundsGroup.transform))
            PlayImpactAudio(collision, false);
    }
}
