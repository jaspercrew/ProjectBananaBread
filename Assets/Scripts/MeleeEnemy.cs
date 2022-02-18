using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeleeEnemy : Enemy
{
    [SerializeField] protected Transform attackPoint;
    [SerializeField] protected float knockbackVal = 2f;
    [SerializeField] protected float attackRange = .25f;
    protected bool isAttacking;
    protected const float AttackRate = .75f;
    protected const int AttackDamage = 10;
    [SerializeField] protected LayerMask playerLayers;
    protected float nextAttackTime;
    protected IEnumerator attackCo;
    
    // Start is called before the first frame update
    private void Start()
    {
        InitializeEnemy();
    }

    private void Update()
    {
        Pathfind_Update();
        ScanForAttack_Update();
        TurnAround_Update();
    }

    protected override bool AbleToMove()
    {
        return canFunction && !isAttacking;
    }

    protected void DoAttack()
    {
        isAttacking = true;
        Animator.SetTrigger(Attack);
        StartCoroutine(attackCo = AttackCoroutine());
    }

    protected IEnumerator AttackCoroutine() {
        // enemy attack modifiers
        // float attackBoost = 1.5f;
        const float beginAttackDelay = .55f;
        const float hitConfirmDelay = .20f;
        const float hitEndDelay = .4f;

        yield return new WaitForSeconds(beginAttackDelay);

        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
            hitColliders, playerLayers);
        if (numHits > 0) {
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); // pause swing animation if an enemy is hit
        }

        if (hitColliders.Length > 0) {
            foreach (Collider2D p in hitColliders) {
                if (p != null && p.gameObject.GetComponent<CharController>() != null && charController.isParrying) {
                    // StartCoroutine(PauseAnimatorCoroutine(.2f));
                    charController.CounterStrike(GetComponent<Enemy>());
                }
                else if (p != null && p.gameObject.GetComponent<CharController>() != null)
                {
                    charController.TakeDamage(AttackDamage, knockbackVal, transform.position);
                }
            }
        }
        
        yield return new WaitForSeconds(hitEndDelay);
        isAttacking = false;
        Debug.Log(AbleToMove());
    }
    
    protected void ScanForAttack_Update() {
        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
            hitColliders, playerLayers);
        if (numHits > 0 && Time.time >= nextAttackTime) {
            DoAttack();
            nextAttackTime = Time.time + 1f / AttackRate;
        }
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
