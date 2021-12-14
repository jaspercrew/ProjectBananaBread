using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CharController: LivingThing {
    private BoxCollider2D _collider;
    private ParticleSystem _dust;
    
    private float speed = 3.5f;
    private float jumpForce = 5f;
    private float attackRate = 2f;
    private float moveVector;
    private float xDir = 2;

    private float nextAttackTime = 0f;
    private float lastAttackTime = 0f;
    private float comboResetThreshold = 1f;
    private int comboCounter = 0;
    //private bool movementEnabled = true;
    
    [SerializeField]
    private Transform attackPoint;
    [SerializeField]
    private float attackRange;

    private int attackDamage = 10;
    public LayerMask enemyLayers;
    
    private readonly HashSet<Collider2D> _colliding = new HashSet<Collider2D>();
    
    // Start is called before the first frame update
    void Start() {
        currentHealth = maxHealth;
        _rigidbody = transform.GetComponent<Rigidbody2D>();
        _animator = transform.GetComponent<Animator>();
        _collider = transform.GetComponent<BoxCollider2D>();
        _dust = transform.GetComponentInChildren<ParticleSystem>();
        

    }

    // Update is called once per frame
    void FixedUpdate() {
        moveVector = Input.GetAxisRaw("Horizontal");

        if (isMovementEnabled()) {
            if (moveVector > Mathf.Epsilon || moveVector < -Mathf.Epsilon) {
                _animator.SetInteger("AnimState", 2);
            }
            else {
                _animator.SetInteger("AnimState", 0);
            }
            
            //actual moving
            transform.position += new Vector3(moveVector * speed * Time.deltaTime, 0, 0);
            
            if (Math.Abs(xDir - moveVector) > 0.01f && IsGrounded() && moveVector != 0) {
                _dust.Play();
                //Debug.Log("dust play");
            }
        
            xDir = moveVector;
            
            //direction switching
            if (moveVector > 0) {
                transform.localScale = new Vector2(-1, transform.localScale.y);
            }
            else if (moveVector < 0) {
                transform.localScale = new Vector2(1, transform.localScale.y);
            }
            
            //Vector3 moveVect = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
            //moveVect = moveVect.normalized * (speed * Time.deltaTime);
            //_rigidbody.MovePosition(transform.position + moveVect);
        }
        
    }

    void Update() {
        if (Time.time >= nextAttackTime){ //attack rate limiting
            attemptAttack();
        }
        
        if (IsGrounded()) {
            _animator.SetBool("Jump", false);
        }
        
        if (Input.GetButtonDown("Jump") && IsGrounded() && isMovementEnabled()) {
            Jump();                     
        }
        
        if (Input.GetButtonDown("Fire1")) { //crouch
            Crouch();
        }

        if (Input.GetButtonUp("Fire1")) { //uncrouch
            UnCrouch();
        }

        //shortjump
        if (Input.GetButtonUp("Jump") && !IsGrounded()) {
            _rigidbody.velocity = Vector2.Scale(_rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }

    private void attemptAttack() {
        if (Input.GetMouseButtonDown(0) && Time.time < lastAttackTime + comboResetThreshold) {
            //continue combo
            comboCounter++;
            Attack(comboCounter);
                
            if (comboCounter >= 4) { //heavy
                //Debug.Log("heavy attack");
                comboCounter = 0;
                nextAttackTime = Time.time + 2f / attackRate;
            }
            else { //light
                nextAttackTime = Time.time + 1f / attackRate;
            }
            lastAttackTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(0)){
            //start new attack chain 
            comboCounter = 1;
            Attack(comboCounter);
                
            nextAttackTime = Time.time + 1f / attackRate;
            lastAttackTime = Time.time;
        }
    }

    private void Crouch() {
        speed /= 2;
    }

    private void UnCrouch() {
        speed *= 2;
    }

    private bool IsGrounded() {
        return _colliding.Count > 0;
    }

    private bool isMovementEnabled() {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            return false;
        }
        return true;
    }
    
    public void TakeDamage(int damage, float knockback, Vector2 point) {
        //GameObject player = GameObject.FindWithTag("Player");
        KnockAwayFromPoint(knockback, point);
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }
    
    protected override void Die() {
        _animator.SetTrigger("Death");
        transform.GetComponent<Collider>().enabled = false;
        _rigidbody.gravityScale = 0;
    }

    private void Jump() {
        _dust.Play();
        _rigidbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        //Debug.Log("fds");
        _animator.SetTrigger("Jump");
        _animator.SetBool("Grounded", false);
    }

    private void Attack(int comboCount) {
        //_rigidbody.AddForce(new Vector2(Input.GetAxisRaw("Horizontal") * 5f, 0), ForceMode2D.Impulse);
        //_rigidbody.velocity = new Vector2(Input.GetAxisRaw("Horizontal") * 3, 0);
        _animator.speed = 1;
        Assert.IsTrue(comboCount <= 4);
        
        //Debug.Log("combo count: " + comboCount);
        _animator.SetTrigger("Attack");
        if (comboCount == 4) { //heavy attack?
            _animator.speed = .5f;
        }
        StartCoroutine(AttackCoroutine(comboCount == 4));
    }

    private IEnumerator AttackCoroutine(bool heavyAttack) {
        //light attack modifiers
        float attackBoost = 1.5f;
        float beginAttackDelay = .15f;
        float hitConfirmDelay = .20f;
        
        if (heavyAttack) { //heavy attack modifiers
            attackBoost = 2.5f;
            beginAttackDelay = .25f;
            hitConfirmDelay = .30f;
        }
    
        yield return new WaitForSeconds(beginAttackDelay);
        if (IsGrounded()) {
            if (moveVector > .5) {
                VelocityDash(1, attackBoost);
            }
            else if (moveVector < -.5) {
                VelocityDash(3, attackBoost);
            }
        }
            
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        if (hitEnemies.Length > 0) {
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); //pause swing animation if an enemy is hit
        }

        foreach (Collider2D enemy in hitEnemies) {
            //Debug.Log("hit");
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage, heavyAttack ? 2f : 1f);
        }
    }
    
    private void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }


    protected override void OnLanding() {
        _dust.Play();
        _animator.SetBool("Grounded", true);
        //Debug.Log("sus");
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        Collider2D col = other.collider;
        float colX = col.transform.position.x;
        float charX = transform.position.x;
        float colW = col.bounds.extents.x;
        float charW = _collider.bounds.extents.x;

        if (!col.isTrigger && Mathf.Abs(charX - colX) < Mathf.Abs(colW) + Mathf.Abs(charW) - 0.01f)
        {
            if (_colliding.Count == 0) {
                OnLanding();
            }
            _colliding.Add(col);

        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        _colliding.Remove(other.collider);
    }

}
