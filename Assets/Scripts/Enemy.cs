using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

public class Enemy : LivingThing
{
    // Attacking
    protected float speed = 5f;
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
            ParticleSystem gorePS = transform.Find("Particles").Find("GorePS").GetComponent<ParticleSystem>();
            ParticleSystem.ShapeModule shape = gorePS.shape;
            if (transform.position.x < charController.transform.position.x)
            {
                shape.rotation = new Vector3(0, 0, 145);
            }
            else
            {
                shape.rotation = new Vector3(0, 0, 0);
            }
            gorePS.Play();

            if (CurrentHealth <= 0) {
                Die();
            }
        }
    }

    protected virtual bool AbleToMove()
    {
        return canFunction;
    }

    protected void Pathfind_Update()
    {
        if (AbleToMove())
        {
            const float buffer = .5f;
            if (charController.transform.position.x > transform.position.x + buffer)
            {
                Rigidbody.velocity = new Vector2(speed, Rigidbody.velocity.y);
            }
            else if (charController.transform.position.x < transform.position.x - buffer)
            {
                Rigidbody.velocity = new Vector2(-speed, Rigidbody.velocity.y);
            }
        }
        else
        {
            Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        }
        
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
        if (Rigidbody.velocity.x > 0) {
            FaceRight();
        }
        else if (Rigidbody.velocity.x < 0) {
            FaceLeft();
        }
        Animator.SetInteger(AnimState, Mathf.Abs(Rigidbody.velocity.x) > .1 ? 2 : 0);
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

    protected virtual void DisableFunctionality() {
        StopAllCoroutines();
        canFunction = false;
    }

    protected virtual void EnableFunctionality() {
        canFunction = true;
    }
}
