using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatedEntity : BeatEntity
{
    public bool isActive = false;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }
    
    public override void Beat()
    {
        isActive = !isActive;

    }
}
