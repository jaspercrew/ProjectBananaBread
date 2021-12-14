using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : LivingThing
{
    // Start is called before the first frame update
    private float speed = 3f;
    private float moveVector = 0f;

    [SerializeField] private Transform attackPoint;
    private float attackRate = .75f;
    private float nextAttackTime = 0f;
    [SerializeField] private float knockbackVal = 2f;
    [SerializeField] private float attackRange = .25f;
    private int attackDamage = 10;
    
    [SerializeField] private LayerMask playerLayers;

    [SerializeField] private int moveState = 0;
    
    
    void Start() {
        currentHealth = maxHealth;
        _animator = transform.GetComponent<Animator>();
        _rigidbody = transform.GetComponent<Rigidbody2D>();
    }

    public void TakeDamage(int damage, float knockback) {
        GameObject player = GameObject.FindWithTag("Player");
        KnockAwayFromPoint(knockback, player.transform.position);
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }
    

    private void Attack() {
        //_rigidbody.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * 5f, 0), ForceMode2D.Impulse);
        //_rigidbody.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * 3, 0);
        _animator.SetTrigger("Attack");
        StartCoroutine(AttackCoroutine());
    }

    private IEnumerator AttackCoroutine() {
        //light attack modifiers
        float attackBoost = 1.5f;
        float beginAttackDelay = .55f;
        float hitConfirmDelay = .20f;

        yield return new WaitForSeconds(beginAttackDelay);

        if (moveVector > .5) {
            VelocityDash(1, attackBoost);
        }
        else if (moveVector < -.5) {
            VelocityDash(3, attackBoost);
        }
        
        Collider2D[] hits = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);

        if (hits.Length > 0) {
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); //pause swing animation if an enemy is hit
            Debug.Log("enemy hit something");
        }

        foreach (Collider2D enemy in hits) {
            //Debug.Log("hit");
            enemy.GetComponent<CharController>().TakeDamage(attackDamage, knockbackVal, transform.position);
        }
    }
    
    private bool isMovementEnabled() {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            return false;
        }
        return true;
    }
    
    // Update is called once per frame
    void Update()
    {
        Collider2D[] scans = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, playerLayers);

        if (scans.Length > 0 && Time.time >= nextAttackTime) {
            //continue combo
            Attack();
            nextAttackTime = Time.time + 1f / attackRate;
        }
        //transform.position += new Vector3(moveVector * speed * Time.deltaTime, 0, 0);
    }
    
    private void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }
}
