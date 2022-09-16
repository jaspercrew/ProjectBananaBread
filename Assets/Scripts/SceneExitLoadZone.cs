using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExitLoadZone : MonoBehaviour
{
    // public string exitName;
    private static readonly int Start = Animator.StringToHash("Start");
    public bool isSceneFinishPoint;

    private void SwitchScene()
    {
        // if (SceneInformation.Instance.SceneInfoForExit(transform).sceneNameOverride.Length < 1)
        // {
        print( SceneInformation.Instance.SceneInfoForExit(transform));
        print( SceneInformation.Instance.SceneInfoForExit(transform).destSceneName);
        string destSceneName = SceneInformation.Instance.SceneInfoForExit(transform).destSceneName;
        int buildIndex = GameManager.Instance.BuildIndexFromSceneName(destSceneName);
        GameManager.Instance.AttemptSwitchScene(buildIndex);
        // }
        // else if (GameManager.Instance.isMenu)
        // {
        //     GameManager.Instance.AttemptSwitchScene(SceneInformation.Instance.SceneInfoForExit(transform).sceneNameOverride);
        // }
        // else
        // {
        //     print("switch scene matches no conditions");
        // }
    }

    // public IEnumerator LoadScene()
    // {
    //     SceneInformation.Instance.sceneFadeAnim.speed = 1 / SceneInformation.SceneTransitionTime;
    //     SceneInformation.Instance.sceneFadeAnim.SetTrigger(Start);
    //     yield return new WaitForSeconds(SceneInformation.SceneTransitionTime);
    //     if (SceneInformation.Instance.SceneInfoForExit(transform).sceneNameOverride.Length < 1)
    //     {
    //         SceneManager.LoadSceneAsync(SceneInformation.Instance.SceneInfoForExit(transform).destinationScene.name);
    //     }
    //     else
    //     {
    //         SceneManager.LoadSceneAsync(SceneInformation.Instance.SceneInfoForExit(transform).sceneNameOverride);
    //     }
    //     
    // }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() != null)
        {
            if (isSceneFinishPoint)
            {
                print(SceneManager.GetActiveScene().buildIndex);
                GameManager.Instance.scenesCompleted[SceneManager.GetActiveScene().buildIndex] = true;
            }
            //AudioManager.Instance.AllFadeOut();
            //SaveData.SaveToFile(1);
            ExitToNextSpawn e = SceneInformation.Instance.SceneInfoForExit(transform);
            Debug.Log("exit touched, setting last exit to " + e.exitTrigger);
            SceneTransitionManager.Instance.LastExitInfo = new LastExitInfo(e);
            SwitchScene();
        }
    }
}
