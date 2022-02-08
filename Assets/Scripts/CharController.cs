using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharController : LivingThing {
    // Components
    private BoxCollider2D boxCollider;
    private ParticleSystem dust;
    private ParticleSystem SlicedashPS;
    private ScreenShakeController screenShakeController;
    private RadialGrapple grappleController;
    private SpriteRenderer spriteRenderer;

    // Configurable player control values
    public float speed = 7f;
    private const int SliceDamage = 100;
    private const float InAirAcceleration = 1f;
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
    [SerializeField] private Transform slicePoint;
    [SerializeField] private float attackRange;
    
    //Children
    private Transform particleChild;
    
    // Trackers
    [HideInInspector]
    public bool isInverted;
    private bool isInvincible;
    public bool isRecentlyGrappled;
    // private float nextParryTime;
    public bool isParrying;
    public bool isCrouching;
    private bool canDoubleJump;
    private bool isWallSliding;
    private bool isWallTouching;
    private Collider2D wallTouchingCollider;
    private WallJumpDirection wallJumpDir;
    private int wallJumpFramesLeft;
    // private float nextAttackTime;
    // private float nextRollTime;
    private int comboCounter;
    private bool isAttacking;
    private bool isSliceDashing;
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
            Dash, Jump, Attack, Parry, Interact, SwitchState, SliceDash, Crouch, 
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
            {() => Input.GetKeyDown(KeyCode.E), Event.EventTypes.Interact},
            {() => Input.GetKeyDown(KeyCode.F), Event.EventTypes.SwitchState},
            {() => Input.GetKeyDown(KeyCode.R), Event.EventTypes.SliceDash},
            {() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch}
        };

    // maps from event type to a boolean function that says whether the conditions for the 
    // event to happen are met, and thus whether it should happen
    //
    // for some reason, the key-value pairs are static contexts, so you can't use variables or call
    // methods of CharController, so we have to explicitly pass an instance of a CharController
    // (i.e. we later explicitly pass in a "this"). we can access private variables just fine since
    // we're inside the class definition
    // TODO: add these to the beginnings of each of the respective functions
    private static readonly Dictionary<Event.EventTypes, Func<CharController, bool>> EventConditions =
        new Dictionary<Event.EventTypes, Func<CharController, bool>>
        {
            {Event.EventTypes.Dash, @this =>
                (@this.IsAbleToAct() || @this.isAttacking) && Time.time > @this.lastDashTime + DashCooldown},
            {Event.EventTypes.Jump, @this => 
                @this.IsAbleToMove() && (@this.IsGrounded() || @this.isWallSliding || @this.canDoubleJump)},
            {Event.EventTypes.Attack, @this => 
                @this.IsAbleToAct() && Time.time > @this.lastAttackTime + AttackCooldown},
            {Event.EventTypes.Parry, @this =>
                @this.IsAbleToAct() && Time.time > @this.lastParryTime + ParryCooldown},
            {Event.EventTypes.Interact, 
                @this => @this.IsAbleToAct()},
            {Event.EventTypes.SwitchState, 
                @this => @this.IsAbleToAct()},
            {Event.EventTypes.SliceDash, @this => 
                (@this.IsAbleToAct() || @this.isAttacking) && Time.time > @this.lastDashTime + DashCooldown},
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
            {Event.EventTypes.Interact, @this => @this.DoInteract()},
            {Event.EventTypes.SwitchState, @this => GameManager.Instance.SwitchWorldState()},
            {Event.EventTypes.SliceDash, @this => @this.DoSliceDash()},
            {Event.EventTypes.Crouch, @this => @this.Crouch()}
        };
    

    private enum WallJumpDirection
    {
        Left = -1, None = 0, Right = 1
    }

    private bool IsGrounded() {
        return colliding.Count > 0;
    }

    private bool IsAbleToMove() {
        return !isAttacking && !isDashing && !isParrying && !grappleController.isGrappling &&
               wallJumpDir == WallJumpDirection.None;
    }

    private bool IsAbleToBeDamaged() {
        return !isInvincible && !isDashing;
    }

    private bool IsAbleToAct() {
        return !isDashing && !isAttacking && !isParrying && !grappleController.isGrappling && !isSliceDashing;
    }

    public void Invert() {
        isInverted = true;
        Rigidbody.gravityScale = -Mathf.Abs(Rigidbody.gravityScale);
        transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        spriteRenderer.flipX = true;
    }
    
    public void DeInvert() {
        isInverted = false;
        Rigidbody.gravityScale = Mathf.Abs(Rigidbody.gravityScale);
        transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        spriteRenderer.flipX = false;
    }

    public void Interrupt() {
        StopAllCoroutines();
        Rigidbody.velocity = Vector2.zero;
        isAttacking = false;
        isCrouching = false;
        isParrying = false;
        isSliceDashing = false;
        isDashing = false;
    }

}
