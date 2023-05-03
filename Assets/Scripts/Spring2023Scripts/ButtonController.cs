using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;

public class ButtonController : MonoBehaviour
{
    public Text countdownText;
    public GameObject finishLine;
    public float countdownDuration = 3f;
    public GameObject Signs;
    public GameObject Checkpoints;
    public LapManager lapMananager;

    public bool buttonPressed = false;
    public bool startLap = false;
    public float countdownTimer = 0f;
    public AudioSource music;
    public AudioSource city;
    public AudioSource countdown;
    public GameObject[] players;
   // public GameObject triggerObject;
   // public TriggerManager triggerManager;



    private void Awake()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
       // triggerObject = GameObject.FindGameObjectWithTag("FinishRace");
      //  triggerManager = triggerObject.GetComponent<TriggerManager>();
    }
    private void Start()
    {
        foreach (GameObject players in players)
        {
            lapMananager = GetComponent<LapManager>();
        }
            
            
        
    }
    private void Update()
    {
        Checkpoints = GameObject.FindGameObjectWithTag("Checkpoints");
        finishLine = GameObject.FindGameObjectWithTag("FinishLine");

        foreach (GameObject players in players)
        {
            countdownText.text = countdownTimer.ToString("F0");
        }
            

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


        if (buttonPressed)
        {
            countdownTimer -= Time.deltaTime;
            if (countdownTimer <= 0f)
            {                              
                    lapMananager.lapStarted = true;
                    lapMananager.StartLap();
                    music.Play();
                    city.Stop();
                    Debug.Log("Countdown Finished!");
                    countdownText.gameObject.SetActive(false);
                    buttonPressed = false;
                              
            }
        }
       

    }
        private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.tag == "StartBox")
        {
            
            countdownText.gameObject.SetActive(true);
            buttonPressed = true;
            countdownTimer = countdownDuration;

            countdown.Play();

            foreach (GameObject players in players)
            {
                players.transform.position = finishLine.transform.position;
            }
            Instantiate(Signs);
        }
    }
}

