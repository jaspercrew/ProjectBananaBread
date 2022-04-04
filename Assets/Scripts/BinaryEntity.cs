using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BinaryEntity : Entity
{

    // Start is called before the first frame update
    protected virtual void Start() {
        CheckEntity();
    }

    protected virtual void TurnShifted()
    {

    }

    protected virtual void TurnUnshifted()
    {

    }
    

    public override void Shift(){
        CheckEntity();
    }

    protected void CheckEntity() {
        if (GameManager.Instance.isGameShifted)
        {
            TurnShifted();
        }
        else
        {
            TurnUnshifted();
        }
    }
}
