using System.Collections;
using System.Collections.Generic;
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
    

    private void Awake()
    {
        // set current state to alt, then switch everything
        // this calls SwitchToState() on all entities in the scene,
        // changing everything to the originalState
        isGameShifted = false;
        Instance = this;
    }

    // Start is called before the first frame update
    private void Start()
    {
        Application.targetFrameRate = 144;
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
        yield return new WaitForSeconds(1f);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private IEnumerator FreezeFrameCoroutine(float time)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1f;
    }
}
