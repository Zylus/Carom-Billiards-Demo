using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PlayerBall : Ball
{
    public GameObject mainCamera; // Reference to the camera centered on the player's ball;
    public GameObject forceBar; // Reference to the UI bar displaying the force of the shot
    public GameObject gameWinMenu; // Reference to the container of the menu that appears on game win
    public GameObject pauseMenu; // Reference to the container of the pause menu
    public GameObject optionsMenu; // Reference to the instanced optionsMenu
    public GameObject ballsHitPanel; // Reference to the panel displaying the balls hit in this shot
    public LineRenderer lr; // Reference to the line renderer displaying aim assist
    public Text scoreText; // Reference to the text element displaying the score
    public Text shotText; // Reference to the text element displaying the number of shots made
    public Text timeSpentText; // Reference to the text element displaying the time spent
    public Button replayButton; // Reference to the button allowing to replay the last shot
    public AudioSource scoreSound; // Reference to the AudioSource containing the score sound effect
    public AudioSource hypeSound; // Reference to the AudioSource containing the sound effect to play during hype mode
    public bool hypeModeAllowed; // Determines whether the player has set Hype Mode to be allowed (in the options menu)

    List<Rigidbody> ballBodies = new List<Rigidbody>(); // List of references to each ball's rigidbody
    List<GameObject> targetBalls = new List<GameObject>(); // List of each ball except the player's
    List<GameObject> ballsHitThisShot = new List<GameObject>(); // List of balls that have been hit during this shot
    List<GameObject> ballsRemaining = new List<GameObject>(); // List of balls that have yet to be hit during this shot
    GameObject finalBallMissing; // The final ball needed to hit to gain a point
    DataManager dataManager; // Reference to the Data Manager
    SaveState lastSave; // SaveState instance to store relevant game state info
    Vector3 tempCameraOffset; // Stores the camera's offset to reset to after Hype Mode is over
    int score = 0;
    int shots = 0;
    float timer = 0f; // Counts the time the player spent on this game
    int maxScore = 3; // Score that needs to be reached to count as a win
    string minutes;
    string seconds;
    bool scoredThisShot = false; // Used to check whether the player has already scored during this shot
    bool hypeMode = false; // Used to check whether or not hype mode is currently active
    float force; // (0-1) the "charge amount" for a shot
    int forceModifier = 300; // The base force that gets multiplied with the float "force" to determine the actual force applied to the ball
    bool isCharging = false; // Used to check whether the player is currently charging a shot (by holding space)
    bool chargeReverse = false; // Determines whether the charge bar charges "backwards" - so that it always moves between 0 and 100
    bool duringShoot = false; // Used to check whether a shot is currently happening (only false once all balls have stopped)
    bool replayingShot = false; // Used to check whether the game is currently replaying the previous shot

    protected override void Start()
    {
        base.Start();
        
        // Set up references
        foreach(Transform ball in ballsGroup.transform)
        {
            ballBodies.Add(ball.gameObject.GetComponent<Rigidbody>());
            if(ball.gameObject != gameObject)
                targetBalls.Add(ball.gameObject);
        }
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

    void UpdateTimer()
    {
        // Increases the timer float and displays it in readable format
        timer += Time.deltaTime;
        minutes = Mathf.Floor(timer / 60).ToString("00");
        seconds = Mathf.Floor(timer % 60).ToString("00");
        timeSpentText.text = minutes + ":" + seconds;
    }

    void CheckObjective()
    {
        // This function checks if we have hit all target balls and increases the score accordingly
        if(!scoredThisShot && duringShoot && !replayingShot) // Only check if we haven't scored yet and this isn't a replay
        {
            bool objectiveReached = true; // Assume true
            foreach(GameObject target in targetBalls)
            {
                if(!ballsHitThisShot.Contains(target))
                {
                    objectiveReached = false; // If at least one ball is not hit yet, objectiveReached becomes false
                    break;
                }
            }
            if(objectiveReached) // This is only true if all targetBalls have been hit
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
        // Set up the text to display on the "game won" menu, and set it active
        gameWinMenu.transform.Find("StatsText").GetComponent<Text>().text = "Duration: " + minutes + ":" + seconds + "\nShots taken: " + shots.ToString();
        gameWinMenu.SetActive(true);
        Time.timeScale = 0;
        
        // Save this game's stats as a file
        GameStats currentStats = new GameStats(score,shots,timer);
        dataManager.WriteToFile("lastgame.json",JsonUtility.ToJson(currentStats));
    }

    void CheckBallStatus()
    {
        // This function checks whether a shot is currently going on and "resets" if all balls have stopped
        if(duringShoot && !forceBar.activeSelf) // The forceBar.activeSelf bool prevents this function from executing "too soon" and instantly ending the shot
        {
            bool everyBallStopped = true; // Assume true
            foreach(Rigidbody body in ballBodies)
            {
                if(body.velocity != Vector3.zero)
                {
                    everyBallStopped = false; // If at least one ball is still moving, everyBallStopped becomes false
                    break;
                }
            }
            if(everyBallStopped) // This is only true if all balls have stopped
            {
                // End the replay
                if(replayingShot)
                    replayingShot = false;

                // Reset UI
                ballsHitPanel.transform.GetChild(0).GetComponent<Image>().color = Color.grey;
                ballsHitPanel.transform.GetChild(1).GetComponent<Image>().color = Color.grey;
                
                // Reset logic - prepare for the next shot
                replayButton.interactable = true;
                duringShoot = false;
                scoredThisShot = false;
                ballsHitThisShot.Clear();
                ballsRemaining.Clear();
                finalBallMissing = null;
            }
        }
    }

    void HypeMode()
    {
        // Checks whether the player ball is close to the final ball required to score; zooms and plays a sound if so
        if(hypeModeAllowed && duringShoot && !scoredThisShot && !replayingShot) // Only execute if the player has activated Hype Mode in the setting and we haven't scored yet
        {
            if(finalBallMissing && rb.velocity != Vector3.zero && rb.velocity.sqrMagnitude < 500 && Vector3.Distance(transform.position, finalBallMissing.transform.position) < 15)
            // Only execute if there is a final ball, the player ball is slowly moving, and the player ball and the final ball are close
            {
                if(!hypeMode)
                {
                    // Start hype mode
                    hypeMode = true;
                    hypeSound.Play();

                    // Store current camera offset to reset to after hype is over
                    tempCameraOffset = mainCamera.GetComponent<CameraController>().offset;

                    // Disable camera movement during hype mode to not interfere with the zooming
                    mainCamera.GetComponent<CameraController>().cameraMovementDisabled = true;
                }
                
                if(Vector3.Distance(mainCamera.transform.position, transform.position) > 30) // Checks the current "zoom", or distance between camera and player ball
                {
                    Vector3 newOffset = new Vector3();
                    // Interpolate between current camera position and player ball position to achieve a zoom effect
                    // Set the camera's offset instead of position so the camera can still move in sync with the ball
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
        // Reset the camera to its normal offset
        mainCamera.GetComponent<CameraController>().offset = tempCameraOffset;
        mainCamera.GetComponent<CameraController>().cameraMovementDisabled = false;
        hypeSound.Stop();
        hypeMode = false;
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
                // Check the direction in which we're charging, then increase / decrease float
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

            // Update UI elements to represent current force
            forceBar.GetComponent<Slider>().value = force;
            forceBar.transform.Find("Text").GetComponent<Text>().text = ((int)(force*100)).ToString() + "%";
    }

    void FireBall()
    {
        if(!replayingShot) // Execute this block only if it's an actual shot, not a replay
        {
            // Increase number of shots
            shots++;
            shotText.text = "Shots: " + shots.ToString();

            // Save the current game state for the purposes of replaying
            SaveState(ballsGroup, force);

            replayButton.interactable = false;
        }

        isCharging = false;
        duringShoot = true;

        // Apply an impulse to the player ball equal to the charged force * forceModifier
        rb.AddForce(transform.forward * force * forceModifier,ForceMode.Impulse);
    }

    void SaveState(GameObject ballsContainer, float force)
    {
        // Saves all balls, the force applied during the last shot, and the camera info into a SaveState object for the purposes of replaying
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

    void TogglePauseMenu()
    {
        // Called by pressing ESC
        // Toggle the pause menu game object and pause / unpause the game accordingly
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

    void UpdateAimAssistLine()
    {
        RaycastHit hit;
        Vector3 position = transform.position;
        Vector3 direction = transform.forward;

        // Cast a ray from the player ball in its shot direction
        if(Physics.Raycast(position, direction, out hit, Mathf.Infinity))
        {
            // Set the end point of the aim assit line to the hit point of the ray
            lr.SetPosition(1,transform.InverseTransformPoint(hit.point));
        }
    }

    void FadeOutForceBar()
    {
        if(!isCharging && forceBar.activeSelf)
        {
            // Fade out the force bar over time
            forceBar.GetComponent<CanvasGroup>().alpha -= Time.deltaTime / 1f;

            // If the force bar is fully faded out, disable and reset it
            if(forceBar.GetComponent<CanvasGroup>().alpha <= 0) {
                forceBar.SetActive(false);
                forceBar.GetComponent<CanvasGroup>().alpha = 1f;
            }
        }
    }

    public void ReplayLastShot()
    {
        // Called when the replay button is pressed
        // This function replays the last shot the player made by resetting all balls to be in the last position and applying the same force the player did
        if(lastSave != null) // Prevents the replay if no previous shot exists
        {
            replayButton.interactable = false;
            replayingShot = true;

            foreach(Transform ball in ballsGroup.transform)
            {
                // Set each ball to have the same position and rotation as when the last shot was made
                SavedBall correctBall = lastSave.GetSavedBall(ball.gameObject.name);
                ball.position = correctBall.position;
                ball.eulerAngles = correctBall.eulerAngles;
            }

            force = lastSave.force;
            // Display the force bar with the amount of force the player applied last shot
            forceBar.GetComponent<Slider>().value = force;
            forceBar.transform.Find("Text").GetComponent<Text>().text = ((int)(force*100)).ToString() + "%";
            forceBar.SetActive(true);

            // Set camera to the same values as it was last shot
            mainCamera.transform.position = lastSave.cameraPosition;
            mainCamera.transform.eulerAngles = lastSave.cameraAngle;
            mainCamera.GetComponent<CameraController>().offset = lastSave.cameraOffset;

            // Fire the ball as if it were an actual shot
            FireBall();
        }
    }
    
    protected override void OnCollisionEnter(Collision collision)
    {
        if(collision.collider.transform.IsChildOf(ballsGroup.transform))
        {
            if(targetBalls.Contains(collision.collider.gameObject) && !ballsHitThisShot.Contains(collision.collider.gameObject))
            {
                // If the player ball is hitting another ball, add it to the list of balls we've hit this shot
                ballsHitThisShot.Add(collision.collider.gameObject);
                ballsRemaining = targetBalls.Except(ballsHitThisShot).ToList();
                if(ballsRemaining.Count() == 1)
                {
                    finalBallMissing = ballsRemaining[0]; // Set the final missing ball for the purposes of hype mode
                }

                // Display the balls we've hit on the UI
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
}
