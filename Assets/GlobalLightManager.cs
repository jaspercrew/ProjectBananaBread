using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class GlobalLightManager : BinaryEntity
{
    private Light2D lightComponent;
    // Start is called before the first frame update
    protected override void Start()
    {
        lightComponent = GetComponent<Light2D>();
        base.Start();
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        lightComponent.enabled = false;
    }
    
    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        lightComponent.enabled = true;
    }
}
