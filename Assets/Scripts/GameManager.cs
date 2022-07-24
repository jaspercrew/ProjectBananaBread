using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isGameShifted;
    
    // public bool isDarkScene;
    // private int frozenFrames;

    // public Dictionary<string, bool> LeverDict = new Dictionary<string, bool>();
    // public bool isReady;
    
    // https://www.gamedeveloper.com/audio/coding-to-the-beat---under-the-hood-of-a-rhythm-game-in-unity
    public float songBpm;
    public float firstBeatOffset;
    private float secPerBeat;
    private float songPosition;
    private float songPositionInBeats;
    private float dspSongTime;

    private void Awake()
    {
        if (Instance == null)
        {
            // Debug.Log("setting gm instance");
            Instance = this;
        }
        isGameShifted = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
        // TODO
        Application.targetFrameRate = 144;

        secPerBeat = 60f / songBpm;

        dspSongTime = (float) AudioSettings.dspTime;
    }

    private void Update()
    {
        // if (Input.GetKeyDown(KeyCode.M))
        // {
        //     TextPop("test text");
        // }
        
        songPosition = (float) (AudioSettings.dspTime - dspSongTime - firstBeatOffset);
        float newSongPositionInBeats = songPosition / secPerBeat;

        if (Mathf.Floor(newSongPositionInBeats) > Mathf.Floor(songPositionInBeats))
        {
            WorldBeat();
        }
        
        songPositionInBeats = newSongPositionInBeats;
    }

    public void WorldBeat()
    {
        isGameShifted = !isGameShifted;
        
        // shift entities
        BeatEntity[] entities = FindObjectsOfType<BeatEntity>();
        
        foreach (BeatEntity entity in entities)
        {
            entity.Beat();
        }
        
        // shift tiles
        TileStateManager t = TileStateManager.Instance;
        if (t != null)
        {
            t.ShiftTilesTo(isGameShifted);
        }
        
        // play sound
        //AudioManager.Instance.OnShift();
    }

    public void FreezeFrame()
    {
        const float freezeTime = 0.1f;
        StartCoroutine(FreezeFrameCoroutine(freezeTime));
    }

    public void PlayerDeath()
    {
        StartCoroutine(PlayerDeathCoroutine());
    }

    private IEnumerator PlayerDeathCoroutine()
    {
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("DemoA");
    }

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
