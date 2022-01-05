using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    private EnvironmentState entityState;
    private Dictionary<EnvironmentState, Animation> stateToAnimation;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public virtual void SwitchToState(EnvironmentState newState)
    {
        entityState = newState;

        switch (newState)
        {
            
        }
    }
}
