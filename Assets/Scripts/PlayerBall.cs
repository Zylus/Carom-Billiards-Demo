using System.Collections;
using System.Collections.Generic;
using System.Linq;
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
    bool replayingShot = false;
    List<Rigidbody> ballBodies = new List<Rigidbody>();
    List<GameObject> targetBalls = new List<GameObject>();
    List<GameObject> ballsHitThisShot = new List<GameObject>();
    List<GameObject> ballsRemaining = new List<GameObject>();
    GameObject finalBallMissing;
    int score = 0;
    int shots = 0;
    float timer = 0f;
    int maxScore = 3;
    string minutes;
    string seconds;
    bool scoredThisShot = false;
    public Text scoreText;
    public Text shotText;
    public Text timeSpentText;
    public GameObject gameWinMenu;
    public GameObject pauseMenu;
    public GameObject optionsMenu;
    public GameObject ballsHitPanel;
    DataManager dataManager;
    public Button replayButton;
    SaveState lastSave;
    public AudioSource scoreSound;
    bool hypeMode = false;
    Vector3 tempCameraOffset;
    Vector3 tempCameraPosition;
    Vector3 tempCameraRotation;

    protected override void Start()
    {
        base.Start();
        foreach(Transform ball in ballsGroup.transform)
        {
            ballBodies.Add(ball.gameObject.GetComponent<Rigidbody>());
            if(ball.gameObject != gameObject)
                targetBalls.Add(ball.gameObject);
        }
        ballsHitPanel.transform.GetChild(0).GetComponent<Image>().color = Color.grey;
        ballsHitPanel.transform.GetChild(1).GetComponent<Image>().color = Color.grey;
        dataManager = GameObject.Find("DataManager").GetComponent<DataManager>();
    }

    protected override void Update()
    {
        base.Update();
        UpdateTimer();
        CheckObjective();
        CheckBallStatus();
        HypeMode();

        // Input handling
        if(Input.GetKey("space"))
        {
            if(!duringShoot && !replayingShot)
                ChargeForce();
        }
        if(Input.GetKeyUp("space"))
        {
            if(!duringShoot && !replayingShot)
                FireBall();
        }
        if(Input.GetKeyDown("escape"))
        {
            if(!optionsMenu.activeSelf)
                TogglePauseMenu();
        }

        UpdateAimAssistLine();
        FadeOutForceBar();        
    }

    void HypeMode()
    {
        if(duringShoot && !scoredThisShot)
        {
            if(finalBallMissing && rb.velocity != Vector3.zero && Vector3.Distance(transform.position, finalBallMissing.transform.position) < 20)
            {
                if(!hypeMode)
                {
                    tempCameraOffset = mainCamera.GetComponent<CameraController>().offset;
                    tempCameraPosition = mainCamera.transform.position;
                    tempCameraRotation = mainCamera.transform.eulerAngles;
                    hypeMode = true;
                }
                Vector3 newOffset = new Vector3();
                if(Vector3.Distance(mainCamera.transform.position, transform.position) > 30)
                {
                    newOffset = Vector3.MoveTowards(mainCamera.transform.position, transform.position, 30 * Time.deltaTime) - transform.position;
                    mainCamera.GetComponent<CameraController>().offset = newOffset;
                }
            }
            else
            {
                if(hypeMode)
                {
                    ResetHype();
                }
            }
        }
        else
        {
            if(hypeMode)
            {
                ResetHype();
            }
        }
    }

    void ResetHype()
    {
        mainCamera.transform.position = tempCameraPosition;
        mainCamera.GetComponent<CameraController>().offset = tempCameraOffset;
        mainCamera.transform.eulerAngles = tempCameraRotation;
        hypeMode = false;
    }

    public void ReplayLastShot()
    {
        if(lastSave != null)
        {
            replayButton.interactable = false;
            replayingShot = true;
            foreach(Transform ball in ballsGroup.transform)
            {
                SavedBall correctBall = lastSave.GetSavedBall(ball.gameObject.name);
                ball.position = correctBall.position;
                ball.eulerAngles = correctBall.eulerAngles;
            }
            force = lastSave.force;
            forceBar.GetComponent<Slider>().value = force;
            forceBar.transform.Find("Text").GetComponent<Text>().text = ((int)(force*100)).ToString() + "%";
            forceBar.SetActive(true);
            mainCamera.transform.position = lastSave.cameraPosition;
            mainCamera.transform.eulerAngles = lastSave.cameraAngle;
            mainCamera.GetComponent<CameraController>().offset = lastSave.cameraOffset;
            FireBall();
        }
        
    }

    void UpdateTimer()
    {
        timer += Time.deltaTime;
        minutes = Mathf.Floor(timer / 60).ToString("00");
        seconds = Mathf.Floor(timer % 60).ToString("00");
        timeSpentText.text = minutes + ":" + seconds;
    }

    void CheckBallStatus()
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
                ballsHitPanel.transform.GetChild(0).GetComponent<Image>().color = Color.grey;
                ballsHitPanel.transform.GetChild(1).GetComponent<Image>().color = Color.grey;
                duringShoot = false;
                ballsHitThisShot.Clear();
                scoredThisShot = false;
                if(replayingShot)
                {
                    replayingShot = false;
                    
                }
                replayButton.interactable = true;
                ballsRemaining.Clear();
                finalBallMissing = null;
            }
                
            
        }
    }

    void CheckObjective() {
        if(!scoredThisShot && duringShoot && !replayingShot)
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
                scoreSound.Play();
                score++;
                scoreText.text = "Score: " + score.ToString();
                scoredThisShot = true;
                if(score >= maxScore)
                {
                    GameWon();
                }
            }
        }
        
    }

    void GameWon()
    {
        gameWinMenu.transform.Find("StatsText").GetComponent<Text>().text = "Duration: " + minutes + ":" + seconds + "\nShots taken: " + shots.ToString();
        gameWinMenu.SetActive(true);
        Time.timeScale = 0;
        GameStats currentStats = new GameStats(score,shots,timer);
        dataManager.WriteToFile("lastgame.json",JsonUtility.ToJson(currentStats));
    }

    void UpdateAimAssistLine()
    {
        RaycastHit hit;
        Vector3 position = transform.position;
        Vector3 direction = transform.forward;

        if(Physics.Raycast(position, direction, out hit, Mathf.Infinity))
        {
            lr.SetPosition(1,transform.InverseTransformPoint(hit.point));
        }
    }

    void FadeOutForceBar() {
        if(!isCharging && forceBar.activeSelf)
        {
            forceBar.GetComponent<CanvasGroup>().alpha -= Time.deltaTime / 1f;
            if(forceBar.GetComponent<CanvasGroup>().alpha <= 0) {
                forceBar.SetActive(false);
                forceBar.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }

    void ChargeForce()
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

    void FireBall()
    {
        if(!replayingShot)
        {
            shots++;
            shotText.text = "Shots: " + shots.ToString();
            SaveState(ballsGroup, force);
            replayButton.interactable = false;
        }
        isCharging = false;
        duringShoot = true;
        rb.AddForce(transform.forward * force * forceModifier,ForceMode.Impulse);
    }

    void SaveState(GameObject ballsContainer, float force)
    {
        lastSave = new SaveState();
        foreach(Transform ball in ballsContainer.transform)
        {
            SavedBall ballToSave = new SavedBall(ball.gameObject.name, ball.position, ball.eulerAngles);
            lastSave.SaveBall(ballToSave);
        }
        lastSave.force = force;
        lastSave.cameraPosition = mainCamera.transform.position;
        lastSave.cameraAngle = mainCamera.transform.eulerAngles;
        lastSave.cameraOffset = mainCamera.GetComponent<CameraController>().offset;
    }

    protected override void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.transform.IsChildOf(ballsGroup.transform))
        {
            if(targetBalls.Contains(collision.collider.gameObject) && !ballsHitThisShot.Contains(collision.collider.gameObject))
            {
                ballsHitThisShot.Add(collision.collider.gameObject);
                ballsRemaining = targetBalls.Except(ballsHitThisShot).ToList();
                if(ballsRemaining.Count() == 1)
                {
                    finalBallMissing = ballsRemaining[0];
                }
                if(collision.collider.gameObject.name == "YellowBall")
                {
                    ballsHitPanel.transform.GetChild(0).GetComponent<Image>().color = Color.yellow;
                }
                else if(collision.collider.gameObject.name == "RedBall")
                {
                    ballsHitPanel.transform.GetChild(1).GetComponent<Image>().color = Color.red;
                }
            }
            PlayImpactAudio(collision, true);
        }
        else if(collision.collider.transform.IsChildOf(tableBoundsGroup.transform))
            PlayImpactAudio(collision, false);
    }

    void TogglePauseMenu()
    {
        if(pauseMenu.gameObject.activeSelf)
        {
            pauseMenu.gameObject.SetActive(false);
            Time.timeScale = 1;
        }
        else
        {
            pauseMenu.gameObject.SetActive(true);
            Time.timeScale = 0;
        }
    }
}
