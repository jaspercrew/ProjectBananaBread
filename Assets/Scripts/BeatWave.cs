using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatWave : MonoBehaviour
{
    private Rigidbody2D rigidbody2D;
    private Vector2 playerVelocityMultiplier;



    public void Initialize(Vector2 vel, Vector2 scalar)
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerVelocityMultiplier = scalar;
        rigidbody2D.velocity = vel;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            Rigidbody2D playerRB = CharController.Instance.Rigidbody;
            CharController.Instance.recentImpulseTime = .5f;
            //CharController.Instance.Rigidbody.velocity = Vector2.zero;
            Vector2 toAdd = Vector2.Scale(rigidbody2D.velocity, playerVelocityMultiplier);
            if (Math.Sign(playerRB.velocity.x) != Math.Sign(toAdd.x))
            {
                playerRB.velocity = new Vector2(0, playerRB.velocity.y);
            }
            if (Math.Sign(playerRB.velocity.y) != Math.Sign(toAdd.y))
            {
                playerRB.velocity = new Vector2(playerRB.velocity.x, 0);
            }

            playerRB.velocity += toAdd;
            CharController.Instance.forcedMoveTime = .3f;
            CharController.Instance.forcedMoveVector = 0 ;
            Destroy(gameObject);
        }
    }
    
    

}
