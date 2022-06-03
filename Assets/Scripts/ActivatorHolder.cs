using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorHolder : ActivatorTrigger
{
    
    private Collider2D Collider2D;
    public float yLockOffset = .5f;
    
    // Start is called before the first frame update
    void Start()
    {
        Collider2D = GetComponent<Collider2D>();

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ActivatorBox activatorBox = other.gameObject.GetComponent<ActivatorBox>();
        if (activatorBox != null)
        {
            Activate();
            activatorBox.Lock();
            other.transform.position = new Vector3(transform.position.x, transform.position.y + yLockOffset, transform.position.z);
        }
    }
    
}
