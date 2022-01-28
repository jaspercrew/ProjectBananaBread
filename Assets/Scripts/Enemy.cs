using System.Collections;
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
    private bool isAlive = true;

    // Start is called before the first frame update
    private void Start() {
        CurrentHealth = MaxHealth;
        Animator = transform.GetComponentInChildren<Animator>();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        if (originalState == 0)
            originalState = EntityState;
    }

    public void TakeDamage(int damage, float knockback) { // assumes damage is taken from PLAYER
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

    private void Interrupt() { // should stop all relevant coroutines
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
        isAlive = false;
        Animator.SetTrigger(Death);
        Debug.Log("deatfh");
        StartCoroutine(DieCoroutine());
        
    }

    protected virtual IEnumerator DieCoroutine() {
        const float deathTime = 2f;
        yield return new WaitForSeconds(deathTime);
        Debug.Log("2");
        Destroy(gameObject);
    }

    private IEnumerator AttackCoroutine() {
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
        foreach (Collider2D p in hitColliders) {
            CharController player = p.GetComponent<CharController>();
            if (player.isParrying) {
                // StartCoroutine(PauseAnimatorCoroutine(.2f));
                player.CounterStrike(GetComponent<Enemy>());
                break;
            }
            player.TakeDamage(AttackDamage, knockbackVal, transform.position);
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
        if (isAlive) {
            // scan for player to attack
            const int maxHits = 20;
            Collider2D[] hitColliders = new Collider2D[maxHits];
            int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
                hitColliders, playerLayers);
            if (numHits > 0 && Time.time >= nextAttackTime) {
                DoAttack();
                nextAttackTime = Time.time + 1f / AttackRate;
            }
        }
    }
    
    private void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
