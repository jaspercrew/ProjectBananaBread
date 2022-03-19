using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharController : LivingThing
{
    public static CharController Instance;
    // Components
    private BoxCollider2D charCollider;
    private ParticleSystem dust;
    private ParticleSystem sliceDashPS;
    private ParticleSystem parryPS;
    private ParticleSystem switchPS;
    private TrailRenderer trailRenderer;
    private ScreenShakeController screenShakeController;
    private RadialGrapple grappleController;
    private SpriteRenderer spriteRenderer;
    private LineRenderer grappleLineRenderer;
    private LineRenderer grappleLOSRenderer;

    // Configurable player control values
    public float speed = 10f;
    private const int SliceDamage = 100;
    private const float MinGroundSpeed = 0.5f;
    private const float OnGroundAcceleration = 28f;
    private const float OnGroundDeceleration = 30f;
    private const float InAirAcceleration = 10f;
    private const float InAirDrag = 1.5f;
    private const float JumpForce = 6.3f;
    private const int HeavyAttackBuildup = 4;
    private const float AttackCooldown = 0.5f;
    private const float ParryCooldown = 1f;
    private const float DashCooldown = 1f;
    private const int AttackDamage = 10;
    private const float ComboResetThreshold = 1f;
    public float attackRange = .25f;
    public LayerMask enemyLayers;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform slicePoint;
    
    
    //Children
    private Transform particleChild;
    
    // Trackers
    private float lastAttackTime;
    private bool isAttacking;
    private int comboCounter;
    private IEnumerator attackCoroutine;
    
    
    private float lastParryTime;
    public bool isParrying;
    
    private float lastDashTime;
    
    private bool isSliceDashing;
    
    public bool isInverted;
    
    private bool isGrounded;
    
    private bool isInvincible;
    
    private bool isGrappleLaunched;
    private bool isLineGrappling;
    public Rigidbody2D grappleProjectile;
    private Rigidbody2D sentProjectile;

    private bool grappleBlocked;
    private GrapplePoint launchedPoint;
    private GrapplePoint hookedPoint;
    public bool isRecentlyGrappled;
    
    private bool justJumped;

    public bool isCrouching;
    private bool canDoubleJump;
    
    private bool canYoink;
    private bool canCast;
    
    private bool isWallSliding;
    private int wallJumpDir;
    //private int wallJumpFramesLeft;
    public WindEmitter currentWindZone;
    
    private int fadeSpriteIterator;
    public float fadeTime;
    
    private float moveVector;
    private float inputVector;
    private float prevMoveVector = 2;
    private int forcedMoveVector;
    private float forcedMoveTime;

    private LayerMask obstacleLayerMask;
    
    private readonly LinkedList<Event> eventQueue = new LinkedList<Event>();

    private class Event
    {
        public const float EventTimeout = 0.25f;
        public readonly EventTypes EventType;
        public readonly float TimeCreated;
        
        public enum EventTypes
        {
            Dash, Jump, DoubleJump, Attack, Parry, Interact, SwitchState, SliceDash, Crouch, Cast, Yoink, Grapple
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
            {() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.Jump},
            {() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.DoubleJump},
            {() => Input.GetMouseButtonDown(0), Event.EventTypes.Attack},
            {() => Input.GetMouseButtonDown(1), Event.EventTypes.Parry},
            {() => Input.GetKeyDown(KeyCode.E), Event.EventTypes.Interact},
            {() => Input.GetKeyDown(KeyCode.F), Event.EventTypes.SwitchState},
            {() => Input.GetKeyDown(KeyCode.R), Event.EventTypes.SliceDash},
            {() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch},
            {() => Input.GetKeyDown(KeyCode.V), Event.EventTypes.Cast},
            {() => Input.GetKeyDown(KeyCode.V), Event.EventTypes.Yoink},
            {() => Input.GetKeyDown(KeyCode.G), Event.EventTypes.Grapple},
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
                @this.IsAbleToMove() && (@this.isGrounded || @this.isWallSliding)},
            {Event.EventTypes.DoubleJump, @this => 
                @this.IsAbleToMove() && !@this.isGrounded && !@this.isWallSliding && 
                @this.canDoubleJump && Input.GetKeyDown(KeyCode.Space)},
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
                @this => @this.IsAbleToAct()},
            {Event.EventTypes.Cast, 
                @this => @this.IsAbleToAct() && @this.castProjectileRb == null && @this.canCast},
            {Event.EventTypes.Yoink, 
                @this => @this.IsAbleToAct() && @this.castProjectileRb != null && @this.canYoink},
            {Event.EventTypes.Grapple, 
                @this => @this.IsAbleToAct()}
        };

    // maps from event type to a void function (action) that actually executes the action
    // associated with that event type
    private static readonly Dictionary<Event.EventTypes, Action<CharController>> EventActions =
        new Dictionary<Event.EventTypes, Action<CharController>>
        {
            {Event.EventTypes.Dash, @this => @this.DoDash()},
            {Event.EventTypes.Jump, @this => @this.DoJump()},
            {Event.EventTypes.DoubleJump, @this => @this.DoDoubleJump()},
            {Event.EventTypes.Attack, @this => @this.AttemptAttack()},
            {Event.EventTypes.Parry, @this => @this.DoParry()},
            {Event.EventTypes.Interact, @this => @this.DoInteract()},
            {Event.EventTypes.SwitchState, @this => @this.CauseSwitch()},
            {Event.EventTypes.SliceDash, @this => @this.DoSliceDash()},
            {Event.EventTypes.Crouch, @this => @this.Crouch()},
            {Event.EventTypes.Cast, @this => @this.DoCast()},
            {Event.EventTypes.Yoink, @this => @this.DoYoink()},
            {Event.EventTypes.Grapple, @this => @this.AttemptLaunchGrapple()},
        };


    private bool IsAbleToMove()
    {
        return !isAttacking && !isDashing && !isParrying && !grappleController.isGrappling && !isLineGrappling;
    }

    private bool IsAbleToBeDamaged() {
        return !isInvincible && !isDashing;
    }

    private IEnumerator ParticleBurstCoroutine(ParticleSystem ps, float time)
    {
        ps.Play();
        yield return new WaitForSeconds(time);
        ps.Stop();
        ps.Clear();
    }

    private bool IsAbleToAct() {
        return !isDashing && !isAttacking && !isParrying && !grappleController.isGrappling && !isSliceDashing;
    }

    public void Invert() {
        
        Rigidbody.gravityScale = -Mathf.Abs(Rigidbody.gravityScale);
        if (!isInverted)
        {
            transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        }
        
        spriteRenderer.flipX = true;
        isInverted = true;
    }
    
    public void DeInvert() {
        Rigidbody.gravityScale = Mathf.Abs(Rigidbody.gravityScale);
        if (isInverted)
        {
            transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        }
        spriteRenderer.flipX = false;
        isInverted = false;
    }

    private void Interrupt() {
        StopAllCoroutines();
        //Rigidbody.velocity = Vector2.zero;
        isAttacking = false;
        isCrouching = false;
        isParrying = false;
        isSliceDashing = false;
        isDashing = false;
    }

}
