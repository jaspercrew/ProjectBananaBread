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
    private RadialGrapple grappleController;
    
    // Configurable player control values
    private float speed = 3.5f;
    private const float JumpForce = 6.3f;
    // TODO: to implement cooldowns, add a condition to the eventConditions dictionary
    private const float AttackCooldown = 0.5f;
    private const float ParryCooldown = 1f;
    private const float DashCooldown = 1f;
    private float lastAttackTime;
    private float lastParryTime;
    private float lastDashTime;
    private const int AttackDamage = 10;
    private const float ComboResetThreshold = 1f;
    public LayerMask enemyLayers;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private float attackRange;
    
    // Trackers
    private bool isInvincible;
    // private float nextParryTime;
    public bool isParrying;
    public bool isCrouching;
    // private float nextAttackTime;
    // private float nextRollTime;
    private int comboCounter;
    private bool isAttacking;
    private float moveVector;
    private float xDir = 2;
    private readonly HashSet<Collider2D> colliding = new HashSet<Collider2D>();
    private IEnumerator attackCoroutine;

    private readonly LinkedList<Event> eventQueue = new LinkedList<Event>();

    private class Event
    {
        public const float EventTimeout = 0.5f;
        public readonly EventTypes EventType;
        public readonly float TimeCreated;
        
        public enum EventTypes
        {
            Dash, Jump, Attack, Parry, SwitchState, Crouch
        }

        public Event(EventTypes type, float time)
        {
            EventType = type;
            TimeCreated = time;
        }
    }

    private readonly Dictionary<Func<bool>, Event.EventTypes> keyToEventType =
        new Dictionary<Func<bool>, Event.EventTypes>
        {
            {() => Input.GetKeyDown(KeyCode.LeftShift), Event.EventTypes.Dash},
            {() => Input.GetButtonDown("Jump"), Event.EventTypes.Jump},
            {() => Input.GetMouseButtonDown(0), Event.EventTypes.Attack},
            {() => Input.GetMouseButtonDown(1), Event.EventTypes.Parry},
            {() => Input.GetKeyDown(KeyCode.F), Event.EventTypes.SwitchState},
            {() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch}
        };

    private readonly Dictionary<Event.EventTypes, Func<CharController, bool>> eventConditions =
        new Dictionary<Event.EventTypes, Func<CharController, bool>>
        {
            {Event.EventTypes.Dash, @this =>
                (@this.IsAbleToAct() || @this.isAttacking) && Time.time > @this.lastDashTime + DashCooldown},
            {Event.EventTypes.Jump, @this => @this.IsGrounded() && @this.IsAbleToMove()},
            {Event.EventTypes.Attack, @this => 
                @this.IsAbleToAct() && Time.time > @this.lastAttackTime + AttackCooldown},
            {Event.EventTypes.Parry, @this =>
                @this.IsAbleToAct() && Time.time > @this.lastParryTime + ParryCooldown},
            {Event.EventTypes.SwitchState, @this => @this.IsAbleToAct()},
            {Event.EventTypes.Crouch, @this => @this.IsAbleToAct()}
        };

    private readonly Dictionary<Event.EventTypes, Action<CharController>> eventActions =
        new Dictionary<Event.EventTypes, Action<CharController>>
        {
            {Event.EventTypes.Dash, @this => @this.DoDash()},
            {Event.EventTypes.Jump, @this => @this.DoJump()},
            {Event.EventTypes.Attack, @this => @this.AttemptAttack()},
            {Event.EventTypes.Parry, @this => @this.DoParry()},
            {Event.EventTypes.SwitchState, @this => GameManager.Instance.SwitchWorldState()},
            {Event.EventTypes.Crouch, @this => @this.Crouch()}
        };
    
    

    // TODO: event queueing system?
    // eventTimeout = 0.5;
    // event queue = [(attack, t1), (dash, t2), ...]
    
    // update()
    // top = queue.peek()
    // if (top.time + eventTimeout >= Time.time)
    //      queue.pop()
    // do this again, until: queue is empty, or event is reached, then do as many events as possible
    // also, once this is done, fix nextAttackTime and nextDashTime or whatever it was

    // Start is called before the first frame update
    private void Start() {
        CurrentHealth = MaxHealth;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.GetComponent<Animator>();
        boxCollider = transform.GetComponent<BoxCollider2D>();
        dust = transform.GetComponentInChildren<ParticleSystem>();
        screenShakeController = FindObjectOfType<Camera>().GetComponent<ScreenShakeController>();
        grappleController = GetComponent<RadialGrapple>();
    }

    // Update is called once per frame
    private void FixedUpdate() {
        moveVector = Input.GetAxisRaw("Horizontal");
        // Debug.Log(moveVector);

        if (!IsAbleToMove()) return;
        
        // movement animations
        Animator.SetInteger(AnimState, Mathf.Abs(moveVector) > Mathf.Epsilon? 2 : 0);

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
        return !IsDashing && !isAttacking && !isParrying && !grappleController.isGrappling;
    }

    private void Update() {
        // if (Time.time >= nextAttackTime && !isAttacking) {
        //     AttemptAttack();
        // }

        // add events if their respective buttons are pressed
        foreach (KeyValuePair<Func<bool>, Event.EventTypes> pair in keyToEventType)
        {
            if (pair.Key.Invoke())
            {
                Debug.Log("enqueueing " + pair.Value + " event");
                eventQueue.AddLast(new Event(pair.Value, Time.time));
            }
        }
        
        
        for (LinkedListNode<Event> node = eventQueue.First; node != null; node = node.Next)
        {
            Event e = node.Value;
            if (Time.time > e.TimeCreated + Event.EventTimeout)
            {
                Debug.Log(e.EventType + " event timed out");
                eventQueue.Remove(node);
                // continue;
            }
            else
            {
                Func<CharController, bool> conditions = eventConditions[e.EventType];
                Action<CharController> actionToDo = eventActions[e.EventType];
                if (conditions.Invoke(this))
                {
                    Debug.Log("reached enqueued " + e.EventType + ", invoking");
                    actionToDo.Invoke(this);
                    eventQueue.Remove(node);
                }
                else
                {
                    // Debug.Log("reached enqueued " + e.EventType +
                    //           ", but skipping because conditions are not met");
                }
            }
            
            // TODO: wrong
            // at this point, the first event in the queue is guaranteed not to be expired,
            // and since they're sorted in order of creation, the rest are also guaranteed to 
            // not be expired. therefore, the rest of the queue contains all the queued events
            // that should be executed

            // Debug.Log(eventQueue.Count + " events to be executed");
            // while (eventQueue.Count > 0)
            // {
            //     Event.EventTypes eventType = eventQueue.Dequeue().EventType;
            //     Debug.Log("executing " + eventType);
            //     Action<CharController> actionToDo = eventTypeToAction[eventType];
            //     actionToDo.Invoke(this);
            // }
        }
        
        if (IsGrounded()) {
            Animator.SetBool(Jump, false);
        }

        // if (Input.GetKeyDown(KeyCode.LeftShift) && Time.time >= nextRollTime) {
        //     DoDash();
        //     nextRollTime = Time.time + 1f / RollRate;
        // }

        // if (Input.GetKeyDown(KeyCode.F) && IsAbleToAct()) {
            // switch states
            // GameObject.FindObjectOfType<BinaryPlatform>().SwitchState(EnvironmentState.Cyberpunk);
            // Debug.Log(GameManager.Instance);
            // GameManager.Instance.SwitchWorldState();
        // }

        // grapple
        // if (Input.GetKeyDown(KeyCode.E) && IsAbleToAct()) {
        //     if (targetGrappleController.isGrappling) {
        //         targetGrappleController.EndGrapple();
        //         // return;
        //     }
        //     else
        //     {
        //         targetGrappleController.StartGrapple();
        //     }
        // }
        
        // if (Input.GetMouseButtonDown(1) && IsAbleToAct() && Time.time >= nextParryTime) {
        //     DoParry();
        //     nextParryTime = Time.time + 1f / ParryRate;
        // }
        
        // if (Input.GetButtonDown("Jump") && IsGrounded() && IsAbleToMove()) {
        //     DoJump();                     
        // }
        //
        // if (Input.GetButtonDown("Fire1")) { // crouch
        //     Crouch();
        // }
        //
        // if (Input.GetButtonUp("Fire1")) { // uncrouch
        //     UnCrouch();
        // }

        // short jump
        if (Input.GetButtonUp("Jump") && !IsGrounded() && Rigidbody.velocity.y > 0) {
            Rigidbody.velocity = Vector2.Scale(Rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }

    private void DoParry()
    {
        if (!IsAbleToAct())
            return;
        // start parry animation
        Animator.SetTrigger(Parry);
        isParrying = true;
        StartCoroutine(ParryCoroutine());
        
        lastParryTime = Time.time;
    }

    private IEnumerator ParryCoroutine() {
        const float parryTime = .5f;
        yield return new WaitForSeconds(parryTime);
        isParrying = false;
        //transform.GetComponent<SpriteRenderer>().flipY = false;
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
    
    private void DoDash()
    {
        // Debug.Log("starting dash");
        if (!IsAbleToAct() && !isAttacking) {
            // Debug.Log("stopping dash cuz bad");
            return;
        }
        
        // attack cancel
        if (isAttacking)
        {
            // Debug.Log("interrupting attack with dash");
            Animator.SetTrigger(Idle); // TODO: dash animation
            isAttacking = false;
            Assert.IsNotNull(attackCoroutine);
            StopCoroutine(attackCoroutine);
            // animator_.SetInteger(AnimState, 0); // TODO
        }

        const float dashSpeed = 9f;
        const float dashTime = .23f;

        float xScale = transform.localScale.x;
        if (Mathf.Abs(xScale) > .5) {
            VelocityDash(xScale > 0? 3 : 1, dashSpeed, dashTime);
            dust.Play();
            Animator.SetTrigger(Dash);
        }

        lastDashTime = Time.time;
    }

    // TODO: add variable isCrouching and set to true/false here instead of changing speed directly
    // and use isCrouching in movement and affect speed there
    private void Crouch()
    {
        if (!IsAbleToAct())
            return;
        speed *= isCrouching ? 0.5f : 2;
        isCrouching = !isCrouching;
    }

    // private void UnCrouch() {
    //     speed *= 2;
    // }

    private bool IsGrounded() {
        return colliding.Count > 0;
    }

    private bool IsAbleToMove() {
        return !isAttacking && !IsDashing && !isParrying && !grappleController.isGrappling;
    }

    private bool AbleToBeDamaged() {
        return !isInvincible && !IsDashing;
    }
    
    // Take damage, knock away from point
    public void TakeDamage(int damage, float knockback, Vector2 point) {
        if (!AbleToBeDamaged()) {
            return;
        }
        StartCoroutine(TakeDamageCoroutine());
        KnockAwayFromPoint(knockback, point);
        CurrentHealth -= damage;
        // damage animation
        Animator.SetTrigger(Hurt);
        
        if (CurrentHealth <= 0) {
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
    
    protected override void Die() 
    {
        Animator.SetTrigger(Death);
        transform.GetComponent<Collider>().enabled = false;
        Rigidbody.gravityScale = 0;
    }

    private void DoJump()
    {
        if (!IsGrounded() || !IsAbleToMove())
            return;
        
        dust.Play();
        Rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        Animator.SetTrigger(Jump);
        Animator.SetBool(Grounded, false);
    }
    
    // handles boolean checking and combo count for attacking 
    private void AttemptAttack()
    {
        if (!IsAbleToAct())
            return;
        if (Time.time < lastAttackTime + ComboResetThreshold) {
            // continue combo
            comboCounter++;
                
            if (comboCounter >= 4) { // heavy
                comboCounter = 0;
                // nextAttackTime = Time.time + 2f / AttackRate;
            }
            // else { // light
            //     nextAttackTime = Time.time + 1f / AttackRate;
            // }
            // lastAttackTime = Time.time;
        }
        else {
            // start new attack chain 
            comboCounter = 1;
            // DoAttack(comboCounter);
                
            // nextAttackTime = Time.time + 1f / AttackRate;
            // lastAttackTime = Time.time;
        }
        
        DoAttack(comboCounter);
    }
    
    private void DoAttack(int comboCount) {
        // _screenShakeController.LightShake();
        isAttacking = true;
        Animator.speed = 1;
        Assert.IsTrue(comboCount <= 4);
        
        Animator.SetTrigger(Attack);
        // TODO: remove when we have actual animations
        if (comboCount == 4) { // heavy attack?
            Animator.speed = .5f;
        }

        attackCoroutine = AttackCoroutine(comboCount == 4);
        StartCoroutine(attackCoroutine);
        
        lastAttackTime = Time.time;
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
            endAttackDelay = .5f;
            hitConfirmDelay = .30f;
        }
    
        yield return new WaitForSeconds(beginAttackDelay);
        
        
        // move while attacking TODO : change this functionality
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


    private void OnLanding() {
        dust.Play();
        Animator.SetBool(Grounded, true);
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
