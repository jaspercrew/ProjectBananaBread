using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CharController: LivingThing {
    //Components
    private BoxCollider2D _collider;
    private ParticleSystem _dust;
    private ScreenShakeController _screenShakeController;
    private TargetGrappleController _targetGrappleController;
    
    //Configurable player control values
    private float speed = 3.5f;
    private float jumpForce = 6.3f;
    private float attackRate = 2f;
    private float parryRate = 1f;
    private float rollRate = 1f;
    private int attackDamage = 10;
    private float comboResetThreshold = 1f;
    public LayerMask enemyLayers;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    
    //Trackers
    private bool isInvincible = false;
    private float nextParryTime = 0f;
    public bool isParrying = false;
    private float nextAttackTime = 0f;
    private float nextRollTime = 0f;
    private float lastAttackTime = 0f;
    private int comboCounter = 0;
    private bool isAttacking = false;
    private float moveVector;
    private float xDir = 2;
    private readonly HashSet<Collider2D> _colliding = new HashSet<Collider2D>();


    // Start is called before the first frame update
    void Start() {
        currentHealth = maxHealth;
        _targetGrappleController = transform.GetComponent<TargetGrappleController>();
        _rigidbody = transform.GetComponent<Rigidbody2D>();
        _animator = transform.GetComponent<Animator>();
        _collider = transform.GetComponent<BoxCollider2D>();
        _dust = transform.GetComponentInChildren<ParticleSystem>();
        _screenShakeController = FindObjectOfType<Camera>().GetComponent<ScreenShakeController>();
    }

    // Update is called once per frame
    void FixedUpdate() {
        moveVector = Input.GetAxisRaw("Horizontal");
        //Debug.Log(moveVector);

        if (AbleToMove()) {
            //movement animations
            if (moveVector > Mathf.Epsilon || moveVector < -Mathf.Epsilon) {
                _animator.SetInteger("AnimState", 2);
            }
            else {
                _animator.SetInteger("AnimState", 0);
            }
            
            //actual moving
            transform.position += new Vector3(moveVector * speed * Time.deltaTime, 0, 0);
            
            //feet dust logic
            if (Math.Abs(xDir - moveVector) > 0.01f && IsGrounded() && moveVector != 0) {
                _dust.Play();
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

    private bool AbleToAct() {
        if (isDashing || isAttacking || isParrying) {
            return false;
        }
        return true;
    }

    void Update() {
        if (Time.time >= nextAttackTime) {
            AttemptAttack();
        }

        if (IsGrounded()) {
            _animator.SetBool("Jump", false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && AbleToAct() && Time.time >= nextRollTime) {
            Roll();
            nextRollTime = Time.time + 1f / rollRate;
        }

        if (Input.GetKeyDown(KeyCode.F) && AbleToAct()) {
            //switch states
            //GameObject.FindObjectOfType<BinaryPlatform>().SwitchState(EnvironmentState.Cyberpunk);
            GameManager.Instance.SwitchWorldState();
            //Debug.Log(GameManager.Instance);
        }

        if (Input.GetKeyDown(KeyCode.E) && AbleToAct()) {
            if (_targetGrappleController.isGrappling) {
                _targetGrappleController.EndGrapple();
                return;
            }
            _targetGrappleController.StartGrapple();
        }
        
        if (Input.GetMouseButtonDown(1) && AbleToAct() && Time.time >= nextParryTime) {
            Parry();
            nextParryTime = Time.time + 1f / parryRate;
        }
        
        if (Input.GetButtonDown("Jump") && IsGrounded() && AbleToMove()) {
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

    private void Parry() {
        //start parry animation
        transform.GetComponent<SpriteRenderer>().flipY = true;
        isParrying = true;
        StartCoroutine(ParryCoroutine());
    }

    private IEnumerator ParryCoroutine() {
        float parryTime = .5f;
        yield return new WaitForSeconds(parryTime);
        isParrying = false;
        transform.GetComponent<SpriteRenderer>().flipY = false;
    }

    public void Counterstrike(Enemy enemy) {
        isAttacking = true;
        //start counter animation
        StartCoroutine(CounterCoroutine(enemy));

    }

    protected IEnumerator CounterCoroutine(Enemy enemy) {
        float counterTime = .2f;
        yield return new WaitForSeconds(counterTime);
        _screenShakeController.MediumShake();
        enemy.TakeDamage(20, 2);
        isAttacking = false;
    }



    private void Roll() {
        float rollSpeed = 9f;
        float rollTime = .23f;
        if (moveVector > .5) {
            VelocityDash(1, rollSpeed, rollTime);
            _dust.Play();
        }
        else if (moveVector < -.5) {
            VelocityDash(3, rollSpeed, rollTime);
            _dust.Play();
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

    private bool AbleToMove() {
        if (isAttacking) {
            return false;
        }
        if (isDashing) {
            return false;
        }
        return true;
    }

    protected bool AbleToBeDamaged() {
        return !isInvincible && !isDashing;
    }
    
    //Take damage, knock away from point
    public void TakeDamage(int damage, float knockback, Vector2 point) {
        if (!AbleToBeDamaged()) {
            return;
        }
        StartCoroutine(TakeDamageCoroutine());
        KnockAwayFromPoint(knockback, point);
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }

    protected IEnumerator TakeDamageCoroutine() {
        _screenShakeController.MediumShake();
        isInvincible = true;
        float invFrames = .2f;
        yield return new WaitForSeconds(invFrames);
        isInvincible = false;
    }
    
    protected override void Die() {
        _animator.SetTrigger("Death");
        transform.GetComponent<Collider>().enabled = false;
        _rigidbody.gravityScale = 0;
    }

    private void Jump() {
        _dust.Play();
        _rigidbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
        _animator.SetTrigger("Jump");
        _animator.SetBool("Grounded", false);
    }

    //handles boolean checking and combo count for attacking 
    private void AttemptAttack() {
        if (Input.GetMouseButtonDown(0) && Time.time < lastAttackTime + comboResetThreshold && !isDashing) {
            //continue combo
            comboCounter++;
            Attack(comboCounter);
                
            if (comboCounter >= 4) { //heavy
                comboCounter = 0;
                nextAttackTime = Time.time + 2f / attackRate;
            }
            else { //light
                nextAttackTime = Time.time + 1f / attackRate;
            }
            lastAttackTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(0) && !isDashing){
            //start new attack chain 
            comboCounter = 1;
            Attack(comboCounter);
                
            nextAttackTime = Time.time + 1f / attackRate;
            lastAttackTime = Time.time;
        }
    }
    
    private void Attack(int comboCount) {
        //_screenShakeController.LightShake();
        isAttacking = true;
        _animator.speed = 1;
        Assert.IsTrue(comboCount <= 4);
        
        _animator.SetTrigger("Attack");
        if (comboCount == 4) { //heavy attack?
            _animator.speed = .5f;
        }
        StartCoroutine(AttackCoroutine(comboCount == 4));
        
    }

    private IEnumerator AttackCoroutine(bool heavyAttack) {
        //light attack modifiers
        float attackBoost = 2.5f;
        float beginAttackDelay = .15f;
        float endAttackDelay = .2f;
        float hitConfirmDelay = .20f;
        
        //heavy attack modifiers
        if (heavyAttack) { 
            attackBoost = 3.0f;
            beginAttackDelay = .25f;
            endAttackDelay = .4f;
            hitConfirmDelay = .30f;
        }
    
        yield return new WaitForSeconds(beginAttackDelay);
        
        
        //move while attacking
        if (IsGrounded()) {
            if (moveVector > .5) {
                VelocityDash(1, attackBoost, .5f);
            }
            else if (moveVector < -.5) {
                VelocityDash(3, attackBoost, .5f);
            }
        }
        
        //scan for hit enemies
        Collider2D[] hitEnemies = Physics2D.OverlapCircleAll(attackPoint.position, attackRange, enemyLayers);

        if (hitEnemies.Length > 0) {
            //pause swing animation if an enemy is hit
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); 
            _screenShakeController.MediumShake();
        }

        foreach (Collider2D enemy in hitEnemies) {
            enemy.GetComponent<Enemy>().TakeDamage(attackDamage, heavyAttack ? 2f : 1f);
        }
        yield return new WaitForSeconds(endAttackDelay);
        isAttacking = false;
    }
    
    //show gizmos in editor
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
        //Grounding Controller
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
