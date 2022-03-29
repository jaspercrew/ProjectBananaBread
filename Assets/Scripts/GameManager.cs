using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isGameShifted;
    private int frozenFrames;

    public Dictionary<string, bool> leverDict = new Dictionary<string, bool>();

    private GameManager()
    {
        if (Instance == null)
            Instance = this;
    }

    private void Awake()
    {
        // set current state to alt, then switch everything
        // this calls SwitchToState() on all entities in the scene,
        // changing everything to the originalState
        isGameShifted = false;
    }

    // Start is called before the first frame update
    private void Start()
    {
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
        //t.ShiftTilesTo(isGameShifted);
        
        // play sound
        AudioManager.Instance.OnShift(isGameShifted);
    }

    public void FreezeFrame()
    {
        const float freezeTime = 0.1f;
        StartCoroutine(FreezeFrameCoroutine(freezeTime));
    }

    private IEnumerator FreezeFrameCoroutine(float time)
    {
        Time.timeScale = 0f;
        yield return new WaitForSecondsRealtime(time);
        Time.timeScale = 1f;
    }
}
