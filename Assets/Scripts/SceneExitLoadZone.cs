using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

public class SceneExitLoadZone : MonoBehaviour
{
    public string exitName;
    public Object scene;
    
    private void SwitchScene()
    {
        SceneManager.LoadSceneAsync(scene.name); // TODO wtf async wtf?? (in wayne voice)
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        GameManager.Instance.lastExitTouched = exitName;
        SwitchScene();
    }
}
