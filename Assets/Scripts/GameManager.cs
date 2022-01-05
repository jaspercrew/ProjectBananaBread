using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    
    public EnvironmentState originalState = EnvironmentState.RealWorld;
    public EnvironmentState altState;
    public EnvironmentState currentState;

    public GameManager()
    {
        if (Instance != null)
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

    public void SwitchWorldState()
    {
        Entity[] entities = GetComponentsInParent<Entity>();
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

        foreach (Entity entity in entities)
        {
            entity.SwitchToState(newState);
        }
    }
}
