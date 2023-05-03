using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rain : MonoBehaviour
{
    int num;
    public GameObject rain;
    // Update is called once per frame
    private void Awake()
    {
        num = Random.Range(1, 50);

        if (num >= 20)
        {
            rain.SetActive(false);
        }
        if (num <= 19)
        {
            rain.SetActive(true);
        }
        Debug.Log(num);
    }


}
