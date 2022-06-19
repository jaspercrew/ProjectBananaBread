using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatedEntity : BinaryEntity
{
    protected bool isActive;
    public bool activeInReal = true;
    public bool activeInAlt = true;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (activeInAlt)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (activeInReal)
        {
            Activate();
        }
        else
        {
            Deactivate();
        }
    }

    protected virtual void Activate()
    {
        isActive = true;
    }

    protected virtual void Deactivate()
    {
        isActive = false;
    }
}
