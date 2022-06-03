using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorTrigger : MonoBehaviour
{
    public bool isActivated = false;
    
    public virtual void Activate()
    {
        isActivated  = true;
    }

    public virtual void Deactivate()
    {
        isActivated = false;
    }

}
