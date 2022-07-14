using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BouncePad : ActivatedEntity
{
    public float bounceVelocity;
    private const float charMultiplier = .8f;
    public bool isInverted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActive)
        {
            return;
        }
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            //print("force added");
            if (other.gameObject.GetComponent<CharController>() == null)
            {
                rb.velocity = new Vector2(0, isInverted ? -bounceVelocity : bounceVelocity);
            }
            else
            {
                rb.velocity = new Vector2(rb.velocity.x, isInverted ? 
                    (-bounceVelocity * charMultiplier) : (bounceVelocity * charMultiplier));
            }
        }
    }

}
