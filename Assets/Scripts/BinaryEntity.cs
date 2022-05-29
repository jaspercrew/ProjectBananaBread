using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BinaryEntity : Entity
{

    // Start is called before the first frame update
    protected virtual void Start()
    {
        CheckEntity();
        //StartCoroutine(LateStart());
    }

    // protected IEnumerator LateStart()
    // {
    //     yield return new WaitForEndOfFrame();
    //     
    // }

    protected virtual void TurnShifted()
    {

    }

    protected virtual void TurnUnshifted()
    {

    }
    

    public override void Shift(){
        CheckEntity();
    }

    protected virtual void CheckEntity() {
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
