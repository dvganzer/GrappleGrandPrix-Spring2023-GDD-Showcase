using UnityEngine;
using UnityEngine.UI;

public class TriggerManager : MonoBehaviour
{
    public GameObject[] players;
    public Text[] positionText;

    private void Update()
    {
        // Find all game objects with the "Player" tag and store them in an array
        players = GameObject.FindGameObjectsWithTag("Player");
    }


    private void OnTriggerEnter(Collider other)
    {
        int position = 1;

        for (int i = 0; i < players.Length; i++)
        {
            if (other.gameObject == players[i])
            {
                position = i + 1;
                Debug.Log("Player " + position + " triggered the object.");
                break;
            }
        }

        if (position != 0)
        {
            foreach (Text text in positionText)
            {
                //text.text += "Player" + players[i] + ":" + position;
            }
        }
    }
}


