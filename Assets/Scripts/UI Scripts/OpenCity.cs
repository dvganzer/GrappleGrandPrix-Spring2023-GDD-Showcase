using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenCity : MonoBehaviour
{
    LevelSelector levelSelector;
    public void Awake()
    {
       levelSelector = GetComponentInParent<LevelSelector>();
    }
    public void SetScene()
    {
        levelSelector.level = 1;
        SaveScene();
    }

    public void SaveScene()
    {
        levelSelector.selectedLevel = levelSelector.level;
    }
}
