using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    public GameObject Setting;
    public GameObject panel;

    void Start()
    {
        
    }

    // Update is called once per frame


   public void Settings(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Time.timeScale = 0;
            Setting.SetActive(true);
            panel.SetActive(false);

        }
    }
    public void CloseSettings(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            Time.timeScale = 1;
            Setting.SetActive(false);
            panel.SetActive(true);

        }
    }

    public void MainMenu()
    {
        
        {
            Time.timeScale = 1;
            SceneManager.LoadScene("TitleScreen");

        }
    }
}
