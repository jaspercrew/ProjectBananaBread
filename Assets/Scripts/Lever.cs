using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ActivatorTrigger , IHittableEntity
{
    public string leverName;
    private Dictionary<string, bool> leverDict;

    public override void Activate()
    {
        isActivated  = true;
        leverDict[leverName] = isActivated;
    }
    // Start is called before the first frame update
    void Start()
    {
        leverDict = GameManager.Instance.leverDict;
        if (!leverDict.ContainsKey(leverName))
        {
            leverDict.Add(leverName, false);
        }
        isActivated = leverDict[leverName];
    }

    public void GetHit(int damage)
    {
        if (!isActivated )
        {
            Activate();
        }
    }

    
}
