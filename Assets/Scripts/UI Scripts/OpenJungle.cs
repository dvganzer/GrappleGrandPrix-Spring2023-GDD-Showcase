using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenJungle : MonoBehaviour
{
    LevelSelector levelSelector;
    public void Awake()
    {
        levelSelector = GetComponentInParent<LevelSelector>();
    }
    public void SetScene()
    {
        levelSelector.level = 2;
        SaveScene();
    }

    public void SaveScene()
    {
        levelSelector.selectedLevel = levelSelector.level;
    }
}
