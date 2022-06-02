using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Android;

public partial class CharController
{
    private Rigidbody2D castProjectileRb;
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
        if (!IsAbleToAct())
            return;
        isParrying = true;
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        // start parry animation
        Animator.SetTrigger(Parry);
        StartCoroutine(ParryCoroutine());
        lastParryTime = Time.time;
    }

    private void DoInteract() {
        foreach (Interactor i in Interactor.interactors) {
            i.Interact();
        }
    }

    private void CauseSwitch()
    {
        lastShiftTime = Time.time;
        PPManager.Instance.ShiftEffect(!GameManager.Instance.isGameShifted);
        transform.Find("ShiftCD").GetComponent<ShiftCD>().image.fillAmount = 1; //TODO: FIX
        switchPS.Play();
        
        GameManager.Instance.ShiftWorld();
        //StartCoroutine(WindDetectionCheck());
    }
    
    // Take damage, knock away from point
    public void TakeDamage(int damage) {
        if (!IsAbleToBeDamaged()) {
            return;
        }
        CurrentHealth -= damage;
        GameManager.Instance.FreezeFrame();
        UIManager.Instance.CheckHealth();
        screenShakeController.MediumShake();
        StartCoroutine(TakeDamageCoroutine());
        Animator.SetTrigger(Hurt);
        StartCoroutine(InvFrameCoroutine(InvTime));
        
        if (CurrentHealth <= 0) {
            Die();
        }
    }

    private IEnumerator TakeDamageCoroutine() {
        isInvincible = true;
        
        yield return new WaitForSeconds(InvTime);
        isInvincible = false;
    }

    protected void Die() 
    {
        Animator.SetTrigger(Death);
        //transform.GetComponent<Collider>().enabled = false;
        Rigidbody.gravityScale = 0;
        GameManager.Instance.PlayerDeath();
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

        
        //bool isHeavy = Math.DivRem(comboCounter, HeavyAttackBuildup, out comboCounter) > 0;

        DoAttack(comboCounter % 3);
        comboCounter++;
    }

    private IEnumerator AttackCoroutine(int combo) {
        print("attackco" + combo);
        // light attack modifiers
        float attackBoost = 3.0f;
        float beginAttackDelay = .32f;
        float endAttackDelay = .30f;

        if (combo == 1) { 
            attackBoost = 3.0f;
            beginAttackDelay = .25f;
            endAttackDelay = .15f;
        }

        else if (combo == 2) { 
            attackBoost = 6.0f;
            beginAttackDelay = .35f;
            endAttackDelay = .5f;
        }

        if (Rigidbody.velocity.x > 0)
        {
            Rigidbody.velocity = 
                new Vector2(Math.Min(Rigidbody.velocity.x, .01f), Rigidbody.velocity.y);
        }
        else
        {
            Rigidbody.velocity = 
                new Vector2(Math.Max(Rigidbody.velocity.x, -.01f), Rigidbody.velocity.y);
        }
        
        if (isGrounded) {
            Rigidbody.AddForce(new Vector2(moveVector * attackBoost, 0), ForceMode2D.Impulse);
            //Rigidbody.velocity = new Vector2(moveVector * attackBoost, Rigidbody.velocity.y);
        }
        
        yield return new WaitForSeconds(beginAttackDelay);
        
        
        const int maxEnemiesHit = 20;
        Collider2D[] hitColliders = new Collider2D[maxEnemiesHit];
        
        // scan for hit enemies
        Physics2D.OverlapCircleNonAlloc(
            attackPoint.position, attackRange, hitColliders, enemyLayers);

        bool hit = false;
        foreach (Collider2D obj in hitColliders)
        {
            if (obj == null)
            {
                break;
            }
            IHittableEntity hitComponent = obj.GetComponent<IHittableEntity>();
            if (hitComponent != null)
            {
                hitComponent.GetHit(AttackDamage);
                hit = true;
            }

            
            // if (enemy is null)
            //     break;
            // if (enemy.GetComponent<Enemy>() != null)
            //     enemy.GetComponent<Enemy>().TakeDamage(AttackDamage);
            // else if (enemy.GetComponent<HittableEntity>() != null)
            //     enemy.GetComponent<HittableEntity>().GetHit();
            // hit = true;
        }

        if (hit)
        {
            fury += FuryIncrement;
            if (fury > MaxFury)
            {
                fury = MaxFury;
            }
            UIManager.Instance.CheckFury();
            screenShakeController.MediumShake();
            AudioManager.Instance.Play(SoundName.Hit, .5f);
        }
        
        yield return new WaitForSeconds(3 * endAttackDelay / 4);
        
        if (Rigidbody.velocity.x > 0)
        {
            Rigidbody.velocity = 
                new Vector2(Math.Min(Rigidbody.velocity.x, .1f), Rigidbody.velocity.y);
        }
        else
        {
            Rigidbody.velocity = 
                new Vector2(Math.Max(Rigidbody.velocity.x, -.1f), Rigidbody.velocity.y);
        }
        yield return new WaitForSeconds(1 * endAttackDelay / 4);
        
        isAttacking = false;
    }

    private void DoAttack(int combo) {
        // _screenShakeController.LightShake();
        isAttacking = true;
        //Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        Animator.speed = 1;
        // Assert.IsTrue(comboCount <= HeavyAttackBuildup);
        switch (combo) {
            case 0:
                Animator.SetTrigger(AttackA);
                break;
            case 1:
                Animator.SetTrigger(AttackB);
                break;
            case 2:
                Animator.SetTrigger(AttackC);
                break;
            default:
                break;
        }
        
        
        // TODO: remove when we have actual animations
        // if (isHeavy) { // heavy attack?
        //     Animator.speed = .5f;
        // }

        attackCoroutine = AttackCoroutine(combo);
        toInterrupt.Add(attackCoroutine);
        StartCoroutine(attackCoroutine);
        
        lastAttackTime = Time.time;
    }

    private IEnumerator ParryCoroutine() {
        yield return new WaitForSeconds(ParryTime);
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
        enemy.TakeDamage(20);
        isAttacking = false;
    }

    private void DoCast()
    {
        const float castSpeed = 65f;
        castProjectileRb = Instantiate(castProjectileInput, attackPoint.position, transform.rotation);
        if (Camera.main == null)
            return;
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        castProjectileRb.GetComponentInParent<BladeProjectile>()
            .Initialize((mousePos - transform.position).normalized, castSpeed);
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
        castProjectileRb.GetComponentInParent<BladeProjectile>().Callback();
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
