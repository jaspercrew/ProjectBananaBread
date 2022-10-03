using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Cinemachine;
using TMPro;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const int microBeatsInBeat = 16;
    public bool musicStart;
    private bool playSnares;
    public float snareDelay = 1f;
    public bool isMenu;
    public bool[] scenesCompleted;
    private bool playFlipped;

    // https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity
    private float microBpm;
    public float songBpm;
    public float firstBeatOffset; //DOES THIS EVEN DO ANYTHING?????????
    private float secPerBeat;
    private double songPosition;
    private double songPositionInBeats = float.NegativeInfinity;
    private double songTime;
    public float coroutineDelay = .5f;
    private int snareMicroBeatCount = 0;
    private int snareCount = 0;
    public int snareToSongDelay = 2;
    private double nextLoopTime;
    public bool isPaused;
    public GameObject MenuCenterCam;
    private BeatEntity[] entities;
    private SpriteRenderer[] metronomes;
    private Transform pauseOverlay;

    private Dictionary<string, int> sceneNameToBuildIndex = new Dictionary<string, int>();
    
    private void Awake()
    {
        if (Instance == null)
        {
            // Debug.Log("setting gm instance");
            Instance = this;
        }
        
        GetSceneIndices();
    }

    public void ToggleFullScreen()
    {
        Screen.fullScreen = !Screen.fullScreen;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // TODO
        nextLoopTime = Double.MaxValue;
        Application.targetFrameRate = 144;
        microBpm = songBpm * 16f;
        secPerBeat = 60f / microBpm;
        songTime = (double) Time.time;
        StartCoroutine(SnareCoroutine());
        SaveData.LoadSettings();
        SaveData.LoadFromFile(1);
        entities = FindObjectsOfType<BeatEntity>();
        metronomes = CameraManager.Instance.transform.Find("MetronomeUI").GetComponentsInChildren<SpriteRenderer>();
        //print(metronomes.Length);
        if (!isMenu)
        {
            pauseOverlay = CameraManager.Instance.transform.Find("PauseOverlay");
            pauseOverlay.gameObject.SetActive(false);
        }

        //print(scenesCompleted.ToList());

    }
    
    private IEnumerator SnareCoroutine()
    {
        CharController.Instance.transform.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
        CharController.Instance.isMetronomeLocked = true;
        yield return new WaitForSeconds(snareDelay);
        playSnares = true;
    }

    private void Update()
    {
        songPosition = (float) (Time.time - songTime - firstBeatOffset);
        double newSongPositionInBeats = songPosition / secPerBeat;
        if (Mathf.Floor((float)newSongPositionInBeats) > Mathf.Floor((float)songPositionInBeats))
        {
            WorldMicroBeat();
        }
        songPositionInBeats = newSongPositionInBeats;
        if (Time.time + 1.0f > nextLoopTime)
        {
            //return;
            //print("loop triggered, is flipped?:" + playFlipped);
            AudioManager.Instance.PlaySongScheduled(SceneInformation.Instance.songA, 0, nextLoopTime, playFlipped);
            AudioManager.Instance.PlaySongScheduled(SceneInformation.Instance.songB, 1, nextLoopTime, playFlipped);
            AudioManager.Instance.PlaySongScheduled(SceneInformation.Instance.songC, 2, nextLoopTime, playFlipped);
            playFlipped = !playFlipped;
            nextLoopTime = nextLoopTime + (60f / songBpm * 64f);
        }
    }

    private void BeginSongLoop()
    {
        //print("begin song loop");
        AudioManager.Instance.PlaySongScheduled(SceneInformation.Instance.songA, 0, Time.time, playFlipped);
        AudioManager.Instance.PlaySongScheduled(SceneInformation.Instance.songB, 1, Time.time, playFlipped);
        AudioManager.Instance.PlaySongScheduled(SceneInformation.Instance.songC, 2, Time.time, playFlipped);
        nextLoopTime = Time.time + (60f / songBpm * 64f);
        playFlipped = !playFlipped;
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
            CharController.Instance.isMetronomeLocked = false;
            if (entities.Length > 0)
            {
                foreach (BeatEntity entity in entities)
                {
                    entity.MicroBeat();
                }
            }
        }
        else if (playSnares)
        {
            snareMicroBeatCount++;
            if (snareMicroBeatCount % 16 == 0 && snareMicroBeatCount <= 64)
            {
                AudioManager.Instance.Play(SoundName.Snare, .5f);
                Color temp = metronomes[snareCount].color;
                temp.a = 1f;
                metronomes[snareCount].color = temp;
                snareCount++;
            }
            
            if (snareMicroBeatCount == (4 * microBeatsInBeat) + snareToSongDelay)
            {
                BeginSongLoop();
                musicStart = true;
                foreach (SpriteRenderer spr in metronomes)
                {
                    Color temp1 = spr.color;
                    temp1.a = 0f;
                    spr.color = temp1;

                }
                
            }
        }
    }

    private void GetSceneIndices() {
        for (int i = 0; i < SceneManager.sceneCountInBuildSettings; i++) {
            string sceneName = SceneNameFromIndex(i);
            sceneNameToBuildIndex[sceneName] = i;
        }
    }

    public int BuildIndexFromSceneName(string sceneName)
    {
        if (sceneNameToBuildIndex.ContainsKey(sceneName))
        {
            return sceneNameToBuildIndex[sceneName];
        }
        else
        {
            Debug.LogError("Scene \"" +sceneName + "\" not in build settings!");
            return -1;
        }
    }
    
    private static string SceneNameFromIndex(int index)
    {
        string path = SceneUtility.GetScenePathByBuildIndex(index);
        return path
            .Substring(0, path.Length - 6)  // gets rid of ".scene"
            .Substring(path.LastIndexOf('/') + 1);       // gets the name of the scene (after directory)
    }
    
    public void AttemptSwitchScene(int index)
    {
        //print(index);
        SaveData.SaveToFile(1);
        StartCoroutine(LoadScene(index));
    }
    
    // public void AttemptSwitchScene(string sceneName)
    // {
    //     //print(index);
    //     SaveData.SaveToFile(1);
    //     StartCoroutine(LoadScene(sceneName));
    //     
    // }
    
    
    
    

    private IEnumerator LoadScene(int index)
    {
        
        AudioManager.Instance.AllFadeOut();
        SceneInformation.Instance.sceneFadeAnim.speed = 2 / SceneInformation.SceneTransitionTime;
        SceneInformation.Instance.sceneFadeAnim.SetTrigger(Animator.StringToHash("Start"));
        yield return new WaitForSeconds(SceneInformation.SceneTransitionTime);
        SceneManager.LoadSceneAsync(index);
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


    public void MenuExit(int index)
    {
        if (scenesCompleted[index - 1] || index < 2)
        {
            PrepMenuExit(index);
            StartCoroutine(MenuExitCoroutine());
        }
    }

    private IEnumerator MenuExitCoroutine()
    {
        float dropDelay = 2.0f;
        MenuCenterCam.GetComponent<CinemachineVirtualCamera>().m_Priority = 100;
        yield return new WaitForSeconds(dropDelay);
        TileStateManager.Instance.transform.Find("GridMain").transform.Find("Menu-Exit").gameObject.SetActive(false);
        AudioManager.Instance.IsolatedPlay(SoundName.ExtendedSnare, .25f);

    }

    public void PrepMenuExit(int index)
    {
        //SceneManager.LoadScene(index);
        AudioManager.Instance.AllFadeOut();
        // string path = SceneUtility.GetScenePathByBuildIndex(index);
        // SceneInformation.Instance.exitMappings[0].sceneNameOverride = 
        //     path.Substring(0, path.Length - 6).Substring(path.LastIndexOf('/') + 1);
        SceneInformation.Instance.exitMappings[0].destSceneName = SceneNameFromIndex(index);
    }

    public void Pause()
    {
        Assert.IsTrue(!isPaused);
        if (isMenu)
        {
            return;
        }
        AudioManager.Instance.PauseAudio();
        isPaused = true;
        Time.timeScale = 0f;
        AudioListener.pause = true;
        pauseOverlay.gameObject.SetActive(true);
    }
    
    public void Unpause()
    {
        Assert.IsTrue(isPaused);
        if (isMenu)
        {
            return;
        }
        AudioManager.Instance.UnpauseAudio();
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
        Token[] tokens = FindObjectsOfType<Token>();
        foreach (Token token in tokens)
        {
            token.ResetToken();
        }

        Gate[] gates = FindObjectsOfType<Gate>();
        foreach (Gate gate in gates)
        {
            gate.ResetGate();
        }
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

    public void TextPop(string text, float duration = 2f)
    {
        GameObject prefab = Resources.Load<GameObject>("Prefabs/TextNotification");
        GameObject created = Instantiate(prefab);
        created.transform.position = CharController.position;
        Destroy(created, duration);
        TMP_Text tmp = created.transform.Find("Canvas").Find("Text").GetComponent<TMP_Text>();
        StartCoroutine(TextPopCoroutine(created, duration));
        tmp.text = text;
    }
    private IEnumerator TextPopCoroutine(GameObject created, float time)
    {
        if (created == null)
        {
            yield break;
        }
        Transform t = CharController.Instance.transform;
        TMP_Text tmp = created.transform.Find("Canvas").Find("Text").GetComponent<TMP_Text>();
        float elapsedTime = 0;
        
        Vector3 offset = new Vector3(.5f, 1.5f, 0);
        float effectHeight = 2f;
        while (elapsedTime < time)
        {
            if (created == null)
            {
                yield break;
            }
            Color c = tmp.color;
            c.a -= 1 * Time.fixedDeltaTime / time;
            tmp.color = c;

            Vector3 startingPos = t.position + offset;
            Vector3 finalPos = t.position + offset + (t.up * effectHeight);
            created.transform.position = Vector3.Lerp(startingPos, finalPos, (elapsedTime / time));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
    }
}
