/*using System;
using UnityEngine;

public class BeatWave : MonoBehaviour
{
    private Vector2 playerVelocityMultiplier;
    private Rigidbody2D rigidbody2D;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var playerRb = CharController.instance.rigidbody;
            CharController.instance.recentImpulseTime = .5f;
            //CharController.Instance.Rigidbody.velocity = Vector2.zero;
            var toAdd = Vector2.Scale(rigidbody2D.velocity, playerVelocityMultiplier);
            if (Math.Sign(playerRb.velocity.x) != Math.Sign(toAdd.x))
                playerRb.velocity = new Vector2(0, playerRb.velocity.y);
            if (Math.Sign(playerRb.velocity.y) != Math.Sign(toAdd.y))
                playerRb.velocity = new Vector2(playerRb.velocity.x, 0);

            playerRb.velocity += toAdd;
            CharController.instance.forcedMoveTime = .3f;
            CharController.instance.forcedMoveVector = 0;
            Destroy(gameObject);
        }
    }

    public void Initialize(Vector2 vel, Vector2 scalar)
    {
        rigidbody2D = GetComponent<Rigidbody2D>();
        playerVelocityMultiplier = scalar;
        rigidbody2D.velocity = vel;
    }
}*/

