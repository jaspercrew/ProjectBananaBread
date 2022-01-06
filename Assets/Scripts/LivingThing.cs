using System.Collections;
using UnityEngine;

public abstract class LivingThing : Entity {
    // Configurable values 
    protected const int MaxHealth = 100;

    // Trackers
    protected int CurrentHealth;
    protected Animator Animator;
    protected Rigidbody2D Rigidbody;
    protected bool IsDashing;

    // animator values beforehand to save time later
    protected static readonly int AnimState = Animator.StringToHash("AnimState");
    protected static readonly int Idle = Animator.StringToHash("Idle");
    protected static readonly int Jump = Animator.StringToHash("Jump");
    protected static readonly int Hurt = Animator.StringToHash("Hurt");
    protected static readonly int Death = Animator.StringToHash("Death");
    protected static readonly int Grounded = Animator.StringToHash("Grounded");
    protected static readonly int Attack = Animator.StringToHash("Attack");
    protected static readonly int Parry = Animator.StringToHash("Parry");
    protected static readonly int Dash = Animator.StringToHash("Recover");


    public void TakeDamage(int damage) {
        CurrentHealth -= damage;
        // damage animation
        Animator.SetTrigger(Hurt);
        
        if (CurrentHealth <= 0) {
            Die();
        }
    }
    
    // boosts the game object in a certain cardinal direction 
    protected void VelocityDash(int cardinalDirection, float dashSpeed, float dashTime) {
        IsDashing = true;
        StartCoroutine(DashCoroutine(dashTime));
        // TODO: do something about this monstrosity
        switch (cardinalDirection) {
            case 0:
                Rigidbody.velocity = new Vector2(0, dashSpeed);
                break;
            case 1:
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
                break;
            case 2:
                Rigidbody.velocity = new Vector2(0, -dashSpeed);
                break;
            case 3:
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x - dashSpeed, Rigidbody.velocity.y);
                break;
            default:
                Debug.Log("invalid dash direction");
                break;
        }
    }

    // dash coroutine handles stopping the dash
    private IEnumerator DashCoroutine(float dashTime) {
        yield return new WaitForSeconds(dashTime);
        Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        IsDashing = false;
    }

    // pauses the animator for pauseTime
    protected IEnumerator PauseAnimatorCoroutine(float pauseTime) {
        float temp = Animator.speed;
        Animator.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        // ReSharper disable once Unity.InefficientPropertyAccess
        Animator.speed = temp;
    }
    
    // knock this object away from point with velocity vel 
    protected void KnockAwayFromPoint(float vel, Vector3 point) {
        Rigidbody.velocity = vel * (transform.position - point).normalized;
    }
    
    

    protected virtual void Die() {
        Animator.SetTrigger(Death);
        //transform.GetComponent<Collider>().isTrigger = true;
        Rigidbody.gravityScale = 0;
    }

    protected void Stun(float stunTime) {
        
    }




}
