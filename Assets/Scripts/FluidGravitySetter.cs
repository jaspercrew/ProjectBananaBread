using System;
using UnityEngine;

public class FluidGravitySetter : BinaryEntity
{
    // private Collider2D collider;
    
    protected override void Awake()
    {
        CheckEntity();
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
    }
    
    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (GameManager.Instance.isGameShifted)
            {
                CharController.Instance.Invert();
            }
            else
            {
                CharController.Instance.DeInvert();
            }
            
        }
        
        else if (other.gameObject.GetComponent<Rigidbody2D>() != null)
        {
            if (GameManager.Instance.isGameShifted)
            {
                other.gameObject.GetComponent<Rigidbody2D>().gravityScale =
                    Mathf.Abs(other.gameObject.GetComponent<Rigidbody2D>().gravityScale);
            }
            else
            {
                other.gameObject.GetComponent<Rigidbody2D>().gravityScale =
                    -Mathf.Abs(other.gameObject.GetComponent<Rigidbody2D>().gravityScale);
            }
        }
    }
}
