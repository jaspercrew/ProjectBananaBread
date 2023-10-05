using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRecorder : BeatEntity
{
    protected override void MicroBeatAction()
    {
        CharController.Instance.SpawnExtendedFadeSprite();
        CharController.Instance.doRecord = true;
    }
}
