using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    protected EnvironmentState EntityState { get; private set; }
    
    // TODO: sprite set
    private Dictionary<EnvironmentState, Sprite[]> stateToSpritesMap = 
        new Dictionary<EnvironmentState, Sprite[]>();

    public virtual void SwitchToState(EnvironmentState newState)
    {
        EntityState = newState;
        
        // TODO: change sprite set
    }
}
