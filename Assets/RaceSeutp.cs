using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaceSeutp : MonoBehaviour
{
    public GameObject[] players;
    public void SetEnteredBoxForAllPlayers()
    {

        foreach (GameObject player in players)
        {
            LapManager lapManager = player.GetComponent<LapManager>();
            if (lapManager != null)
            {
                lapManager.enteredBox = true;
            }
        }
    }

    private void Update()
    {
        players = GameObject.FindGameObjectsWithTag("Player");
    }
}
