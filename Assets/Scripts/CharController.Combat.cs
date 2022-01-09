using System;
using System.Collections;
using UnityEngine;

public partial class CharController {
    private void DoParry()
    {
        if (!IsAbleToAct())
            return;
        // start parry animation
        Animator.SetTrigger(Parry);
        isParrying = true;
        StartCoroutine(ParryCoroutine());
        
        lastParryTime = Time.time;
    }

    // Take damage, knock away from point
    public void TakeDamage(int damage, float knockback, Vector2 point) {
        if (!IsAbleToBeDamaged()) {
            return;
        }
        StartCoroutine(TakeDamageCoroutine());
        KnockAwayFromPoint(knockback, point);
        CurrentHealth -= damage;
        // damage animation
        Animator.SetTrigger(Hurt);
        
        if (CurrentHealth <= 0) {
            Die();
        }
    }

    private IEnumerator TakeDamageCoroutine() {
        screenShakeController.MediumShake();
        isInvincible = true;
        const float invFrames = .2f;
        yield return new WaitForSeconds(invFrames);
        isInvincible = false;
    }

    protected override void Die() 
    {
        Animator.SetTrigger(Death);
        transform.GetComponent<Collider>().enabled = false;
        Rigidbody.gravityScale = 0;
    }

    // handles combo count for attacking 
    private void AttemptAttack()
    {
        if (!IsAbleToAct())
            return;
        
        // reset if combo reset threshold passed
        if (lastAttackTime + ComboResetThreshold < Time.time) {
            comboCounter = 0;
        }

        comboCounter++;
        // this line is equivalent to the 3 commented lines below
        bool isHeavy = Math.DivRem(comboCounter, HeavyAttackBuildup, out comboCounter) > 0;

        // bool isHeavy = comboCounter == HeavyAttackBuildup;
        // if (isHeavy)
        //     comboCounter -= HeavyAttackBuildup;
        
        DoAttack(isHeavy);
    }

    private IEnumerator AttackCoroutine(bool isHeavyAttack) {
        // light attack modifiers
        float attackBoost = 2.5f;
        float beginAttackDelay = .15f;
        float endAttackDelay = .2f;
        float hitConfirmDelay = .20f;
        
        // heavy attack modifiers
        if (isHeavyAttack) { 
            attackBoost = 3.0f;
            beginAttackDelay = .25f;
            endAttackDelay = .5f;
            hitConfirmDelay = .30f;
        }
    
        yield return new WaitForSeconds(beginAttackDelay);
        
        
        // move while attacking TODO : change this functionality
        if (IsGrounded()) {
            if (moveVector > .5) {
                VelocityDash(1, attackBoost, .5f);
            }
            else if (moveVector < -.5) {
                VelocityDash(3, attackBoost, .5f);
            }
        }

        const int maxEnemiesHit = 20;
        Collider2D[] hitColliders = new Collider2D[maxEnemiesHit];
        
        // scan for hit enemies
        int numHitEnemies = Physics2D.OverlapCircleNonAlloc(
            attackPoint.position, attackRange, hitColliders, enemyLayers);

        if (numHitEnemies > 0) {
            // pause swing animation if an enemy is hit
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); 
            screenShakeController.MediumShake();
        }

        foreach (Collider2D enemy in hitColliders)
        {
            if (enemy is null)
                break;
            enemy.GetComponent<Enemy>().TakeDamage(AttackDamage, isHeavyAttack ? 2f : 1f);
        }
        yield return new WaitForSeconds(endAttackDelay);
        isAttacking = false;
    }

    private void DoAttack(bool isHeavy) {
        // _screenShakeController.LightShake();
        isAttacking = true;
        Animator.speed = 1;
        // Assert.IsTrue(comboCount <= HeavyAttackBuildup);
        
        Animator.SetTrigger(Attack);
        // TODO: remove when we have actual animations
        if (isHeavy) { // heavy attack?
            Animator.speed = .5f;
        }

        attackCoroutine = AttackCoroutine(isHeavy);
        StartCoroutine(attackCoroutine);
        
        lastAttackTime = Time.time;
    }

    private IEnumerator ParryCoroutine() {
        const float parryTime = .5f;
        yield return new WaitForSeconds(parryTime);
        isParrying = false;
        //transform.GetComponent<SpriteRenderer>().flipY = false;
    }

    public void CounterStrike(Enemy enemy) {
        isAttacking = true;
        // start counter animation
        StartCoroutine(CounterCoroutine(enemy));
    }

    private IEnumerator CounterCoroutine(Enemy enemy) {
        const float counterTime = .2f;
        yield return new WaitForSeconds(counterTime);
        screenShakeController.MediumShake();
        enemy.TakeDamage(20, 2);
        isAttacking = false;
    }
}
