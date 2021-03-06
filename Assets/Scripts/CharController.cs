using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;

public partial class CharController : BeatEntity
{
    public static CharController Instance;

    public static Vector3 position
    {
        get => Instance.transform.position;
        set => Instance.transform.position = value;
    }
    
    // Components
    private BoxCollider2D charCollider;
    private ParticleSystem dust;
    private ScreenShakeController screenShakeController;
    private Light2D charLight;
    // private RadialGrapple grappleController;
    private SpriteRenderer spriteRenderer;
    private LineRenderer grappleLineRenderer;
    // ReSharper disable once InconsistentNaming
    private LineRenderer grappleLOSRenderer;
    private LineRenderer grappleClearRenderer;
    private BoxCollider2D groundCheck;
    private Rigidbody2D Rigidbody;
    private Animator Animator;
    
    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    
    
    [Header("Configurable player control values")] 
    // Configurable player control values
    public float baseSpeed = 8f;
    private float speed;

    private const float MinGroundSpeed = 0.5f;
    private const float OnGroundAcceleration = 38f;
    private const float OnGroundDeceleration = 30f;
    private const float InAirAcceleration = 18f;
    private const float InAirDrag = 1.5f;
    private const float MaxYSpeed = 30f;
    private const float DashBoost = 15f;
    private const float heightReducer = 4f;
    // private const float VerticalDrag = 10f;
    [SerializeField]
    private float jumpForce = 12f;
    private const float BaseGravity = 0.0f;
    

    private const float DashCooldown = 1f;

    public bool isDashing;
    public float gravityValue = BaseGravity;
    //Children
    private Transform particleChild;
    
    // Trackers
    private bool canFunction = true;
    

    private HashSet<IEnumerator> toInterrupt = new HashSet<IEnumerator>();
    private IEnumerator dashCoroutine;

    private float lastDashTime;

    public bool isInverted;
    
    // ReSharper disable once InconsistentNaming
    private bool isPlatformGrounded;
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

    private bool isGrappleLaunched;
    private bool isLineGrappling;
    public Rigidbody2D grappleProjectile;
    private Rigidbody2D sentProjectile;

    private bool grappleBlocked;
    private GrapplePoint launchedPoint;
    // ReSharper disable once NotAccessedField.Local
    private GrapplePoint hookedPoint;
    public bool isRecentlyGrappled;
    
    private bool justJumped;

    public bool isCrouching;
    //private bool canDoubleJump;
    

    private bool wallJumpAvailable;
    // ReSharper disable once InconsistentNaming
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

    private int fadeSpriteIterator;
    [FormerlySerializedAs("fadeTime")] public float emitFadesTime;
    
    private float moveVector;
    private float inputVector;
    private float prevInVector = 2;
    private int forcedMoveVector;
    private float forcedMoveTime;
    
    // animator values beforehand to save time later
    protected static readonly int AnimState = Animator.StringToHash("AnimState");
    protected static readonly int Idle = Animator.StringToHash("Idle");
    protected static readonly int Jump = Animator.StringToHash("Jump");
    protected static readonly int Death = Animator.StringToHash("Death");
    protected static readonly int Grounded = Animator.StringToHash("Grounded");
    protected static readonly int Dash = Animator.StringToHash("Dash");
    
    
    private LayerMask obstacleLayerMask;
    private LayerMask obstaclePlusLayerMask;
    private LayerMask platformLayerMask;
    
    private readonly LinkedList<Event> eventQueue = new LinkedList<Event>();

    private class Event
    {
        public const float EventTimeout = 0.25f;
        public readonly EventTypes EventType;
        public readonly float TimeCreated;
        
        public enum EventTypes
        {
            Dash, Jump, Interact, 
             Crouch, Grapple
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
            {() => Input.GetKeyDown(KeyCode.E), Event.EventTypes.Interact},
            {() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch},
            {() => Input.GetKeyDown(KeyCode.G), Event.EventTypes.Grapple},
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
                (@this.IsAbleToAct()) && Time.time > @this.lastDashTime + DashCooldown &&
                !@this.isCrouching},
            {Event.EventTypes.Jump, @this => 
                @this.IsAbleToMove() && 
                (@this.isGrounded || (@this.jumpAvailable && !@this.justJumped) ||
                 @this.isWallSliding || (@this.wallJumpAvailable && !@this.justJumped)) &&
                !@this.isCrouching},
            // {Event.EventTypes.DoubleJump, @this => 
            //     @this.IsAbleToMove() && !@this.isGrounded && !@this.isWallSliding && 
            //     @this.canDoubleJump && Input.GetKeyDown(KeyCode.Space)},
            {Event.EventTypes.Interact, 
                @this => @this.IsAbleToAct()},
            // {Event.EventTypes.Crouch, 
            //     @this => @this.IsAbleToAct()},
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
            //{Event.EventTypes.Interact, @this => @this.DoInteract()},
            //{Event.EventTypes.Crouch, @this => @this.Crouch()},
            {Event.EventTypes.Grapple, @this => @this.AttemptLaunchGrapple()},
        };


    private bool IsAbleToMove()
    {
        return !isDashing && !isLineGrappling && canFunction;
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
        return !isDashing  && canFunction;
    }
    
    protected void FaceLeft()
    {
        //Debug.Log("face left");
        Transform t = transform; // more efficient, according to Rider
        Vector3 s = t.localScale;
        t.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
    }

    protected void FaceRight()
    {
        //Debug.Log("face right");
        Transform t = transform; // more efficient, according to Rider
        Vector3 s = t.localScale;
        t.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
    }

    public void Invert() {
        if (isInverted)
        {
            return;
        }
        gravityValue = -Mathf.Abs(gravityValue);
        //transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        particleChild.transform.localScale =
            new Vector3(particleChild.transform.localScale.x, -Mathf.Abs(particleChild.transform.localScale.y), 0);
        spriteRenderer.flipY = true;
        charCollider.offset = new Vector2(originalColliderOffset.x, -originalColliderOffset.y);
        isInverted = true;
    }
    
    public void DeInvert() {
        if (!isInverted)
        {
            return;
        }
        gravityValue = Mathf.Abs(gravityValue);
        particleChild.transform.localScale =
            new Vector3(particleChild.transform.localScale.x, Mathf.Abs(particleChild.transform.localScale.y), 0);
        //transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        spriteRenderer.flipY = false;
        charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y);
        isInverted = false;
    }
    
    protected void Die() 
    {
        Animator.SetTrigger(Death);
        canFunction = false;
        Rigidbody.gravityScale = 0;
        Rigidbody.velocity = Vector2.zero;
        Rigidbody.bodyType = RigidbodyType2D.Static;
        GameManager.Instance.PlayerDeath();
    }

   

  

    private void Interrupt() {
        foreach (IEnumerator co in toInterrupt)
        {
            if (co != null)
            {
                StopCoroutine(co);
            }
        }

        isCrouching = false;
        isDashing = false;

    }



    public void PrepForScene()
    {
        Interrupt();
    }

}
