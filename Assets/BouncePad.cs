using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class BouncePad : ActivatedEntity
{
    public float bounceVelocity;
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
            rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y + (isInverted ? -bounceVelocity : bounceVelocity));
        }
    }

}
