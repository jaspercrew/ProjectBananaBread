using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEditor.SearchService;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isGameShifted;
    public bool isDarkScene;
    private int frozenFrames;

    public Dictionary<string, bool> leverDict = new Dictionary<string, bool>();
    // private GameManager()
    // {
    //     if (Instance == null)
    //         Instance = this;
    // }

    private void Awake()
    {
        Instance = this;
        isGameShifted = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Application.targetFrameRate = 144;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.M))
        {
            TextPop("test text");
        }
    }

    public void ShiftWorld()
    {
        isGameShifted = !isGameShifted;
        
        // shift entities
        Entity[] entities = FindObjectsOfType<Entity>();
        
        foreach (Entity entity in entities)
        {
            entity.Shift();
        }
        
        // shift tiles
        TileStateManager t = TileStateManager.Instance;
        if (t != null)
        {
            t.ShiftTilesTo(isGameShifted);
        }
        // play sound
        AudioManager.Instance.OnShift(isGameShifted);
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
        SceneManager.LoadScene("TTHub");
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
        created.transform.position = CharController.Instance.transform.position;
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
