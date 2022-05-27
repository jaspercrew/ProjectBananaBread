using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    private bool isOpen;
    public List<ActivatorTrigger> triggers;


    // Start is called before the first frame update
    private void Start()
    {
        
    }

    private void Update()
    {
        if (!isOpen)
        {
            bool allActive = true;
            foreach (ActivatorTrigger trigger in triggers)
            {
                allActive &= trigger.isActivated;
            }

            if (allActive)
            {
                Open();
            }
        }

    }


    public void Open()
    {
        Debug.Log("OPENED");
        isOpen = true;
    }
}
