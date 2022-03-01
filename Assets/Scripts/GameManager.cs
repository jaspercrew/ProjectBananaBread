using System.Collections;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;

    public bool isGameShifted;
    private int frozenFrames;
    

    private GameManager()
    {
        if (Instance == null)
            Instance = this;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        // set current state to alt, then switch everything
        // this calls SwitchToState() on all entities in the scene,
        // changing everything to the originalState
        isGameShifted = false;
        ShiftWorld();
    }

    // private void Update()
    // {
    //     if (frozenFrames > 0)
    //     {
    //         Debug.Log("decrement frozeframes");
    //         frozenFrames -= 1;
    //         if (frozenFrames == 0)
    //         {
    //             Time.timeScale = 1;
    //         }
    //     }
    //
    //     
    // }

    public void ShiftWorld()
    {
        Entity[] entities = FindObjectsOfType<Entity>();
        isGameShifted = !isGameShifted;
        

        foreach (Entity entity in entities)
        {
            entity.Shift();
        }
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
