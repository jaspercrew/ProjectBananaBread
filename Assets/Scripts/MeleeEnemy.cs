using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : CloseAttackerEnemy
{
    
    //public float meleeRadius = .25f;
    //public float knockbackVal = 2f;
    public float yOffset = .5f;
    public int AttackDamage = 10;
    public LayerMask playerLayers;
    
    const int maxHits = 20;
    protected Vector2 attackPoint;
    protected bool isAttacking;
    
    
    protected IEnumerator attackCo;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        attackCD = 2f;
    }
    protected override void PlayerScan_Update()
    {
        attackPoint = new Vector2(transform.position.x + (transform.localScale.x < 0 ? attackRange : -attackRange), transform.position.y + yOffset);
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint, attackRange,
            hitColliders, playerLayers);
        if (numHits > 0)
        {
            playerInAttackRange = true;
        }
        else
        {
            playerInAttackRange = false;
        }
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

    protected override bool AttackConditions()
    {
        return base.AttackConditions() && !isAttacking;
    }

    protected IEnumerator AttackCoroutine()
    {
        Rigidbody.velocity = Vector2.ClampMagnitude(Rigidbody.velocity, .01f);
        // enemy attack modifiers
        const float beginAttackDelay = .55f;
        const float hitEndDelay = .4f;

        yield return new WaitForSeconds(beginAttackDelay);

        
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint, attackRange,
            hitColliders, playerLayers);

        if (numHits > 0)
        {
            foreach (Collider2D col in hitColliders)
            {
                if (col.gameObject.GetComponent<CharController>() != null && charController.isParrying) {
                    charController.CounterStrike(GetComponent<Enemy>());
                    break;
                }
                if (col.gameObject.GetComponent<CharController>() != null)
                {
                    charController.TakeDamage(AttackDamage);
                    break;
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
