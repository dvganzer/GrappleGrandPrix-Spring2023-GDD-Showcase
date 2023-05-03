using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnPrefabs : MonoBehaviour
{
    public GameObject button;
    public GameObject finishLine;
    public  AudioSource amb;
    public AudioSource raceMusic;
    public AudioSource countdown;
    
    void Start()
    {
        Instantiate(button);
        Instantiate(finishLine);
        Instantiate(amb);
        Instantiate(raceMusic);
        Instantiate(countdown);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
