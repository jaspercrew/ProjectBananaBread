using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public string lastExitTouched;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
    }
}
