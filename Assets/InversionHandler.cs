using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InversionHandler : BeatEntity
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void MicroBeatAction()
    {
        VolumeManager.instance.ToggleVolume();

        if (CharController.Instance.isInverted)
        {
            CharController.Instance.DeInvert();
        }
        else
        {
            CharController.Instance.Invert();
        }
        
        base.MicroBeatAction();
    }
}
