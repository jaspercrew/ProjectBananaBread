using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class LoadZone : MonoBehaviour
{
    public Object scene;
    
    private void SwitchScene()
    {
        SceneManager.LoadScene(scene.name);
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        SwitchScene();
    }
}
