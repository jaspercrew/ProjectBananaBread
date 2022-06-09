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
    private static readonly int Start = Animator.StringToHash("Start");

    private void SwitchScene()
    {
        SaveData.SaveToFile(1);
        StartCoroutine(LoadScene());
    }

    private IEnumerator LoadScene()
    {
        SceneInformation.Instance.sceneFadeAnim.speed = 1 / SceneInformation.SceneTransitionTime;
        SceneInformation.Instance.sceneFadeAnim.SetTrigger(Start);
        yield return new WaitForSeconds(SceneInformation.SceneTransitionTime);
        SceneManager.LoadSceneAsync(scene.name);
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
