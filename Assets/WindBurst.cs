using System;
using System.Collections;
using UnityEngine;

public class WindBurst : BeatEntity
{
    public Vector2 direction;
    public float velocity;
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
            CharController.Instance.GetComponent<Rigidbody2D>().AddForce(direction * velocity, ForceMode2D.Impulse);
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
