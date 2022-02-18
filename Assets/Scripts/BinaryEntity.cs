using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BinaryEntity : Entity
{
    public EnvironmentState enabledState;
    public bool isActive;

    // Start is called before the first frame update
    private void Awake() {
        CheckEntity(GameManager.Instance.currentState);
    }

    protected virtual void ActivateEntity()
    {
        isActive = true;
    }

    protected virtual void DeactivateEntity()
    {
        isActive = false;
    }
        
    

    public override void SwitchToState(EnvironmentState state) {
        CheckEntity(state);
    }

    protected void CheckEntity(EnvironmentState state) {
        if (enabledState == state)
        {
            ActivateEntity();
        }
        else
        {
            DeactivateEntity();
        }
    }
}
