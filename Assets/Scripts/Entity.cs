using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private EnvironmentState entityState;
    private Dictionary<EnvironmentState, Animation> stateToAnimation;

    public virtual void SwitchToState(EnvironmentState newState)
    {
        entityState = newState;

        switch (newState)
        {
            
        }
    }
}
