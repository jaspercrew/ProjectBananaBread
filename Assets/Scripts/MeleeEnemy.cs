using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : CloseAttackerEnemy
{
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float knockbackVal = 2f;
    [SerializeField] protected float attackRange = .25f;
    protected bool isAttacking;
    protected const int AttackDamage = 10;
    [SerializeField] protected LayerMask playerLayers;
    protected IEnumerator attackCo;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        attackCD = 2f;
    }
    

    protected override bool AbleToMove()
    {
        return base.AbleToMove() && !isAttacking;
    }

    protected override void DoAttack()
    {
        isAttacking = true;
        Animator.SetTrigger(Attack);
        StartCoroutine(attackCo = AttackCoroutine());
    }

    protected IEnumerator AttackCoroutine()
    {
        Rigidbody.velocity = Vector2.ClampMagnitude(Rigidbody.velocity, .01f);
        // enemy attack modifiers
        // float attackBoost = 1.5f;
        const float beginAttackDelay = .55f;
        const float hitEndDelay = .4f;

        yield return new WaitForSeconds(beginAttackDelay);

        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
            hitColliders, playerLayers);

        if (numHits > 0) {
            foreach (Collider2D p in hitColliders) {
                if (p != null && p.gameObject.GetComponent<CharController>() != null && charController.isParrying) {
                    // StartCoroutine(PauseAnimatorCoroutine(.2f));
                    charController.CounterStrike(GetComponent<Enemy>());
                }
                else if (p != null && p.gameObject.GetComponent<CharController>() != null)
                {
                    charController.TakeDamage(AttackDamage);
                }
            }
        }
        
        yield return new WaitForSeconds(hitEndDelay);
        isAttacking = false;
    }

    public override void Interrupt() { // should stop all relevant coroutines
        if (attackCo != null) {
            StopCoroutine(attackCo); // interrupt attack if take damage
            Rigidbody.velocity = Vector2.zero;
            isAttacking = false;
        }
    }
    protected override void DisableFunctionality() {
        StopAllCoroutines();
        canFunction = false;
    }

    protected override void EnableFunctionality() {
        canFunction = true;
        isAttacking = false;
    }
}
