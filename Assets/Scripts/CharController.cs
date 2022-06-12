using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;

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
    private Light2D charLight;
    // private RadialGrapple grappleController;
    private SpriteRenderer spriteRenderer;
    private LineRenderer grappleLineRenderer;
    // ReSharper disable once InconsistentNaming
    private LineRenderer grappleLOSRenderer;
    private LineRenderer grappleClearRenderer;
    private BoxCollider2D groundCheck;

    // private Transform parentWindFX;
    // private ParticleSystem leftWindFX;
    // private ParticleSystem rightWindFX;
    // private ParticleSystem upWindFX;
    // private ParticleSystem downWindFX;

    // Cape Config
    // [Header("Cape Offsets")] 
    //
    // [SerializeField]
    // private Vector2 idleOffset;
    //
    // [SerializeField]
    // private Vector2 runOffset;
    //
    // [SerializeField]
    // private Vector2 jumpOffset;
    //
    // [SerializeField]
    // private Vector2 fallOffset;
    
    //private CapeController capeAnchor;
    //private CapeController capeOutlineAnchor;
    
    [Header("Configurable player control values")] 
    // Configurable player control values
    public float speed = 10f;
    private const int SliceDamage = 100;
    
    private const float MinGroundSpeed = 0.5f;
    private const float OnGroundAcceleration = 38f;
    private const float OnGroundDeceleration = 30f;
    private const float InAirAcceleration = 18f;
    private const float InAirDrag = 1.5f;
    private const float MaxYSpeed = 30f;
    private const float dashBoost = 15f;
    // private const float VerticalDrag = 10f;
    [SerializeField]
    private float JumpForce = 12f;
    private const float baseGravity = 9.0f;
    private const int HeavyAttackBuildup = 4;
    private const float AttackCooldown = 0.5f;
    private const float ParryCooldown = 1f;
    private const float ParryTime = .4f;
    private const float DashCooldown = 1f;
    public const float ShiftCooldown = 1.5f;
    const float InvTime = 1.25f;
    public const float MaxFury = 100;
    public const float FuryIncrement = 10;
    private const int AttackDamage = 1;
    public const float MaxLightBuffer = 7f;
    private const float MaxLightIntensity = .5f;
    private const float MaxOuterLightRadius = 5;
    private const float MaxInnerLightRadius = 3;
    private const float ComboResetThreshold = 1.2f;
    private const float attackRange = 1.15f;
    public LayerMask enemyLayers;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private Transform slicePoint;
    
    public float gravityValue = baseGravity;
    //Children
    private Transform particleChild;
    
    // Trackers
    public float lightBuffer;
    private bool canFunction = true;
    private float lastAttackTime;
    private bool isAttacking;
    private int comboCounter;

    public float fury;

    private HashSet<IEnumerator> toInterrupt = new HashSet<IEnumerator>();
    private IEnumerator dashCoroutine;
    private IEnumerator attackCoroutine;

    private float lastShiftTime;
    
    private float lastParryTime;
    public bool isParrying;
    
    private float lastDashTime;
    
    private bool isSliceDashing;
    
    public bool isInverted;
    
    private bool _isGrounded;
    private bool isGrounded
    {
        get => _isGrounded;
        set
        {
            if (_isGrounded && !value)
            {
                StartCoroutine(JumpBufferCoroutine());
            }
            //Update the boolean variable
            _isGrounded = value;
        }
    }

    private bool jumpAvailable;
    public bool isInvincible;
    
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
    //private bool canDoubleJump;
    
    private bool canYoink;
    private bool canCast;

    private bool wallJumpAvailable;
    private bool _isWallSliding;
    private bool isWallSliding
    {
        get {return _isWallSliding;}
        set
        {
            if (_isWallSliding && !value)
            {
                StartCoroutine(WallJumpBufferCoroutine());
            }
            //Update the boolean variable
            _isWallSliding = value;
        }
    }
    private int wallJumpDir;
    //private int wallJumpFramesLeft;
    public WindInfo currentWind;
    
    private int fadeSpriteIterator;
    [FormerlySerializedAs("fadeTime")] public float emitFadesTime;
    
    private float moveVector;
    private float inputVector;
    private float prevInVector = 2;
    private int forcedMoveVector;
    private float forcedMoveTime;

    private LayerMask obstacleLayerMask;
    private LayerMask obstaclePlusLayerMask;
    
    private readonly LinkedList<Event> eventQueue = new LinkedList<Event>();

    private class Event
    {
        public const float EventTimeout = 0.25f;
        public readonly EventTypes EventType;
        public readonly float TimeCreated;
        
        public enum EventTypes
        {
            Dash, Jump, /*DoubleJump,*/ Attack, Parry, Interact, SwitchState, 
            SliceDash, Crouch, Cast, Yoink, Grapple
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
            //{() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.DoubleJump},
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
                @this.IsAbleToMove() && (@this.isGrounded || (@this.jumpAvailable && !@this.justJumped) ||
                                         @this.isWallSliding || (@this.wallJumpAvailable && !@this.justJumped))},
            // {Event.EventTypes.DoubleJump, @this => 
            //     @this.IsAbleToMove() && !@this.isGrounded && !@this.isWallSliding && 
            //     @this.canDoubleJump && Input.GetKeyDown(KeyCode.Space)},
            {Event.EventTypes.Attack, @this => 
                @this.IsAbleToAct() && Time.time > @this.lastAttackTime + AttackCooldown},
            {Event.EventTypes.Parry, @this =>
                @this.IsAbleToAct() && Time.time > @this.lastParryTime + ParryCooldown},
            {Event.EventTypes.Interact, 
                @this => @this.IsAbleToAct()},
            {Event.EventTypes.SwitchState, 
                @this => @this.IsAbleToAct() && Time.time > @this.lastShiftTime + ShiftCooldown},
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
            //{Event.EventTypes.DoubleJump, @this => @this.DoDoubleJump()},
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
        return !isAttacking && !isDashing && !isParrying && !isLineGrappling && canFunction;
    }

    private bool IsAbleToBeDamaged() {
        return !isInvincible && !isDashing && canFunction;
    }

    private IEnumerator ParticleBurstCoroutine(ParticleSystem ps, float time)
    {
        ps.Play();
        yield return new WaitForSeconds(time);
        ps.Stop();
        ps.Clear();
    }

    private IEnumerator WallJumpBufferCoroutine()
    {
        
        const float buffer = .15f;
        wallJumpAvailable = true;
        yield return new WaitForSeconds(buffer);
        wallJumpAvailable = false;
    }
    
    private IEnumerator JumpBufferCoroutine()
    {
        const float buffer = .15f;
        jumpAvailable = true;
        yield return new WaitForSeconds(buffer);
        jumpAvailable = false;
    }

    private bool IsAbleToAct() {
        return !isDashing && !isAttacking && !isParrying && !isSliceDashing && canFunction;
    }

    public void Invert() {
        gravityValue = -Mathf.Abs(gravityValue);
        if (!isInverted)
        {
            transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        }
        
        spriteRenderer.flipX = true;
        isInverted = true;
    }
    
    public void DeInvert() {
        gravityValue = Mathf.Abs(gravityValue);
        if (isInverted)
        {
            transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        }
        spriteRenderer.flipX = false;
        isInverted = false;
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (SceneInformation.Instance.isGravityScene)
        {
            Invert();
        }
        if (SceneInformation.Instance.isWindScene)
        {
            currentWind = SceneInformation.Instance.altStateWind;

        }
        if (SceneInformation.Instance.isDarkScene)
        {
            
        }
    }

    // protected override void CheckEntity()
    // {
    //     base.CheckEntity();
    //     Debug.Log("char checked");
    // }
    
    

    protected override void TurnUnshifted()
    {
        //Debug.Log("char unshifted");
        base.TurnUnshifted();
        if (SceneInformation.Instance.isGravityScene)
        {
            DeInvert();
        }
        if (SceneInformation.Instance.isWindScene)
        {
            currentWind = SceneInformation.Instance.realStateWind;
            
        }
        if (SceneInformation.Instance.isDarkScene)
        {
            
        }
    }

    public IEnumerator InvFrameCoroutine(float time)
    {
        float blinkTime = .15f;
        float elapsedTime = 0;
        bool isHidden = false;

        while (elapsedTime < time)
        {
            if (isHidden)
            {
                yield return new WaitForSeconds(blinkTime);
                isHidden = false;
                spriteRenderer.forceRenderingOff = false;
            }
            else
            {
                yield return new WaitForSeconds(blinkTime); 
                isHidden = true;
                spriteRenderer.forceRenderingOff = true;
            }

            elapsedTime += blinkTime;
        }  
        // Make sure we got there
        spriteRenderer.forceRenderingOff = false;
        yield return null;
    }




    private void Interrupt() {
        foreach (IEnumerator co in toInterrupt)
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
        }
        
        //Rigidbody.velocity = Vector2.zero;
        isAttacking = false;
        isCrouching = false;
        isParrying = false;
        isSliceDashing = false;
        isDashing = false;
    }

    public bool IFrames()
    {
        return isDashing || isInvincible || isSliceDashing || !canFunction;
    }

    public void PrepForScene()
    {
        Interrupt();
        currentWind = null;
    }

}
