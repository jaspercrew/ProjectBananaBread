using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : MonoBehaviour
{
    public bool isActive;
    public string leverName;
    private Dictionary<string, bool> leverDict;

    public void Activate()
    {
        isActive = true;
        leverDict[leverName] = isActive;

    }
    // Start is called before the first frame update
    void Start()
    {
        leverDict = GameManager.Instance.leverDict;
        if (!leverDict.ContainsKey(leverName))
        {
            leverDict.Add(leverName, false);
        }
        isActive = leverDict[leverName];
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
