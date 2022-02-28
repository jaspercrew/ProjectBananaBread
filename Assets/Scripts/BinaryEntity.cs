using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BinaryEntity : Entity
{
    public EnvironmentState enabledState;
    public bool isShifted;

    // Start is called before the first frame update
    protected virtual void Awake() {
        CheckEntity(GameManager.Instance.currentState);
    }

    protected virtual void ShiftEntity()
    {
        isShifted = true;
    }

    protected virtual void DeshiftEntity()
    {
        isShifted = false;
    }
        
    

    public override void SwitchToState(EnvironmentState state) {
        CheckEntity(state);
    }

    protected void CheckEntity(EnvironmentState state) {
        if (enabledState == state)
        {
            ShiftEntity();
        }
        else
        {
            DeshiftEntity();
        }
    }
}
