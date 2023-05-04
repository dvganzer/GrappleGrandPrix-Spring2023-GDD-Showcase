using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LapManager : MonoBehaviour
{
    public float BestLapTime { get; private set; } = Mathf.Infinity;
    public float LastLapTime { get; private set; } = 0;
    public float CurrentLapTime { get; private set; } = 0;
   public int CurrentLap { get; private set; } = 0;

    public float lapTimerTimeStamp;
    public int lastCheckpointPassed = 0;

    public Transform checkpointsParent;
    public int checkpointCount;
    public int checkpointLayer;

    
    public FirstPersonMovement playerController;

    public AudioSource city;
    public AudioSource raceMusic;
    public AudioSource raceMusic2;
    public AudioSource checkpointPassedSound;
    public AudioSource countdown;
    public AudioSource missedCheckpoint;

    public Text countdownText;
    public GameObject finishLine;
    public float countdownTimer = 0f;
    public float countdownDuration = 3f;

    public GameObject[] players;
    RaceSeutp raceSetup;


    public bool lapStarted = false;
    public bool enteredBox = false;


    public List<AudioSource> music;
    private int currentSourceIndex;
    void Awake()
    {
        raceSetup = GetComponent<RaceSeutp>();
        finishLine = GameObject.FindGameObjectWithTag("FinishLine");
        players = GameObject.FindGameObjectsWithTag("Player");
        playerController = GetComponent<FirstPersonMovement>();
        checkpointsParent = GameObject.Find("Checkpoints").transform;
        checkpointCount = checkpointsParent.childCount;
        checkpointLayer = LayerMask.NameToLayer("Checkpoint");
        
    }

   public void StartLap()
    {
        Debug.Log("StartLap!");
        CurrentLap++;
        lastCheckpointPassed = 1;
        lapTimerTimeStamp = Time.time;
        Debug.Log("Lap:" + CurrentLap);
        
        
    }
    void EndLap()
    {
        LastLapTime = lapTimerTimeStamp;
        BestLapTime = Mathf.Min(LastLapTime, BestLapTime);
        Debug.Log("EndLap - LapTime was " + LastLapTime + "seconds");
        CurrentLapTime = 0;
    }

    private void OnTriggerEnter(Collider collider)
    {

        if (collider.gameObject.name == "1")
        {

            if (lastCheckpointPassed == checkpointCount && CurrentLap == 3)
            {
                city.Play();
                StopCurrentSource();
                EndLap();
                lapStarted = false;

            }

        }

        if (lastCheckpointPassed == checkpointCount && CurrentLap != 3)
        {
            StartLap();
            LastLapTime = lapTimerTimeStamp;
            BestLapTime = Mathf.Min(LastLapTime, BestLapTime);

        }


        if (collider.gameObject.layer == checkpointLayer && collider.gameObject.name == (lastCheckpointPassed + 1).ToString())
        {
            lastCheckpointPassed++;
            checkpointPassedSound.Play();
        }
        if (collider.gameObject.layer == checkpointLayer && collider.gameObject.name != (lastCheckpointPassed).ToString())
        {
            missedCheckpoint.Play();
        }

        if (collider.gameObject.tag == "StartBox")
        {
            

            foreach (GameObject players in players)
            {
                enteredBox = true;
                LapManager lapManager = players.GetComponent<LapManager>();

                    lapManager.enteredBox = true;
           

                Debug.Log("Hit Start");
                players.transform.position = finishLine.transform.position;
                countdownText.gameObject.SetActive(true);

                lapManager.countdownTimer = lapManager.countdownDuration;

                countdown.Play();
            }


            

        }

    }
    void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");

        if (countdownText.text == "3")
        {
            countdownText.color = Color.red;
        }
        if (countdownText.text == "2")
        {
            countdownText.color = Color.yellow;
        }
        if (countdownText.text == "1")
        {
            countdownText.color = Color.green;
        }
        if (enteredBox)
        {
            foreach (GameObject players in players)
            {
                LapManager lapManager = players.GetComponent<LapManager>();
                lapManager.countdownTimer -= Time.deltaTime;
                lapManager.countdownText.text = countdownTimer.ToString("F0");
                
                if (countdownTimer <= 0f)
                {
                    countdownText.text = "GO!";
                    lapStarted = true;
                    StartLap();
                    PlayCurrentSource();
                    city.Stop();
                    countdownText.gameObject.SetActive(false);
                    enteredBox = false;
                }

            }
            
        }

        
        if (lapStarted)
        CurrentLapTime = lapTimerTimeStamp > 0 ? Time.time - lapTimerTimeStamp : 0;
        //Debug.Log(CurrentLapTime);
        

    }
  
    void PlayCurrentSource()
    {
        if (currentSourceIndex >= music.Count)
        {
            // We've reached the end of the list, so stop playing.
            return;
        }

        AudioSource currentSource = music[currentSourceIndex];
        currentSource.Play();

        StartCoroutine(WaitForSourceEnd(currentSource.clip.length));
    }

    IEnumerator WaitForSourceEnd(float delay)
    {
        yield return new WaitForSeconds(delay);

        currentSourceIndex++;
        PlayCurrentSource();
    }

    public void StopCurrentSource()
    {
            AudioSource currentSource = music[currentSourceIndex];
            currentSource.Stop();
        
    }

}
