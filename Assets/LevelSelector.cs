using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LevelSelector : MonoBehaviour
{
    public  int selectedLevel;
    public int level;
    public bool isActive;
    public GameObject Continue;
    void Update()
    {
        Debug.Log(selectedLevel);
        if(selectedLevel != 0)
        {
            isActive = true;
            Continue.SetActive(true);
        }
        else
        {
            isActive = false ;
            Continue.SetActive(false);
        }

    }

    public void OpenScene(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (selectedLevel == 1)
            {
                SceneManager.LoadScene("CityMap");
            }
           /* if (selectedLevel == 2)
            {
                SceneManager.LoadScene("Jungle_Scene");
            }
            if (selectedLevel == 3)
            {
                SceneManager.LoadScene("Sandbox");
            } */
        }

    }

    
    
}
