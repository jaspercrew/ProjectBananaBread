using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private EnvironmentState entityState;
    // TODO: sprite set
    private Dictionary<EnvironmentState, Sprite[]> stateToSpritesMap = 
        new Dictionary<EnvironmentState, Sprite[]>();

    public virtual void SwitchToState(EnvironmentState newState)
    {
        entityState = newState;
        
        // TODO: change sprite set
        
    }
}
