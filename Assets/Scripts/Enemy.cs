using System.Collections;
using Pathfinding;
using UnityEngine;

public class Enemy : LivingThing , IHittableEntity
{
    public float knocktime = .2f;
    public float speed = 3f;
    public float aggroRange = 10f;
    public float attackRange;
    public float attackCooldown;
    public float attackRecovery;
    public float knockbackMult = 3f;
    public float pushForce = 15f;
    public bool hasLosAggro;
    public bool hasLosAttack;
    public Transform attackPoint;
    protected float LastAttackTime;

    //protected bool animationLocked = false;
    protected bool inRecovery;
    protected bool IsAlive;
    protected bool IsStunned;
    protected bool MovementDisabledAirborne;
    protected bool MovementDisabledTimed;
    protected bool PlayerInAggroRange;
    protected bool PlayerInAttackRange;
    protected bool BeingKnocked;

    protected AIPath AIPath;
    protected CharController CharController;
    protected LayerMask PlayerMask;
    protected BoxCollider2D collider;

    // Start is called before the first frame update
    protected override void Start()
    {
        print("start");
        PlayerInAggroRange = false;
        IsAlive = true;
        IsStunned = false;
        currentHealth = maxHealth;
        AIPath = GetComponent<AIPath>();
        Animator = transform.GetComponentInChildren<Animator>();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        CharController = FindObjectOfType<CharController>();
        PlayerMask = LayerMask.GetMask("Player");
        collider = GetComponent<BoxCollider2D>();
    }

    public virtual void GetHit(int damage)
    {
        TakeDamage(damage);
        //print("get hit - enemy");
    }

    public void TakeDamage(int damage) { // assumes damage is taken from PLAYER
        //print(isAlive);
        if (IsAlive)
        {
            Stun(knocktime);

            StartCoroutine(KnockbackCoroutine(CharController.position.x > transform.position.x
                ? Vector2.left
                : Vector2.right));
            //StartCoroutine(DisableMoveCoroutine(.2f));
            currentHealth -= damage;
            
            // damage animation
            Animator.SetTrigger(Hurt);

            ParticleSystem gorePS = transform.Find("Particles").Find("GorePS").GetComponent<ParticleSystem>();
            if (gorePS != null)
            {
                ParticleSystem.ShapeModule shape = gorePS.shape;
                if (transform.position.x < CharController.transform.position.x)
                {
                    shape.rotation = new Vector3(0, 0, 145);
                }
                else
                {
                    shape.rotation = new Vector3(0, 0, 0);
                }

                gorePS.Play();
            }

            if (currentHealth <= 0) {
                Die();
            }
        }
    }

    protected IEnumerator KnockbackCoroutine(Vector2 dir)
    {
        BeingKnocked = true;
        //Rigidbody.velocity = dir * knockbackMult;
        Rigidbody.AddForce(dir * knockbackMult, ForceMode2D.Impulse);
        yield return new WaitForSeconds(knocktime);
        BeingKnocked = false;
        Rigidbody.velocity = Vector2.zero;
    }
    
    protected virtual bool IsFrozen() //Set velocity to ZERO
    {
        return !IsAlive || MovementDisabledAirborne || MovementDisabledTimed || IsStunned;
    }

    protected virtual bool CanMove() //can begin new movements
    {
        return !BeingKnocked && !IsFrozen() && !inRecovery;
    }

    protected void PlayerScan_Update()
    {
        //print(isAlive);
        const int maxHits = 20;
        Collider2D[] hitCollidersAggro = new Collider2D[maxHits];
        Collider2D[] hitCollidersAttack = new Collider2D[maxHits];
        int numHitsAggro = Physics2D.OverlapCircleNonAlloc(transform.position, aggroRange,
            hitCollidersAggro, PlayerMask);
        
        Transform attackScanPoint = attackPoint == null ? transform : attackPoint;
        int numHitsAttack = Physics2D.OverlapCircleNonAlloc(attackScanPoint.position, attackRange,
            hitCollidersAttack, PlayerMask);

        PlayerInAttackRange = numHitsAttack > 0;
        PlayerInAggroRange = numHitsAggro > 0;
    }

    protected void AttackLoop_Update()
    {
        if (AttackConditions())
        {
            LastAttackTime = Time.time;
            DoAttack();
        }
    }

    protected virtual bool AttackConditions()
    {
        if (hasLosAttack)
        {
            if (!Utils.LosCheck(transform, CharController.transform))
            {
                return false;
            }
        }
        
        return Time.time > LastAttackTime + attackCooldown;
    }

    protected virtual void DoAttack()
    {
        StartCoroutine(RecoveryCoroutine());
    }

    protected IEnumerator RecoveryCoroutine()
    {
        inRecovery = true;
        yield return new WaitForSeconds(attackRecovery);
        inRecovery = false;
    }

    protected virtual void Pathfind_Update()
    {
        if (!CanMove())
        {
            //print("a");
            return;
        }
        if (IsFrozen())
        {
            //print("b");
            Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
            return;
        }
        //Debug.Log(movementDisabledAirborne);
        if (hasLosAggro)
        {
            if (!Utils.LosCheck(transform, CharController.transform))
            {
                return;
            }
        }
        if (PlayerInAggroRange)
        {
            //print("c");
            float thisXPos = transform.position.x;
            float charXPos = CharController.transform.position.x;
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
        IsAlive = false;
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
        if (BeingKnocked)
        {
            return;
        }
        Animator.SetInteger(AnimState, Mathf.Abs(Rigidbody.velocity.x) > .01 ? 2 : 0);
        if (Rigidbody.velocity.x > 0) {
            FaceRight();
        }
        else if (Rigidbody.velocity.x < 0) {
            FaceLeft();
        }
    }

    protected void PlayerPush_Update()
    {
        if (collider.bounds.Contains(CharController.position))
        {
            int dir = 1;
            if (CharController.position.x < transform.position.x)
            {
                dir = -1;
            }
            CharController.Instance.GetComponent<Rigidbody2D>().AddForce(Vector2.right * dir * pushForce);
        }
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
        IsStunned = true;
    }

    protected virtual void EndStun()
    {
        IsStunned = false;
    }

    public override void Yoink(float yoinkForce)
    {
        Vector3 dir = (Camera.main).ScreenToWorldPoint(Input.mousePosition) - transform.position;
        Rigidbody.AddForce(yoinkForce * dir.normalized , ForceMode2D.Impulse);
        MovementDisabledAirborne = true;
    }
    


    protected IEnumerator DisableMoveCoroutine(float time)
    {
        MovementDisabledTimed = true;
        yield return new WaitForSeconds(time);
        MovementDisabledTimed = false;

    }

    protected virtual void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.GetComponent<Platform>() != null)
        {
            MovementDisabledAirborne = false;
        }
    }
}
