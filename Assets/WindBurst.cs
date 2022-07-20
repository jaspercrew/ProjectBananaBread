using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class WindBurst : BeatEntity
{
    public Vector2 direction;
    public float windVelocity;
    private bool playerInRange;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void BeatAction()
    {
        if (playerInRange)
        {
            //Vector2 vel = CharController.Instance.GetComponent<Rigidbody2D>().velocity;
            CharController.Instance.GetComponent<Rigidbody2D>().velocity = windVelocity * direction.normalized;

        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
