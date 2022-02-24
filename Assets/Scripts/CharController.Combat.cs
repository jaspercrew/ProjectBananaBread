using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public partial class CharController
{
    private Rigidbody2D castProjectileRB;
    public Rigidbody2D castProjectileInput;
    private void DoSliceDash() {
        if (!IsAbleToAct()) {
             return;
        }
        isSliceDashing = true;
        const float dashSpeed = 40f;
        const float dashTime = .15f;

        float xScale = transform.localScale.x; 
        VelocityDash(xScale > 0? -dashSpeed : dashSpeed, dashTime);
        sliceDashPS.Play();
        Animator.SetTrigger(Dash);
        StartCoroutine(SliceDashCoroutine(dashTime));

        lastDashTime = Time.time;
    }

    private IEnumerator SliceDashCoroutine(float dashTime) {
        yield return new WaitForSeconds(dashTime * 3);
        isSliceDashing = false;
    }

    private IEnumerator SliceExecuteCoroutine(Enemy enemy) {
        
        enemy.Stun(2);
        Rigidbody.velocity = Vector2.zero;
        Transform t = transform; // more efficient, according to Rider
        t.localScale = new Vector3(-t.localScale.x, t.localScale.y, 0);
        isDashing = false;
        isSliceDashing = false;
        isAttacking = true;
        
        yield return new WaitForSeconds(1.25f);
        Animator.SetTrigger(Attack);
        if (enemy.CurrentHealth < SliceDamage) { //execute
                
        }
        else {
            enemy.TakeDamage(SliceDamage);
        }
        yield return new WaitForSeconds(.5f);
        isAttacking = false;
    }
    
    private void DoParry() {
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        if (!IsAbleToAct())
            return;
        // start parry animation
        Animator.SetTrigger(Parry);
        isParrying = true;
        StartCoroutine(ParryCoroutine());
        
        lastParryTime = Time.time;
    }

    private void DoInteract() {
        //Debug.Log(Interactor.interactors.Count);
        foreach (Interactor i in Interactor.interactors) {
            i.Interact();
        }
    }

    public void CauseSwitch()
    {
        PPManager.Instance.ShiftEffect(GameManager.Instance.currentState == GameManager.Instance.originalState);
        switchPS.Play();
        
        GameManager.Instance.SwitchWorldState();
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
        float attackBoost = 1.5f;
        float beginAttackDelay = .12f;
        float endAttackDelay = .35f;
        float hitConfirmDelay = .20f;
        
        // heavy attack modifiers
        if (isHeavyAttack) { 
            attackBoost = 2.0f;
            beginAttackDelay = .25f;
            endAttackDelay = .5f;
            hitConfirmDelay = .30f;
        }
    
        yield return new WaitForSeconds(beginAttackDelay);
        
        
        // move while attacking TODO : change this functionality
        if (isGrounded) {
            if (moveVector > .5) {
                Rigidbody.velocity = new Vector2(attackBoost, Rigidbody.velocity.y);
            }
            else if (moveVector < -.5) {
                Rigidbody.velocity = new Vector2(-attackBoost, Rigidbody.velocity.y);
            }
            else {
                Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
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

        bool hit = false;
        
        foreach (Collider2D enemy in hitColliders)
        {
            if (enemy is null)
                break;
            enemy.GetComponent<Enemy>().TakeDamage(AttackDamage, isHeavyAttack ? 2f : 1f);
            hit = true;
        }
        
        if (hit)
            AudioManager.Instance.Play(SoundName.Hit, .5f);
        
        yield return new WaitForSeconds(endAttackDelay);
        isAttacking = false;
    }

    private void DoAttack(bool isHeavy) {
        // _screenShakeController.LightShake();
        isAttacking = true;
        //Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
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
        const float parryTime = .7f;
        yield return new WaitForSeconds(parryTime);
        isParrying = false;
        //transform.GetComponent<SpriteRenderer>().flipY = false;
    }

    public void CounterStrike(Enemy enemy)
    {
        isAttacking = true;
        // start counter animation
        
       // IEnumerator switchParticleCoroutine; 
        //switchParticleCoroutine = 

        StartCoroutine(ParticleBurstCoroutine(parryPS, 0.3f));
        StartCoroutine(CounterCoroutine(enemy));
    }

    private IEnumerator CounterCoroutine(Enemy enemy) {
        const float counterTime = .2f;

        yield return new WaitForSeconds(counterTime);
        screenShakeController.MediumShake();
        AudioManager.Instance.Play(SoundName.Hit, .5f);
        enemy.TakeDamage(20, 2);
        isAttacking = false;
    }

    private void DoCast()
    {
        const float castSpeed = 35f;
        castProjectileRB = Instantiate(castProjectileInput, attackPoint.position, transform.rotation);
        castProjectileRB.GetComponentInParent<BladeProjectile>().Initialize(((Camera.main).ScreenToWorldPoint(Input.mousePosition) - transform.position).normalized, castSpeed);
        StartCoroutine(CastCoroutine());
        canCast = false;
    }

    private IEnumerator CastCoroutine()
    {
        const float cooldown = .3f;
        yield return new WaitForSeconds(cooldown);
        canYoink = true;
        
    }

    private void DoYoink()
    {
        castProjectileRB.GetComponentInParent<BladeProjectile>().Callback();
        canYoink = false;
        //StartCoroutine(YoinkCoroutine());

    }

    public void ReturnCast()
    {
        StartCoroutine(YoinkCoroutine());
    }
    
    private IEnumerator YoinkCoroutine()
    {
        const float cooldown = .3f;
        yield return new WaitForSeconds(cooldown);
        canCast = true;
    }
}
