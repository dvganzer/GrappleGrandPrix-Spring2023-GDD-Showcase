using System.Collections;
using System.Collections.Generic;
using UnityEngine.InputSystem;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsAppear : MonoBehaviour
{
    public GameObject Setting;
    public void Settings(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
         
            Setting.SetActive(true);
            
        }
        if (context.canceled)
        {

            Setting.SetActive(false);

        }
    }

    public void ToMapSelection(InputAction.CallbackContext context)
    {
        SceneManager.LoadScene("MapSelection");
    }
}
