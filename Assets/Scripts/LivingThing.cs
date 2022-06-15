using System.Collections;
using UnityEngine;

public abstract class LivingThing : BinaryEntity {
    // Configurable values 
    public int maxHealth;
    //protected bool isStunned;

    // Trackers
    public int currentHealth;
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
    protected static readonly int AttackA = Animator.StringToHash("AttackA");
    protected static readonly int AttackB = Animator.StringToHash("AttackB");
    protected static readonly int AttackC = Animator.StringToHash("AttackC");
    protected static readonly int Parry = Animator.StringToHash("Parry");
    protected static readonly int Dash = Animator.StringToHash("Dash");
    
    protected void FaceLeft()
    {
        //Debug.Log("face left");
        Transform t = transform; // more efficient, according to Rider
        Vector3 s = t.localScale;
        t.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
    }

    protected void FaceRight()
    {
        //Debug.Log("face right");
        Transform t = transform; // more efficient, according to Rider
        Vector3 s = t.localScale;
        t.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
    }
    
    // boosts the game object in a certain cardinal direction 
    protected void VelocityDash(float dashSpeed, float dashTime) {
        IsDashing = true;
        StartCoroutine(DashCoroutine(dashTime, dashSpeed));
        // monstrosity removed
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
    }

    // dash coroutine handles stopping the dash
    private IEnumerator DashCoroutine(float dashTime, float dashSpeed) {
        //Debug.Log(savedVel);
        yield return new WaitForSeconds(dashTime);
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x - dashSpeed, Rigidbody.velocity.y);
        IsDashing = false;
    }

    // pauses the animator for pauseTime
    // protected IEnumerator PauseAnimatorCoroutine(float pauseTime) {
    //     float temp = Animator.speed;
    //     Animator.speed = 0;
    //     yield return new WaitForSeconds(pauseTime);
    //     // ReSharper disable once Unity.InefficientPropertyAccess
    //     Animator.speed = temp;
    // }
    




}
