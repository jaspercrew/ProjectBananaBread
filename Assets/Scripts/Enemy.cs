using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : LivingThing
{
    //Configurable values for enemy
    private float speed = 3f;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float knockbackVal = 2f;
    [SerializeField] private float attackRange = .25f;
    private float attackRate = .75f;
    private int attackDamage = 10;
    [SerializeField] private LayerMask playerLayers;
    [SerializeField] private int moveState = 0; //determines movement behavior
    
    //Trackers
    private float moveVector = 0f;
    private float nextAttackTime = 0f;
    private IEnumerator attackCo;
    
    
    
    // Start is called before the first frame update
    void Start() {
        currentHealth = maxHealth;
        animator_ = transform.GetComponent<Animator>();
        _rigidbody = transform.GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage, float knockback) { //assumes damage is taken from PLAYER
        Interrupt();
        GameObject player = GameObject.FindWithTag("Player");
        KnockAwayFromPoint(knockback, player.transform.position);
        currentHealth -= damage;
        //damage animation
        animator_.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }

    public void Interrupt() { //should stop all relevant coroutines
        StopCoroutine(attackCo); //interrupt attack if take damage
        _rigidbody.velocity = Vector2.zero;
    }
    

    private void Attack() {
        animator_.SetTrigger("Attack");
        StartCoroutine(attackCo = AttackCoroutine());
    }

    private IEnumerator AttackCoroutine() {
        //enemy attack modifiers
        float attackBoost = 1.5f;
        float beginAttackDelay = .55f;
        float hitConfirmDelay = .20f;

        yield return new WaitForSeconds(beginAttackDelay);

        //move while attacking
        // if (moveVector > .5) {
        //     VelocityDash(1, attackBoost);
        // }
        // else if (moveVector < -.5) {
        //     VelocityDash(3, attackBoost);
        // }
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        if (hits.Length > 0) {
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); //pause swing animation if an enemy is hit
        }
        foreach (Collider2D p in hits) {
            CharController player = p.GetComponent<CharController>();
            if (player.isParrying) {
                //StartCoroutine(PauseAnimatorCoroutine(.2f));
                player.CounterStrike(GetComponent<Enemy>());
                break;
            }
            player.TakeDamage(attackDamage, knockbackVal, transform.position);
        }
    }
    
    private bool isMovementEnabled() {
        if (animator_.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            return false;
        }
        return true;
    }
    
    // Update is called once per frame
    void Update()
    {
        //scan for player to attack
        Collider2D[] scans = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);
        if (scans.Length > 0 && Time.time >= nextAttackTime) {
            Attack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
    }
    
    private void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
