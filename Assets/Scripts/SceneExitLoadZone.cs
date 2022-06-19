using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExitLoadZone : MonoBehaviour
{
    // public string exitName;
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
        SceneManager.LoadSceneAsync(SceneInformation.Instance.SceneInfoForExit(transform).destinationScene.name);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() != null)
        {
            ExitToNextSpawn e = SceneInformation.Instance.SceneInfoForExit(transform);
            Debug.Log("exit touched, setting last exit to " + e.exitTrigger);
            SceneTransitionManager.Instance.LastExitInfo = new LastExitInfo(e);
            SwitchScene();
        }
    }
}
