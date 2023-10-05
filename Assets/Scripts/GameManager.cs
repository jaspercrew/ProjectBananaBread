using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public const int MicroBeatsInBeat = 16;
    public static GameManager instance;
    public bool musicStart;
    public float snareDelay = 1f;
    public bool isMenu;
    public int[] levelProgress;
    public float songBpm;
    public float firstBeatOffset; //DOES THIS EVEN DO ANYTHING?????????
    public float coroutineDelay = .5f;
    public int snareToSongDelay = 2;
    public bool isPaused;

    [FormerlySerializedAs("MenuCenterCam")]
    public GameObject menuCenterCam;

    private readonly Dictionary<string, int> sceneNameToBuildIndex = new Dictionary<string, int>();
    private BeatEntity[] entities;
    private SpriteRenderer[] metronomes;

    // https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity
    private float microBpm;
    private double nextLoopTime;
    private Transform pauseOverlay;
    private bool playFlipped;
    private bool playSnares;
    private float secPerBeat;
    private int snareCount;
    private int snareMicroBeatCount;
    private double songPosition;
    private double songPositionInBeats = float.NegativeInfinity;
    private double songTime;

    private void Awake()
    {
        if (instance == null)
            // Debug.Log("setting gm instance");
            instance = this;
        GetSceneIndices();
    }

    // Start is called before the first frame update
    private void Start()
    {
        // TODO
        nextLoopTime = double.MaxValue;
        Application.targetFrameRate = 144;
        microBpm = songBpm * 16f;
        secPerBeat = 60f / microBpm;
        songTime = Time.time;
        StartCoroutine(SnareCoroutine());
        SaveData.LoadSettings();
        SaveData.LoadFromFile(1);
        entities = FindObjectsOfType<BeatEntity>();
        metronomes = CameraManager.instance.transform
            .Find("MetronomeUI")
            .GetComponentsInChildren<SpriteRenderer>();
        //print(metronomes.Length);
        if (!isMenu)
        {
            pauseOverlay = CameraManager.instance.transform.Find("PauseOverlay");
            pauseOverlay.gameObject.SetActive(false);
        }

        //print(levelProgress.ToList());
    }

    private void Update()
    {
        songPosition = (float) (Time.time - songTime - firstBeatOffset);
        var newSongPositionInBeats = songPosition / secPerBeat;
        if (Mathf.Floor((float) newSongPositionInBeats) > Mathf.Floor((float) songPositionInBeats))
            WorldMicroBeat();
        songPositionInBeats = newSongPositionInBeats;
        double time = Time.time;
        if (time + 3.0d > nextLoopTime)
        {
            //print((time));
            //print(nextLoopTime);
            //return;
            //print("loop triggered, is flipped?:" + playFlipped);
            AudioManager.instance.PlaySongScheduled(
                SceneInformation.instance.songA,
                0,
                nextLoopTime,
                playFlipped
            );
            AudioManager.instance.PlaySongScheduled(
                SceneInformation.instance.songB,
                1,
                nextLoopTime,
                playFlipped
            );
            AudioManager.instance.PlaySongScheduled(
                SceneInformation.instance.songC,
                2,
                nextLoopTime,
                playFlipped
            );
            AudioManager.instance.PlaySongScheduled(
                SceneInformation.instance.songD,
                3,
                nextLoopTime,
                playFlipped
            );

            playFlipped = !playFlipped;
            //AudioClip audioClip = AudioManager.Instance.soundToClip[SceneInformation.Instance.songA];
            //nextLoopTime += (double)audioClip.samples / audioClip.frequency;

            //nextLoopTime += (60d / (double)songBpm) * 16d;
            nextLoopTime += 60d / songBpm * 32d;
            //print(60d / (double)songBpm);
        }
    }

    private void OnApplicationQuit()
    {
        SaveSettings();
        SaveData.SaveToFile(1);
    }

    private void OnAudioFilterRead(float[] data, int channels)
    {
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    private IEnumerator SnareCoroutine()
    {
        CharController.instance.transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        CharController.instance.isMetronomeLocked = true;
        yield return new WaitForSeconds(snareDelay);
        playSnares = true;
    }

    private void BeginSongLoop()
    {
        //print("begin song loop");
        // AudioManager.Instance.PlaySong(SceneInformation.Instance.songA, 0);
        // AudioManager.Instance.PlaySong(SceneInformation.Instance.songB, 1);
        // AudioManager.Instance.PlaySong(SceneInformation.Instance.songC, 2);
        // nextLoopTime = Time.time + (60f / songBpm * 64f);
        //playFlipped = !playFlipped;
        nextLoopTime = Time.time + 3f;
    }

    public void WorldMicroBeat()
    {
        StartCoroutine(BeatDelayCoroutine());
    }

    public IEnumerator BeatDelayCoroutine()
    {
        if (musicStart)
        {
            yield return new WaitForSeconds(coroutineDelay);
            CharController.instance.isMetronomeLocked = false;
            if (entities.Length > 0)
                foreach (var entity in entities)
                    entity.MicroBeat();
        }
        else if (playSnares)
        {
            snareMicroBeatCount++;
            if (snareMicroBeatCount % 16 == 0 && snareMicroBeatCount <= 64)
            {
                AudioManager.instance.Play(SoundName.Snare, .5f);
                var temp = metronomes[snareCount].color;
                temp.a = 1f;
                metronomes[snareCount].color = temp;
                snareCount++;
            }

            if (snareMicroBeatCount == 4 * MicroBeatsInBeat + snareToSongDelay)
            {
                BeginSongLoop();
                musicStart = true;
                //EchoController.Instance.Init();
                foreach (var spr in metronomes)
                {
                    var temp1 = spr.color;
                    temp1.a = 0f;
                    spr.color = temp1;
                }
            }
        }
    }

    private void GetSceneIndices()
    {
        for (var i = 0; i < SceneManager.sceneCountInBuildSettings; i++)
        {
            var sceneName = SceneNameFromIndex(i);
            sceneNameToBuildIndex[sceneName] = i;
        }
    }

    public int BuildIndexFromSceneName(string sceneName)
    {
        if (sceneNameToBuildIndex.ContainsKey(sceneName))
            return sceneNameToBuildIndex[sceneName];

        Debug.LogError("Scene \"" + sceneName + "\" not in build settings!");
        return -1;
    }

    private static string SceneNameFromIndex(int index)
    {
        var path = SceneUtility.GetScenePathByBuildIndex(index);
        return path.Substring(0, path.Length - 6) // gets rid of ".scene"
            .Substring(path.LastIndexOf('/') + 1); // gets the name of the scene (after directory)
    }

    public void AttemptSwitchScene(int index)
    {
        //print(index);
        SaveData.SaveToFile(1);
        SaveData.SaveSettings();
        StartCoroutine(LoadScene(index));
    }

    // public void AttemptSwitchScene(string sceneName)
    // {
    //     //print(index);
    //     SaveData.SaveToFile(1);
    //     StartCoroutine(LoadScene(sceneName));
    //
    // }

    private IEnumerator LoadScene(int sceneIndex)
    {
        AudioManager.instance.AllFadeOut();
        SceneInformation.instance.exitMappings[0].destSceneName = SceneNameFromIndex(sceneIndex);
        var furthestCheckpoint = levelProgress[sceneIndex];
        SceneTransitionManager.instance.checkPointToUse = furthestCheckpoint;
        SceneInformation.instance.sceneFadeAnim.speed = 2 / SceneInformation.SceneTransitionTime;
        SceneInformation.instance.sceneFadeAnim.SetTrigger(Animator.StringToHash("Start"));
        yield return new WaitForSeconds(SceneInformation.SceneTransitionTime);
        SceneManager.LoadSceneAsync(sceneIndex);
        //AudioManager.Instance.Awake();
    }

    // private IEnumerator LoadScene(string sceneName)
    // {
    //
    //     AudioManager.Instance.AllFadeOut();
    //     SceneInformation.Instance.sceneFadeAnim.speed = 2 / SceneInformation.SceneTransitionTime;
    //     SceneInformation.Instance.sceneFadeAnim.SetTrigger(Animator.StringToHash("Start"));
    //     yield return new WaitForSeconds(SceneInformation.SceneTransitionTime);
    //     SceneManager.LoadSceneAsync(sceneName);
    //     //AudioManager.Instance.Awake();
    // }


    public void MenuExit(int sceneIndex)
    {
        SaveData.SaveToFile(1);
        if (
            sceneIndex >= levelProgress.Length
            || //out of indices
            levelProgress[sceneIndex] == SaveData.levelLengths[sceneIndex] - 1
            || sceneIndex < 2
        )
        {
            PrepMenuExit(sceneIndex);
            StartCoroutine(MenuExitCoroutine());
        }
    }

    private IEnumerator MenuExitCoroutine()
    {
        var dropDelay = 2.0f;
        GameObject.Find("CenterCanvas").GetComponent<Canvas>().enabled = false;
        menuCenterCam.GetComponent<CinemachineVirtualCamera>().m_Priority = 100;
        yield return new WaitForSeconds(dropDelay);
        /*TileStateManager.instance.transform
            .Find("GridMain")
            .transform.Find("Menu-Exit")
            .gameObject.SetActive(false);*/
        AudioManager.instance.IsolatedPlay(SoundName.ExtendedSnare, .25f);
    }

    public void PrepMenuExit(int sceneIndex)
    {
        //SceneManager.LoadScene(index);
        AudioManager.instance.AllFadeOut();
        // string path = SceneUtility.GetScenePathByBuildIndex(index);
        // SceneInformation.Instance.exitMappings[0].sceneNameOverride =
        //     path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
        SceneInformation.instance.exitMappings[0].destSceneName = SceneNameFromIndex(sceneIndex);
        SceneTransitionManager.instance.checkPointToUse = levelProgress[sceneIndex];
    }

    public void Pause()
    {
        Assert.IsTrue(!isPaused);
        if (isMenu)
            return;
        AudioManager.instance.PauseAudio();
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        pauseOverlay.gameObject.SetActive(true);
    }

    public void Unpause()
    {
        Assert.IsTrue(isPaused);
        if (isMenu)
            return;
        AudioManager.instance.UnpauseAudio();
        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        pauseOverlay.gameObject.SetActive(false);
    }

    public void FreezeFrame()
    {
        const float freezeTime = 0.1f;
        StartCoroutine(FreezeFrameCoroutine(freezeTime));
    }

    public void PlayerDeath()
    {
        /*var tokens = FindObjectsOfType<Token>();
        foreach (var token in tokens)
            token.ResetToken();*/

        /*var gates = FindObjectsOfType<Gate>();
        foreach (var gate in gates)
            gate.ResetGate();*/
    }

    // public void ChangeAudio(float slider)
    // {
    //     const float volumeMultiplier = 5f;
    //     foreach (AudioSource source in AudioManager.Instance.mainSources)
    //     {
    //         source.volume = slider * volumeMultiplier;
    //     }
    // }
    public void SaveSettings()
    {
        SaveData.SaveSettings();
    }

    // private IEnumerator PlayerDeathCoroutine()
    // {
    //     yield return new WaitForSeconds(2f);
    //     SceneManager.LoadScene("DemoA");
    // }

    private IEnumerator FreezeFrameCoroutine(float time)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1f;
    }

    public void CloseGame()
    {
        Application.Quit();
    }

    public void TextPop(string text, float duration = 2f)
    {
        var prefab = Resources.Load<GameObject>("Prefabs/TextNotification");
        var created = Instantiate(prefab);
        created.transform.position = CharController.instance.transform.position;
        Destroy(created, duration);
        var tmp = created.transform.Find("Canvas").Find("Text").GetComponent<TMP_Text>();
        StartCoroutine(TextPopCoroutine(created, duration));
        tmp.text = text;
    }

    private IEnumerator TextPopCoroutine(GameObject created, float time)
    {
        if (created == null)
            yield break;
        var t = CharController.instance.transform;
        var tmp = created.transform.Find("Canvas").Find("Text").GetComponent<TMP_Text>();
        float elapsedTime = 0;

        var offset = new Vector3(.5f, 1.5f, 0);
        var effectHeight = 2f;
        while (elapsedTime < time)
        {
            if (created == null)
                yield break;
            var c = tmp.color;
            c.a -= 1 * Time.fixedDeltaTime / time;
            tmp.color = c;

            var startingPos = t.position + offset;
            var finalPos = t.position + offset + t.up * effectHeight;
            created.transform.position = Vector3.Lerp(startingPos, finalPos, elapsedTime / time);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}