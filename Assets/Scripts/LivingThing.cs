using System.Collections;
using UnityEngine;

public abstract class LivingThing : Entity {
    // Configurable values 
    protected const int MaxHealth = 100;
    protected bool isStunned;

    // Trackers
    public int CurrentHealth;
    protected Animator Animator;
    protected Rigidbody2D Rigidbody;
    protected bool isDashing;

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
    
    protected void FaceLeft()
    {
        Transform t = transform; // more efficient, according to Rider
        t.localScale = new Vector3(1, t.localScale.y, 0);
    }

    protected void FaceRight()
    {
        Transform t = transform; // more efficient, according to Rider
        t.localScale = new Vector3(-1, t.localScale.y, 0);
    }
    
    // boosts the game object in a certain cardinal direction 
    protected void VelocityDash(float dashSpeed, float dashTime) {
        isDashing = true;
        StartCoroutine(DashCoroutine(dashTime, dashSpeed));
        // monstrosity removed
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
    }

    // dash coroutine handles stopping the dash
    private IEnumerator DashCoroutine(float dashTime, float dashSpeed) {
        //Debug.Log(savedVel);
        yield return new WaitForSeconds(dashTime);
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x - dashSpeed, Rigidbody.velocity.y);
        isDashing = false;
    }

    // pauses the animator for pauseTime
    // protected IEnumerator PauseAnimatorCoroutine(float pauseTime) {
    //     float temp = Animator.speed;
    //     Animator.speed = 0;
    //     yield return new WaitForSeconds(pauseTime);
    //     // ReSharper disable once Unity.InefficientPropertyAccess
    //     Animator.speed = temp;
    // }
    
    public virtual IEnumerator StunCoroutine(float stunTime) {
        yield return new WaitForSeconds(stunTime);
        isStunned = false;
    }




}
