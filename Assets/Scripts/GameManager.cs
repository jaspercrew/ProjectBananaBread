using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public EnvironmentState originalState = EnvironmentState.RealWorld;
    public EnvironmentState altState;
    //[HideInInspector]
    public EnvironmentState currentState;
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
        currentState = altState;
        SwitchWorldState();
    }

    private void Update()
    {
        if (frozenFrames > 0)
        {
            Debug.Log("decrement frozeframes");
            frozenFrames -= 1;
            if (frozenFrames == 0)
            {
                Time.timeScale = 1;
            }
        }

        
    }

    public void SwitchWorldState()
    {
        Entity[] entities = FindObjectsOfType<Entity>();
        // string s = "";
        //
        // foreach (Entity e in entities)
        // {
        //     s += e.name + ", ";
        // }
        //
        // Debug.Log(s.Substring(0, s.Length - 2));
        
        EnvironmentState newState;
        
        if (currentState == originalState)
        {
            newState = altState;
        } 
        else if (currentState == altState)
        {
            newState = originalState;
        }
        else
        {
            Debug.LogError("Error: scene isn't in original OR alternate states; something went wrong");
            return;
        }

        currentState = newState;

        foreach (Entity entity in entities)
        {
            entity.SwitchToState(newState);
        }
    }

    public void FreezeFrame()
    {
        Time.timeScale = 0;
        const int frames = 50;
        frozenFrames = frames;
    }
}
