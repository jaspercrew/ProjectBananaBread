using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

public class Enemy : LivingThing
{
    // Attacking
    protected float speed = 3f;
    // [SerializeField] private int moveState; // determines movement behavior
    
    // Trackers
    // private float moveVector = 0f;
    protected EnvironmentState originalState = 0;
    protected bool canFunction;

    protected AIPath aiPath;
    protected CharController charController;

    // Start is called before the first frame update

    protected void InitializeEnemy()
    {
        canFunction = true;
        CurrentHealth = MaxHealth;
        aiPath = GetComponent<AIPath>();
        Animator = transform.GetComponentInChildren<Animator>();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        charController = FindObjectOfType<CharController>();
        if (originalState == 0)
            originalState = EntityState;
    }

    public void TakeDamage(int damage, float knockback) { // assumes damage is taken from PLAYER
        if (canFunction) {
            Interrupt();
            GameObject player = GameObject.FindWithTag("Player");
            KnockAwayFromPoint(knockback, player.transform.position);
            CurrentHealth -= damage;
            // damage animation
            Animator.SetTrigger(Hurt);

            if (CurrentHealth <= 0) {
                Die();
            }
        }
    }

    protected void Pathfind_Update()
    {

        
    }

    public virtual void Interrupt() { // should stop all relevant coroutines
    }
    

    protected override void Die() {
        const float deathTime = 1f;
        DisableFunctionality();
        Animator.SetTrigger(Death);
        Destroy(gameObject, deathTime);
    }
    
    
    protected bool IsMovementEnabled()
    {
        return true;
    }
    
    // Update is called once per frame


    protected void TurnAround_Update() {
        if (aiPath.desiredVelocity.x > 0) {
            FaceRight();
        }
        else if (aiPath.desiredVelocity.x < 0) {
            FaceLeft();
        }
        Animator.SetInteger(AnimState, Mathf.Abs(aiPath.velocity.x) > .1 ? 2 : 0);
    }


    public override void Stun(float stunTime) {
        Interrupt();
        DisableFunctionality();
        StartCoroutine(StunCoroutine(stunTime));
    }

    public override IEnumerator StunCoroutine(float stunTime) {
        yield return new WaitForSeconds(stunTime);
        EnableFunctionality();
    }
    
    // private void OnDrawGizmosSelected() {
    //     if (attackPoint == null) {
    //         return;
    //     }
    //     Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    // }

    protected void DisableFunctionality() {
        StopAllCoroutines();
        canFunction = false;
        aiPath.canMove = false;
    }

    protected void EnableFunctionality() {
        canFunction = true;
        aiPath.canMove = true;
    }
}
