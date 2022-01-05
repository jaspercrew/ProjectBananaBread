using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class CharController: LivingThing {
    // Components
    private BoxCollider2D boxCollider;
    private ParticleSystem dust;
    private ScreenShakeController screenShakeController;
    private TargetGrappleController targetGrappleController;
    
    // Configurable player control values
    private float speed = 3.5f;
    private const float JumpForce = 6.3f;
    private const float AttackRate = 2f;
    private const float ParryRate = 1f;
    private const float RollRate = 1f;
    private const int AttackDamage = 10;
    private const float ComboResetThreshold = 1f;
    public LayerMask enemyLayers;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    
    // Trackers
    private bool isInvincible;
    private float nextParryTime;
    public bool isParrying;
    private float nextAttackTime;
    private float nextRollTime;
    private float lastAttackTime;
    private int comboCounter;
    private bool isAttacking;
    private float moveVector;
    private float xDir = 2;
    private readonly HashSet<Collider2D> colliding = new HashSet<Collider2D>();
    private IEnumerator attackCoroutine;


    // Start is called before the first frame update
    private void Start() {
        currentHealth = maxHealth;
        targetGrappleController = transform.GetComponent<TargetGrappleController>();
        _rigidbody = transform.GetComponent<Rigidbody2D>();
        animator_ = transform.GetComponent<Animator>();
        boxCollider = transform.GetComponent<BoxCollider2D>();
        dust = transform.GetComponentInChildren<ParticleSystem>();
        screenShakeController = FindObjectOfType<Camera>().GetComponent<ScreenShakeController>();
    }

    // Update is called once per frame
    private void FixedUpdate() {
        moveVector = Input.GetAxisRaw("Horizontal");
        // Debug.Log(moveVector);

        if (!IsAbleToMove()) return;
        
        // movement animations
        animator_.SetInteger(AnimState, Mathf.Abs(moveVector) > Mathf.Epsilon? 2 : 0);

        // actual moving
        transform.position += new Vector3(moveVector * speed * Time.deltaTime, 0, 0);
            
        // feet dust logic
        if (Math.Abs(xDir - moveVector) > 0.01f && IsGrounded() && moveVector != 0) {
            dust.Play();
        }
        xDir = moveVector;

        float yScale = transform.localScale.y;
        // direction switching
        if (moveVector > 0) {
            transform.localScale = new Vector3(-1, yScale, 0);
        }
        else if (moveVector < 0) {
            transform.localScale = new Vector3(1, yScale, 0);
        }
            
        // Vector3 moveVect = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
        // moveVect = moveVect.normalized * (speed * Time.deltaTime);
        // _rigidbody.MovePosition(transform.position + moveVect);

    }

    private bool IsAbleToAct()
    {
        return !isDashing && !isAttacking && !isParrying;
    }

    private void Update() {
        if (Time.time >= nextAttackTime && !isAttacking) {
            AttemptAttack();
        }

        if (IsGrounded()) {
            animator_.SetBool(Jump, false);
        }

        if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextRollTime) {
            Dash();
            nextRollTime = Time.time + 1f / RollRate;
        }

        if (Input.GetKeyDown(KeyCode.F) && IsAbleToAct()) {
            // switch states
            // GameObject.FindObjectOfType<BinaryPlatform>().SwitchState(EnvironmentState.Cyberpunk);
            // Debug.Log(GameManager.Instance);
            GameManager.Instance.SwitchWorldState();
        }

        if (Input.GetKeyDown(KeyCode.E) && IsAbleToAct()) {
            if (targetGrappleController.isGrappling) {
                targetGrappleController.EndGrapple();
                // return;
            }
            else
            {
                targetGrappleController.StartGrapple();
            }
        }
        
        if (Input.GetMouseButtonDown(1) && IsAbleToAct() && Time.time >= nextParryTime) {
            Parry();
            nextParryTime = Time.time + 1f / ParryRate;
        }
        
        if (Input.GetButtonDown("Jump") && IsGrounded() && IsAbleToMove()) {
            DoJump();                     
        }
        
        if (Input.GetButtonDown("Fire1")) { // crouch
            Crouch();
        }

        if (Input.GetButtonUp("Fire1")) { // uncrouch
            UnCrouch();
        }

        // short jump
        if (Input.GetButtonUp("Jump") && !IsGrounded()) {
            _rigidbody.velocity = Vector2.Scale(_rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }

    private void Parry() {
        // start parry animation
        transform.GetComponent<SpriteRenderer>().flipY = true;
        isParrying = true;
        StartCoroutine(ParryCoroutine());
    }

    private IEnumerator ParryCoroutine() {
        const float parryTime = .5f;
        yield return new WaitForSeconds(parryTime);
        isParrying = false;
        transform.GetComponent<SpriteRenderer>().flipY = false;
    }

    public void CounterStrike(Enemy enemy) {
        isAttacking = true;
        // start counter animation
        StartCoroutine(CounterCoroutine(enemy));
    }

    private IEnumerator CounterCoroutine(Enemy enemy) {
        const float counterTime = .2f;
        yield return new WaitForSeconds(counterTime);
        screenShakeController.MediumShake();
        enemy.TakeDamage(20, 2);
        isAttacking = false;
    }
    
    private void Dash()
    {
        Debug.Log("starting dash");
        if (!IsAbleToAct() && !isAttacking) {
            Debug.Log("stopping dash cuz bad");
            return;
        }
        
        if (isAttacking)
        {
            Debug.Log("interrupting attack with dash");
            animator_.SetTrigger(Idle); // TODO: dash animation
            isAttacking = false;
            Assert.IsNotNull(attackCoroutine);
            StopCoroutine(attackCoroutine);
            // animator_.SetInteger(AnimState, 0); // TODO
        }

        const float rollSpeed = 9f;
        const float rollTime = .23f;

        float xScale = transform.localScale.x;
        if (Mathf.Abs(xScale) > .5) {
            VelocityDash(xScale > 0? 3 : 1, rollSpeed, rollTime);
            dust.Play();
        }
    }

    // TODO: add variable isCrouching and set to true/false here instead of changing speed directly
    // and use isCrouching in movement and affect speed there
    private void Crouch() {
        speed /= 2;
    }

    private void UnCrouch() {
        speed *= 2;
    }

    private bool IsGrounded() {
        return colliding.Count > 0;
    }

    private bool IsAbleToMove() {
        return !isAttacking && !isDashing;
    }

    private bool AbleToBeDamaged() {
        return !isInvincible && !isDashing;
    }
    
    // Take damage, knock away from point
    public void TakeDamage(int damage, float knockback, Vector2 point) {
        if (!AbleToBeDamaged()) {
            return;
        }
        StartCoroutine(TakeDamageCoroutine());
        KnockAwayFromPoint(knockback, point);
        currentHealth -= damage;
        // damage animation
        animator_.SetTrigger(Hurt);
        
        if (currentHealth <= 0) {
            Die();
        }
    }

    private IEnumerator TakeDamageCoroutine() {
        screenShakeController.MediumShake();
        isInvincible = true;
        const float invFrames = .2f;
        yield return new WaitForSeconds(invFrames);
        isInvincible = false;
    }
    
    protected override void Die() {
        animator_.SetTrigger(Death);
        transform.GetComponent<Collider>().enabled = false;
        _rigidbody.gravityScale = 0;
    }

    private void DoJump() {
        dust.Play();
        _rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        animator_.SetTrigger(Jump);
        animator_.SetBool(Grounded, false);
    }
    
    // TODO: event queueing system?
    // eventTimeout = 0.5;
    // event queue = [(attack, t1), (dash, t2), ...]
    
    // update()
    // top = queue.peek()
    // if (top.time + eventTimeout >= Time.time)
    //      queue.pop()
    // do this again, until: queue is empty, or event is reached, then do as many events as possible
    // also, once this is done, fix nextAttackTime and nextDashTime or whatever it was

    // handles boolean checking and combo count for attacking 
    private void AttemptAttack()
    {
        if (!IsAbleToAct())
            return;
        if (Input.GetMouseButtonDown(0) && Time.time < lastAttackTime + ComboResetThreshold) {
            // continue combo
            comboCounter++;
            DoAttack(comboCounter);
                
            if (comboCounter >= 4) { // heavy
                comboCounter = 0;
                nextAttackTime = Time.time + 2f / AttackRate;
            }
            else { // light
                nextAttackTime = Time.time + 1f / AttackRate;
            }
            lastAttackTime = Time.time;
        }
        else if (Input.GetMouseButtonDown(0)){
            // start new attack chain 
            comboCounter = 1;
            DoAttack(comboCounter);
                
            nextAttackTime = Time.time + 1f / AttackRate;
            lastAttackTime = Time.time;
        }
    }
    
    private void DoAttack(int comboCount) {
        // _screenShakeController.LightShake();
        isAttacking = true;
        animator_.speed = 1;
        Assert.IsTrue(comboCount <= 4);
        
        animator_.SetTrigger(Attack);
        // TODO: remove when we have actual animations
        if (comboCount == 4) { // heavy attack?
            animator_.speed = .5f;
        }

        attackCoroutine = AttackCoroutine(comboCount == 4);
        StartCoroutine(attackCoroutine);
    }

    private IEnumerator AttackCoroutine(bool isHeavyAttack) {
        // light attack modifiers
        float attackBoost = 2.5f;
        float beginAttackDelay = .15f;
        float endAttackDelay = .2f;
        float hitConfirmDelay = .20f;
        
        // heavy attack modifiers
        if (isHeavyAttack) { 
            attackBoost = 3.0f;
            beginAttackDelay = .25f;
            endAttackDelay = .4f;
            hitConfirmDelay = .30f;
        }
    
        yield return new WaitForSeconds(beginAttackDelay);
        
        
        // move while attacking
        if (IsGrounded()) {
            if (moveVector > .5) {
                VelocityDash(1, attackBoost, .5f);
            }
            else if (moveVector < -.5) {
                VelocityDash(3, attackBoost, .5f);
            }
        }

        const int maxEnemiesHit = 20;
        Collider2D[] hitColliders = new Collider2D[maxEnemiesHit];
        
        // scan for hit enemies
        int numHitEnemies = Physics2D.OverlapCircleNonAlloc(
            attackPoint.position, attackRange, hitColliders, enemyLayers);

        if (numHitEnemies > 0) {
            // pause swing animation if an enemy is hit
            StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay)); 
            screenShakeController.MediumShake();
        }

        foreach (Collider2D enemy in hitColliders)
        {
            if (enemy is null)
                break;
            enemy.GetComponent<Enemy>().TakeDamage(AttackDamage, isHeavyAttack ? 2f : 1f);
        }
        yield return new WaitForSeconds(endAttackDelay);
        isAttacking = false;
    }
    
    // show gizmos in editor
    private void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }


    protected override void OnLanding() {
        dust.Play();
        animator_.SetBool(Grounded, true);
        // Debug.Log("sus");
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        // Grounding Controller
        Collider2D col = other.collider;
        float colX = col.transform.position.x;
        float charX = transform.position.x;
        float colW = col.bounds.extents.x;
        float charW = boxCollider.bounds.extents.x;

        if (!col.isTrigger && Mathf.Abs(charX - colX) < Mathf.Abs(colW) + Mathf.Abs(charW) - 0.01f)
        {
            if (colliding.Count == 0) {
                OnLanding();
            }
            colliding.Add(col);

        }
    }
    private void OnCollisionExit2D(Collision2D other)
    {
        colliding.Remove(other.collider);
    }

}
