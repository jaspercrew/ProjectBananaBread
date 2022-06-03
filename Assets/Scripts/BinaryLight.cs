using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class BinaryLight : BinaryEntity
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
        lightComponent.enabled = true;
    }
    
    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        lightComponent.enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
