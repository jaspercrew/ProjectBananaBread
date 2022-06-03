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
        SaveData.SaveToFile(1);
        SceneManager.LoadSceneAsync(scene.name); // TODO wtf async wtf?? (in wayne voice)
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() != null)
        {
            SceneTransitionManager.Instance.lastExitTouched = exitName;
            SwitchScene();
        }
    }
}
