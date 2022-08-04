using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerRewinder : BeatEntity
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        StartCoroutine(CharController.Instance.StartRewind());
    }
}
