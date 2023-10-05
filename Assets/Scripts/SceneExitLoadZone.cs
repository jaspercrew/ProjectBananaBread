using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneExitLoadZone : MonoBehaviour
{
    // public string exitName;
    private static readonly int Start = Animator.StringToHash("Start");
    public bool isSceneFinishPoint;

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
                var sceneIndex = SaveData.levelLengths[SceneManager.GetActiveScene().buildIndex];
                GameManager.instance.levelProgress[sceneIndex] =
                    SaveData.levelLengths[sceneIndex] - 1;
            }

            //AudioManager.Instance.AllFadeOut();
            //SaveData.SaveToFile(1);
            var e = SceneInformation.instance.SceneInfoForExit(transform);
            Debug.Log("exit touched, setting last exit to " + e.exitTrigger);
            SceneTransitionManager.instance.lastExitInfo = new LastExitInfo(e);
            SwitchScene();
        }
    }

    private void SwitchScene()
    {
        // if (SceneInformation.Instance.SceneInfoForExit(transform).sceneNameOverride.Length < 1)
        // {
        // print( SceneInformation.Instance.SceneInfoForExit(transform));
        // print( SceneInformation.Instance.SceneInfoForExit(transform).destSceneName);
        var destSceneName = SceneInformation.instance.SceneInfoForExit(transform).destSceneName;
        var buildIndex = GameManager.instance.BuildIndexFromSceneName(destSceneName);
        GameManager.instance.AttemptSwitchScene(buildIndex);
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
}