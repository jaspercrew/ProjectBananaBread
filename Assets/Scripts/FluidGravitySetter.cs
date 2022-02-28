using System;
using UnityEngine;

public class FluidGravitySetter : BinaryEntity
{
    private Collider2D collider;
    private void Awake()
    {
        CheckEntity(GameManager.Instance.currentState);
    }

    protected override void ShiftEntity()
    {
        base.ShiftEntity();
    }
    
    protected override void DeshiftEntity()
    {
        base.DeshiftEntity();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (isShifted)
            {
                CharController.Instance.Invert();
            }
            else
            {
                CharController.Instance.DeInvert();
            }
            
        }
    }
}
