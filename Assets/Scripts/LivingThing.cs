using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingThing : Entity {
    //Configurable values 
    protected int maxHealth = 100;
    //Trackers

    protected int currentHealth;
    protected Animator animator_;
    protected Rigidbody2D _rigidbody;
    protected bool isDashing = false;

    // animator values beforehand to save time later
    protected static readonly int AnimState = Animator.StringToHash("AnimState");
    protected static readonly int Idle = Animator.StringToHash("Idle");
    protected static readonly int Jump = Animator.StringToHash("Jump");
    protected static readonly int Hurt = Animator.StringToHash("Hurt");
    protected static readonly int Death = Animator.StringToHash("Death");
    protected static readonly int Grounded = Animator.StringToHash("Grounded");
    protected static readonly int Attack = Animator.StringToHash("Attack");

    public virtual void TakeDamage(int damage) {
        currentHealth -= damage;
        //damage animation
        animator_.SetTrigger(Hurt);
        
        if (currentHealth <= 0) {
            Die();
        }
    }
    
    //boosts the gameobject in a certain cardinal direction 
    protected void VelocityDash(int cardinalDirection, float dashSpeed, float dashTime) {
        isDashing = true;
        StartCoroutine(DashCoroutine(dashTime));
        switch (cardinalDirection) {
            case 0:
                _rigidbody.velocity = new Vector2(0, dashSpeed);
                break;
            case 1:
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x + dashSpeed, _rigidbody.velocity.y);
                break;
            case 2:
                _rigidbody.velocity = new Vector2(0, -dashSpeed);
                break;
            case 3:
                _rigidbody.velocity = new Vector2(_rigidbody.velocity.x - dashSpeed, _rigidbody.velocity.y);
                break;
            default:
                Debug.Log("invalid dash direction");
                break;
        }
    }

    //dash coroutine handles stopping the dash
    protected IEnumerator DashCoroutine(float dashTime) {
        yield return new WaitForSeconds(dashTime);
        _rigidbody.velocity = new Vector2(0, _rigidbody.velocity.y);
        isDashing = false;
    }
    
    protected virtual void OnLanding() {
        animator_.SetBool(Grounded, true);
        //Debug.Log("sus");
    }
    
    //pauses the animator for pausetime
    protected IEnumerator PauseAnimatorCoroutine(float pauseTime) {
        float temp = animator_.speed;
        animator_.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        animator_.speed = temp;
    }
    
    //knock this object away from point with velocity vel 
    protected void KnockAwayFromPoint(float vel, Vector3 point) {
        _rigidbody.velocity = vel * (transform.position - point).normalized;
    }
    
    

    protected virtual void Die() {
        animator_.SetTrigger(Death);
        transform.GetComponent<Collider>().enabled = false;
        _rigidbody.gravityScale = 0;
    }

    protected void Stun(float stunTime) {
        
    }




}
