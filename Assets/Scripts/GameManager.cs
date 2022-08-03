using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.Serialization;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    public const int microBeatsInBeat = 16;
    public bool musicStart;
    private bool playSnares;
    public float snareDelay = 1f;
    
    //public bool isGameShifted;
    
    // public bool isDarkScene;
    // private int frozenFrames;

    // public Dictionary<string, bool> LeverDict = new Dictionary<string, bool>();
    // public bool isReady;
    
    // https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity
    private float microBpm;
    public float songBpm;
    public float firstBeatOffset; //DOES THIS EVEN DO ANYTHING?????????
    private float secPerBeat;
    private float songPosition;
    private float songPositionInBeats = float.NegativeInfinity;
    private float dspSongTime;
    public float coroutineDelay = .5f;
    private int snareCount = 0;
    public int snareToSongDelay = 2;
    private BeatEntity[] entities;

    private void Awake()
    {
        if (Instance == null)
        {
            // Debug.Log("setting gm instance");
            Instance = this;
        }
        //isGameShifted = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // TODO
        Application.targetFrameRate = 144;
        microBpm = songBpm * 16f;
        secPerBeat = 60f / microBpm;
        dspSongTime = (float) AudioSettings.dspTime;
        StartCoroutine(SnareCoroutine());
        entities = FindObjectsOfType<BeatEntity>();
    }

    private IEnumerator SnareCoroutine()
    {
        yield return new WaitForSeconds(snareDelay);
        playSnares = true;
    }

    private void Update()
    {
        songPosition = (float) (AudioSettings.dspTime - dspSongTime - firstBeatOffset);
        float newSongPositionInBeats = songPosition / secPerBeat;
        if (Mathf.Floor(newSongPositionInBeats) > Mathf.Floor(songPositionInBeats))
        {
            WorldMicroBeat();
        }
        songPositionInBeats = newSongPositionInBeats;
    }

    public void WorldMicroBeat()
    {
        StartCoroutine(BeatDelayCoroutine());
    }

    public IEnumerator BeatDelayCoroutine()
    {
        if (musicStart)
        {
            yield return new WaitForSecondsRealtime(coroutineDelay);
            
            foreach (BeatEntity entity in entities)
            {
                entity.MicroBeat();
            }
        }
        else if (playSnares)
        {
            snareCount++;
            if (snareCount % 16 == 0 && snareCount <= 64)
            {
                AudioManager.Instance.Play(SoundName.Snare, .5f);
            }
            
            if (snareCount == (4 * microBeatsInBeat) + snareToSongDelay)
            {
                AudioManager.Instance.PlaySong(SceneInformation.Instance.song);
                musicStart = true;
            }
        }

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

    public void TextPop(String text, float duration = 2f)
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
