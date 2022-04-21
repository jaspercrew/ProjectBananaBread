using System;
using System.Collections;
using Pathfinding;
using UnityEngine;
public class Enemy : LivingThing
{
    [Tooltip("Speed is only for moving enemies")]
    public float speed = 3f;
    [Tooltip("Aggrodist is only for moving enemies")]
    public float aggroDist;
    public float attackRange;

    public float attackCD;
    protected float lastAttackTime;
    
    protected bool canFunction;
    protected bool movementDisabledAirborne;
    protected bool movementDisabledTimed;
    protected bool playerInAttackRange;
    
    protected AIPath aiPath;
    protected CharController charController;
    protected LayerMask playerMask;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        playerInAttackRange = false;
        canFunction = true;
        CurrentHealth = MaxHealth;
        aiPath = GetComponent<AIPath>();
        Animator = transform.GetComponentInChildren<Animator>();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        charController = FindObjectOfType<CharController>();
        playerMask = LayerMask.GetMask("Player");
    }

    public void TakeDamage(int damage) { // assumes damage is taken from PLAYER
        if (canFunction) {
            Stun(.2f);
            GameObject player = GameObject.FindWithTag("Player");
            //KnockAwayFromPoint(knockback, player.transform.position);
            StartCoroutine(DisableMoveCoroutine(.2f));
            CurrentHealth -= damage;
            
            // damage animation
            Animator.SetTrigger(Hurt);
            ParticleSystem gorePS = transform.Find("Particles").Find("GorePS").GetComponent<ParticleSystem>();
            if (gorePS != null)
            {
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
            }

            if (CurrentHealth <= 0) {
                Die();
            }
        }
    }
    
    protected virtual bool AbleToMove()
    {
        return canFunction && !movementDisabledAirborne && !movementDisabledTimed;
    }

    protected void PlayerScan_Update()
    {
        float dist = Vector2.Distance(charController.transform.position, transform.position);
        if (dist < attackRange)
        {
            playerInAttackRange = true;
        }
        else
        {
            playerInAttackRange = false;
        }
    }

    protected void AttackLoop_Update()
    {
        if (playerInAttackRange && AttackConditions())
        {
            lastAttackTime = Time.time;
            DoAttack();
        }
    }

    protected virtual bool AttackConditions()
    {
        return Time.time > lastAttackTime + attackCD;
    }

    protected virtual void DoAttack()
    {
        
    }

    protected virtual void Pathfind_Update()
    {
        //Debug.Log(movementDisabledAirborne);
        float dist = Vector2.Distance(charController.transform.position, transform.position);
        if (AbleToMove() && !playerInAttackRange && dist < aggroDist)
        {
            float thisXPos = transform.position.x;
            float charXPos = charController.transform.position.x;
            const float buffer = .5f;
            float xLeft = charXPos - buffer;
            float xRight = charXPos + buffer;
            
            float targetX = Mathf.Abs(thisXPos - xLeft) < Mathf.Abs(thisXPos - xRight) ? xLeft : xRight; //??
            
            if (targetX > transform.position.x)
            {
                Rigidbody.velocity = new Vector2(speed, Rigidbody.velocity.y);
            }
            else if (targetX < transform.position.x)
            {
                Rigidbody.velocity = new Vector2(-speed, Rigidbody.velocity.y);
            }
        }
    }

    public virtual void Interrupt() { // should stop all relevant coroutines
    }
    

    protected void Die() {
        const float deathTime = 1f;
        DisableFunctionality();
        Animator.SetTrigger(Death);
        Destroy(gameObject, deathTime);
    }
    
    protected virtual void TurnAround_Update() {
        if (Rigidbody.velocity.x > 0) {
            FaceRight();
        }
        else if (Rigidbody.velocity.x < 0) {
            FaceLeft();
        }
        Animator.SetInteger(AnimState, Mathf.Abs(Rigidbody.velocity.x) > .1 ? 2 : 0);
    }


    public void Stun(float stunTime) {
        //Debug.Log("stun");
        Interrupt();
        DisableFunctionality();
        StartCoroutine(StunCoroutine(stunTime));
    }

    public override IEnumerator StunCoroutine(float stunTime) {
        yield return new WaitForSeconds(stunTime);
        EnableFunctionality();
    }

    protected virtual void DisableFunctionality() {
        StopAllCoroutines();
        canFunction = false;
    }

    protected virtual void EnableFunctionality() {
        canFunction = true;
    }

    public override void Yoink(float yoinkForce)
    {
        Vector3 dir = (Camera.main).ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Rigidbody.AddForce(yoinkForce * dir.normalized , ForceMode2D.Impulse);
        movementDisabledAirborne = true;
    }
    


    protected IEnumerator DisableMoveCoroutine(float time)
    {
        movementDisabledTimed = true;
        yield return new WaitForSeconds(time);
        movementDisabledTimed = false;

    }

    protected void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Platform>() != null)
        {
            movementDisabledAirborne = false;
        }
    }
}
