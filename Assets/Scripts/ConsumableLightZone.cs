using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;

public class ConsumableLightZone : BinaryEntity
{
    private const float intensity = 1f;
    public bool beenConsumed;
    public float radius;
    
    public bool activeInReal;
    public bool activeInAlt;
    private bool isActive;

    private Collider2D collider2D;
    private Light2D light2d;

    protected override void Start()
    {
        beenConsumed = false;
        collider2D = transform.GetComponent<Collider2D>();
        light2d = transform.Find("Light").GetComponent<Light2D>();
        base.Start();
    }
    
    
    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (!beenConsumed)
        {
            if (activeInAlt)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
        }
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (!beenConsumed)
        {
            if (activeInReal)
            {
                Activate();
            }
            else
            {
                Deactivate();
            }
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

    private void Activate()
    {
        light2d.intensity = intensity;
        isActive = true;
    }

    private void Deactivate()
    {
        light2d.intensity = 0f;
        isActive = false;
    }

    private void Extinguish()
    {
        light2d.intensity = 0f;
        beenConsumed = true;
    }
}