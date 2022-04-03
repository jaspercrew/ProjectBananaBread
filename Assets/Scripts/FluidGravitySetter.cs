using System;
using UnityEngine;

public class FluidGravitySetter : BinaryEntity
{
    [Serializable]
    public class GravityInfo
    {
        public bool isDown;
        public bool isEnabled;
    }
    // private Collider2D collider;
    public GravityInfo realStateGravity;
    public GravityInfo altStateGravity;
    
    [HideInInspector]
    public GravityInfo currentGravity;
    
    protected override void Awake()
    {
        CheckEntity();
    }

    protected override void TurnShifted()
    { // set current gravity in these to be correct
        base.TurnShifted();
    }
    
    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
    
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
        else if (rb != null)
        {
            GravityInfo toSetTo = GameManager.Instance.isGameShifted ? altStateGravity : realStateGravity;
            if (toSetTo.isEnabled)
            {
                int dir = toSetTo.isDown ? 1 : -1;
                float strength = Mathf.Abs(rb.gravityScale);
                rb.gravityScale = dir * strength;
            }
        }
    }
}
