using System;
using System.Collections;
using UnityEngine;

public class ChargerEnemy : CloseAttackerEnemy
{
    private bool isWaiting;
    private bool isCharging;
    private int chargeDir;
    private bool alreadyHit;
    
    public float chargeDelay;
    public float chargeVelocity;
    public float chargeTime;
    public int chargeDamage;

    protected override void DoAttack()
    {
        isWaiting = true;
        StartCoroutine(Wait_Coroutine());
    }

    protected IEnumerator Wait_Coroutine()
    {
        yield return new WaitForSeconds(chargeDelay);
        isWaiting = false;
        Animator.SetTrigger(Attack);
        isCharging = true;
        alreadyHit = false;
        if (charController.transform.position.x > transform.position.x)
        {
            chargeDir = 1;
        }
        else
        {
            chargeDir = -1;
        }

        Rigidbody.velocity = new Vector2(chargeVelocity * chargeDir, 0);
        StartCoroutine(AttackCoroutine());
    }

    protected IEnumerator AttackCoroutine()
    {
        yield return new WaitForSeconds(chargeTime);
        isCharging = false;
        Rigidbody.velocity = Vector2.zero;
    }

    protected override bool AttackConditions()
    {
        return base.AttackConditions() && !isCharging && !isWaiting;
    }

    protected override bool AbleToMove()
    {
        return base.AbleToMove() && !isCharging && !isWaiting;
    }

    protected virtual void CheckCollision_Update()
    {
        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(transform.position, 1f,
            hitColliders, playerMask);
        if (numHits > 0 && !alreadyHit)
        {
            alreadyHit = true;
            if (charController.isParrying)
            {
                isCharging = false;
                charController.CounterStrike(this);
                return;
            }
            charController.TakeDamage(chargeDamage);
        }
    }

    protected override void Update()
    {
        base.Update();
        CheckCollision_Update();
    }
    

    // protected override void OnCollisionEnter2D(Collision2D other)
    // {
    //     base.OnCollisionEnter2D(other);
    //     if (other.gameObject.CompareTag("Player") && isCharging)
    //     {
    //         if (charController.isParrying)
    //         {
    //             isCharging = false;
    //             charController.CounterStrike(this);
    //             return;
    //         }
    //         charController.TakeDamage(chargeDamage);
    //     }
    // }
}