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
    private const int HeavyAttackBuildup = 4;
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
        public const float EventTimeout = 0.25f;
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

    // maps from a boolean function to an event, where the function, when called, returns whether
    // the event's respective button is being pressed, and thus whether the event be queued
    private static readonly Dictionary<Func<bool>, Event.EventTypes> KeyToEventType =
        new Dictionary<Func<bool>, Event.EventTypes>
        {
            {() => Input.GetKeyDown(KeyCode.LeftShift), Event.EventTypes.Dash},
            {() => Input.GetButtonDown("Jump"), Event.EventTypes.Jump},
            {() => Input.GetMouseButtonDown(0), Event.EventTypes.Attack},
            {() => Input.GetMouseButtonDown(1), Event.EventTypes.Parry},
            {() => Input.GetKeyDown(KeyCode.F), Event.EventTypes.SwitchState},
            {() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch}
        };

    // maps from event type to a boolean function that says whether the conditions for the 
    // event to happen are met, and thus whether it should happen
    //
    // for some reason, the key-value pairs are static contexts, so you can't use variables or call
    // methods of CharController, so we have to explicitly pass an instance of a CharController
    // (i.e. we later explicitly pass in a "this"). we can access private variables just fine since
    // we're inside the class definition
    private static readonly Dictionary<Event.EventTypes, Func<CharController, bool>> EventConditions =
        new Dictionary<Event.EventTypes, Func<CharController, bool>>
        {
            {Event.EventTypes.Dash, @this =>
                (@this.IsAbleToAct() || @this.isAttacking) && Time.time > @this.lastDashTime + DashCooldown},
            {Event.EventTypes.Jump, @this => 
                @this.IsGrounded() && @this.IsAbleToMove()},
            {Event.EventTypes.Attack, @this => 
                @this.IsAbleToAct() && Time.time > @this.lastAttackTime + AttackCooldown},
            {Event.EventTypes.Parry, @this =>
                @this.IsAbleToAct() && Time.time > @this.lastParryTime + ParryCooldown},
            {Event.EventTypes.SwitchState, 
                @this => @this.IsAbleToAct()},
            {Event.EventTypes.Crouch, 
                @this => @this.IsAbleToAct()}
        };

    // maps from event type to a void function (action) that actually executes the action
    // associated with that event type
    private static readonly Dictionary<Event.EventTypes, Action<CharController>> EventActions =
        new Dictionary<Event.EventTypes, Action<CharController>>
        {
            {Event.EventTypes.Dash, @this => @this.DoDash()},
            {Event.EventTypes.Jump, @this => @this.DoJump()},
            {Event.EventTypes.Attack, @this => @this.AttemptAttack()},
            {Event.EventTypes.Parry, @this => @this.DoParry()},
            {Event.EventTypes.SwitchState, @this => GameManager.Instance.SwitchWorldState()},
            {Event.EventTypes.Crouch, @this => @this.Crouch()}
        };
    

    private void Start() {
        CurrentHealth = MaxHealth;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.GetComponent<Animator>();
        boxCollider = transform.GetComponent<BoxCollider2D>();
        dust = transform.GetComponentInChildren<ParticleSystem>();
        screenShakeController = FindObjectOfType<Camera>().GetComponent<ScreenShakeController>();
        grappleController = GetComponent<RadialGrapple>();
    }

    
    private void FixedUpdate() {
        moveVector = Input.GetAxisRaw("Horizontal");
        if (!IsAbleToMove()) return;
        
        // movement animations
        Animator.SetInteger(AnimState, Mathf.Abs(moveVector) > float.Epsilon? 2 : 0);

        // actual moving TODO add velocity overriding 
        transform.position += new Vector3(moveVector * speed * Time.deltaTime, 0, 0);

        // feet dust logic
        if (Math.Abs(xDir - moveVector) > 0.01f && IsGrounded() && moveVector != 0) {
            dust.Play();
        }
        
        xDir = moveVector;

        Vector3 scale = transform.localScale;
        float xScale = scale.x;
        float yScale = scale.y;
        // direction switching
        if (moveVector > 0 && Math.Abs(xScale + 1) > float.Epsilon) {
            transform.localScale = new Vector3(-1, yScale, 0);
        }
        else if (moveVector < 0 && Math.Abs(xScale - 1) > float.Epsilon) {
            transform.localScale = new Vector3(1, yScale, 0);
        }
    }

    private void Update() {
        // add events if their respective buttons are pressed
        foreach (KeyValuePair<Func<bool>, Event.EventTypes> pair in KeyToEventType)
        {
            if (pair.Key.Invoke())
            {
                // Debug.Log("enqueueing " + pair.Value + " event");
                eventQueue.AddLast(new Event(pair.Value, Time.time));
            }
        }
        
        // parse event queue
        for (LinkedListNode<Event> node = eventQueue.First; node != null; node = node.Next)
        {
            Event e = node.Value;
            
            // remove expired events
            if (Time.time > e.TimeCreated + Event.EventTimeout)
            {
                // Debug.Log(e.EventType + " event timed out");
                eventQueue.Remove(node);
                // continue;
            }
            // execute events whose conditions are met, and remove those whose aren't
            else
            {
                Func<CharController, bool> conditions = EventConditions[e.EventType];
                Action<CharController> actionToDo = EventActions[e.EventType];
                if (conditions.Invoke(this))
                {
                    // Debug.Log("reached enqueued " + e.EventType + ", invoking");
                    actionToDo.Invoke(this);
                    eventQueue.Remove(node);
                }
            }
        }
        
        if (IsGrounded()) {
            Animator.SetBool(Jump, false);
        }

        if (Input.GetButtonUp("Jump") && !IsGrounded() && Rigidbody.velocity.y > 0) {
            Rigidbody.velocity = Vector2.Scale(Rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }

    private bool IsGrounded() {
        return colliding.Count > 0;
    }

    private bool IsAbleToMove() {
        return !isAttacking && !IsDashing && !isParrying && !grappleController.isGrappling;
    }

    private bool IsAbleToBeDamaged() {
        return !isInvincible && !IsDashing;
    }

    private bool IsAbleToAct()
    {
        return !IsDashing && !isAttacking && !isParrying && !grappleController.isGrappling;
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

    private void DoDash()
    {
        if (!IsAbleToAct() && !isAttacking) {
            return;
        }
        
        // attack cancel
        if (isAttacking)
        {
            Animator.SetTrigger(Idle); // TODO: dash animation
            isAttacking = false;
            Assert.IsNotNull(attackCoroutine);
            StopCoroutine(attackCoroutine);
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

    private void DoJump()
    {
        if (!IsGrounded() || !IsAbleToMove())
            return;
        
        dust.Play();
        Rigidbody.AddForce(new Vector2(0, JumpForce), ForceMode2D.Impulse);
        Animator.SetTrigger(Jump);
        Animator.SetBool(Grounded, false);
    }

    private void DoAttack(bool isHeavy) {
        // _screenShakeController.LightShake();
        isAttacking = true;
        Animator.speed = 1;
        // Assert.IsTrue(comboCount <= HeavyAttackBuildup);
        
        Animator.SetTrigger(Attack);
        // TODO: remove when we have actual animations
        if (isHeavy) { // heavy attack?
            Animator.speed = .5f;
        }

        attackCoroutine = AttackCoroutine(isHeavy);
        StartCoroutine(attackCoroutine);
        
        lastAttackTime = Time.time;
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

    // TODO: add variable isCrouching and set to true/false here instead of changing speed directly
    // and use isCrouching in movement and affect speed there
    private void Crouch()
    {
        if (!IsAbleToAct())
            return;
        isCrouching = !isCrouching;
        speed *= isCrouching? 0.5f : 2;
    }

    // Take damage, knock away from point
    public void TakeDamage(int damage, float knockback, Vector2 point) {
        if (!IsAbleToBeDamaged()) {
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

    // handles combo count for attacking 
    private void AttemptAttack()
    {
        if (!IsAbleToAct())
            return;
        
        // reset if combo reset threshold passed
        if (lastAttackTime + ComboResetThreshold < Time.time) {
            comboCounter = 0;
        }

        comboCounter++;
        // this line is equivalent to the 3 commented lines below
        bool isHeavy = Math.DivRem(comboCounter, HeavyAttackBuildup, out comboCounter) > 0;

        // bool isHeavy = comboCounter == HeavyAttackBuildup;
        // if (isHeavy)
        //     comboCounter -= HeavyAttackBuildup;
        
        DoAttack(isHeavy);
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
