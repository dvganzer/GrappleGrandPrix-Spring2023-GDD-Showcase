using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spawn : MonoBehaviour
{
    public Vector3 startPos;
    public void OnPlayerJoined()
    {
        transform.position = startPos;
    }
}
