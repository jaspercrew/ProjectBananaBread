using System;
using System.Collections;
using Pathfinding;
using UnityEngine;
using UnityEngine.Serialization;

public class Enemy : LivingThing , IHittableEntity
{
    public float knocktime = .2f;
    public float speed = 3f;
    public float aggroRange = 10f;
    public float attackRange;
    public float attackCD;
    public float knockbackMult = 4f;
    public bool LOS_Aggro;
    public bool LOS_Attack;
    public Transform attackPoint;
    protected float lastAttackTime;

    //protected bool animationLocked = false;
    protected bool isAlive;
    protected bool isStunned;
    protected bool movementDisabledAirborne;
    protected bool movementDisabledTimed;
    protected bool playerInAggroRange;
    protected bool playerInAttackRange;
    protected bool beingKnocked;

    protected AIPath aiPath;
    protected CharController charController;
    protected LayerMask playerMask;

    // Start is called before the first frame update
    protected virtual void Start()
    {
        print("start");
        playerInAggroRange = false;
        isAlive = true;
        isStunned = false;
        CurrentHealth = MaxHealth;
        aiPath = GetComponent<AIPath>();
        Animator = transform.GetComponentInChildren<Animator>();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        charController = FindObjectOfType<CharController>();
        playerMask = LayerMask.GetMask("Player");
    }

    public void GetHit(int damage)
    {
        TakeDamage(damage);
        //print("get hit - enemy");
    }

    public void TakeDamage(int damage) { // assumes damage is taken from PLAYER
        //print(isAlive);
        if (isAlive)
        {
            Stun(knocktime);

            StartCoroutine(KnockbackCoroutine(CharController.Instance.transform.position.x > transform.position.x
                ? Vector2.left
                : Vector2.right));
            //StartCoroutine(DisableMoveCoroutine(.2f));
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

    protected IEnumerator KnockbackCoroutine(Vector2 dir)
    {
        beingKnocked = true;
        //Rigidbody.velocity = dir * knockbackMult;
        Rigidbody.AddForce(dir * knockbackMult, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knocktime);
        beingKnocked = false;
        Rigidbody.velocity = Vector2.zero;
    }
    
    protected virtual bool AbleToMove()
    {
        return isAlive && !movementDisabledAirborne && !movementDisabledTimed && !isStunned;
    }

    protected void PlayerScan_Update()
    {
        //print(isAlive);
        const int maxHits = 20;
        Collider2D[] hitCollidersAggro = new Collider2D[maxHits];
        Collider2D[] hitCollidersAttack = new Collider2D[maxHits];
        int numHitsAggro = Physics2D.OverlapCircleNonAlloc(transform.position, aggroRange,
            hitCollidersAggro, playerMask);
        
        Transform attackScanPoint = attackPoint == null ? transform : attackPoint;
        int numHitsAttack = Physics2D.OverlapCircleNonAlloc(attackScanPoint.position, attackRange,
            hitCollidersAttack, playerMask);

        playerInAttackRange = numHitsAttack > 0;
        playerInAggroRange = numHitsAggro > 0;
    }

    protected void AttackLoop_Update()
    {
        if (AttackConditions())
        {
            lastAttackTime = Time.time;
            DoAttack();
        }
    }

    protected virtual bool AttackConditions()
    {
        if (LOS_Attack)
        {
            if (!Utils.LOSCheck(transform, charController.transform))
            {
                return false;
            }
        }
        
        return Time.time > lastAttackTime + attackCD;
    }

    protected virtual void DoAttack()
    {
        
    }

    protected virtual void Pathfind_Update()
    {
        if (beingKnocked)
        {
            return;
        }
        if (!AbleToMove())
        {
            Rigidbody.velocity = Vector2.zero;
        }
        //Debug.Log(movementDisabledAirborne);
        if (LOS_Aggro)
        {
            if (!Utils.LOSCheck(transform, charController.transform))
            {
                return;
            }
        }
        if (AbleToMove() && playerInAggroRange)
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
        print("enemy death");
        const float deathTime = 1f;
        isAlive = false;
        Animator.SetTrigger(Death);
        Destroy(gameObject, deathTime);
    }

    // protected IEnumerator AnimationLockCoroutine(float time)
    // {
    //     animationLocked = true;
    //     yield return new WaitForSeconds(time);
    //     animationLocked = false;
    //
    // }
    
    protected virtual void TurnAround_Update() {
        if (beingKnocked)
        {
            return;
        }
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
        StartCoroutine(StunCoroutine(stunTime));
    }

    public IEnumerator StunCoroutine(float stunTime) {
        BeginStun();
        yield return new WaitForSeconds(stunTime);
        EndStun();
    }

    protected virtual void BeginStun() {
        //StopAllCoroutines();
        isStunned = true;
    }

    protected virtual void EndStun()
    {
        isStunned = false;
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

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Platform>() != null)
        {
            movementDisabledAirborne = false;
        }
    }
}
