using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class ConsumableLightZone : BinaryEntity
{
    public bool beenConsumed;
    public float radius;
    
    public bool activeInReal;
    public bool activeInAlt;
    private bool isActive;

    private Collider2D collider2D;

    protected override void Start()
    {
        base.Start();
        beenConsumed = false;
        collider2D = transform.GetComponent<Collider2D>();
    }
    
    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (activeInAlt)
        {
            isActive = true;
        }
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (activeInReal)
        {
            isActive = true;
        }
    }
    

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player") && !beenConsumed && isActive)
        {
            CharController.Instance.lightBuffer = CharController.maxLightBuffer;
            Extinguish();
        }
    }

    private void Extinguish()
    {
        beenConsumed = true;
    }
}