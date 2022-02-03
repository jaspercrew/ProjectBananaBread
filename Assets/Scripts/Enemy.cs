using System;
using System.Collections;
using Pathfinding;
using UnityEngine;

public class Enemy : LivingThing
{
    // Configurable values for enemy
    // private float speed = 3f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float knockbackVal = 2f;
    [SerializeField] private float attackRange = .25f;
    private const float AttackRate = .75f;
    private const int AttackDamage = 10;
    [SerializeField] private LayerMask playerLayers;
    // [SerializeField] private int moveState; // determines movement behavior
    
    // Trackers
    // private float moveVector = 0f;
    private float nextAttackTime;
    private IEnumerator attackCo;
    private EnvironmentState originalState = 0;
    private bool canFunction;

    private AIPath aiPath;
    private CharController charController;

    // Start is called before the first frame update
    private void Start() {
        canFunction = true;
        CurrentHealth = MaxHealth;
        aiPath = GetComponent<AIPath>();
        Animator = transform.GetComponentInChildren<Animator>();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        charController = FindObjectOfType<CharController>();
        if (originalState == 0) ;
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

    public virtual void Interrupt() { // should stop all relevant coroutines
        if (attackCo != null) {
            StopCoroutine(attackCo); // interrupt attack if take damage
            Rigidbody.velocity = Vector2.zero;
        }
    }
    

    private void DoAttack() {
        Animator.SetTrigger(Attack);
        StartCoroutine(attackCo = AttackCoroutine());
    }

    protected override void Die() {
        const float deathTime = 1f;
        DisableFunctionality();
        Animator.SetTrigger(Death);
        Destroy(gameObject, deathTime);
    }

    private IEnumerator  AttackCoroutine() {
        // enemy attack modifiers
        // float attackBoost = 1.5f;
        const float beginAttackDelay = .55f;
        const float hitConfirmDelay = .20f;

        yield return new WaitForSeconds(beginAttackDelay);

        // move while attacking
        // if (moveVector > .5) {
        //     VelocityDash(1, attackBoost);
        // }
        // else if (moveVector < -.5) {
        //     VelocityDash(3, attackBoost);
        // }

        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
            hitColliders, playerLayers);
        if (numHits > 0) {
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); // pause swing animation if an enemy is hit
        }

        if (hitColliders.Length > 0) {
            foreach (Collider2D p in hitColliders) {
                if (p == null || p.gameObject.GetComponent<CharController>() == null) {
                    break;
                }
                if (charController.isParrying) {
                    // StartCoroutine(PauseAnimatorCoroutine(.2f));
                    charController.CounterStrike(GetComponent<Enemy>());
                    break;
                }
                charController.TakeDamage(AttackDamage, knockbackVal, transform.position);
            }
        }
    }

    // private bool IsMovementEnabled() {
    //     if (animator_.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
    //         return false;
    //     }
    //     return true;
    // }
    
    // Update is called once per frame
    private void Update()
    {
        if (canFunction) {
            // scan for player to attack
            ScanForAttack_Update();
            //movement visuals
            TurnAround_Update();
        }
    }

    private void TurnAround_Update() {
        if (aiPath.desiredVelocity.x > 0) {
            FaceRight();
        }
        else if (aiPath.desiredVelocity.x < 0) {
            FaceLeft();
        }
        Animator.SetInteger(AnimState, Mathf.Abs(aiPath.velocity.x) > .1 ? 2 : 0);
    }

    private void ScanForAttack_Update() {
        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
            hitColliders, playerLayers);
        if (numHits > 0 && Time.time >= nextAttackTime) {
            DoAttack();
            nextAttackTime = Time.time + 1f / AttackRate;
        }
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
