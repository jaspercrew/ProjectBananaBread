using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public partial class CharController : BeatEntity
{
    public static CharController instance;

    // maps from a boolean function to an event, where the function, when called, returns whether
    // the event's respective button is being pressed, and thus whether the event be queued
    private static readonly Dictionary<Func<bool>, Event.EventTypes> KeyToEventType =
        new Dictionary<Func<bool>, Event.EventTypes>
        {
            {() => Input.GetKeyDown(KeyCode.LeftShift), Event.EventTypes.Boost},
            //{() => Input.GetKeyDown(KeyCode.H), Event.EventTypes.Boost},
            {() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.Jump},
            //{() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.DoubleJump},
            //{() => Input.GetKeyDown(KeyCode.E), Event.EventTypes.Interact},
            //{() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch},
            {() => Input.GetKeyDown(KeyCode.Q), Event.EventTypes.Grapple}
        };

    // maps from event type to a boolean function that says whether the conditions for the
    // event to happen are met, and thus whether it should happen
    //
    // for some reason, the key-value pairs are static contexts, so you can't use variables or call
    // methods of CharController, so we have to explicitly pass an instance of a CharController
    // (i.e. we later explicitly pass in a "this"). we can access private variables just fine since
    // we're inside the class definition
    private static readonly Dictionary<
        Event.EventTypes,
        Func<CharController, bool>
    > EventConditions = new Dictionary<Event.EventTypes, Func<CharController, bool>>
    {
        // {Event.EventTypes.Dash, @this =>
        //     (@this.IsAbleToAct()) && Time.time > @this.lastDashTime + DashCooldown &&
        //     !@this.isCrouching},
        {Event.EventTypes.Boost, @this => @this.IsAbleToAct() && !@this.recentlyBoosted},
        {
            Event.EventTypes.Jump,
            @this =>
                @this.IsAbleToMove()
                && (
                    @this.IsGrounded
                    || (@this.jumpAvailable && !@this.justJumped)
                    || @this.isNearWallOnLeft
                    || @this.isNearWallOnRight
                    || @this.doubleJumpAvailable
                )
                && !@this.isCrouching
                && !@this.disabledMovement
        },
        // {Event.EventTypes.DoubleJump, @this =>
        //     @this.IsAbleToMove() && !@this.isGrounded && !@this.isWallSliding &&
        //     @this.canDoubleJump && Input.GetKeyDown(KeyCode.Space)},
        {Event.EventTypes.Interact, @this => @this.IsAbleToAct()},
        // {Event.EventTypes.Crouch,
        //     @this => @this.IsAbleToAct()},
        {Event.EventTypes.Grapple, @this => @this.IsAbleToAct()}
    };

    // maps from event type to a void function (action) that actually executes the action
    // associated with that event type
    private static readonly Dictionary<Event.EventTypes, Action<CharController>> EventActions =
        new Dictionary<Event.EventTypes, Action<CharController>>
        {
            //{Event.EventTypes.Dash, @this => @this.DoDash()},
            {Event.EventTypes.Jump, @this => @this.DoJump()},
            {Event.EventTypes.Boost, @this => @this.Boost()},
            //{Event.EventTypes.DoubleJump, @this => @this.DoDoubleJump()},
            //{Event.EventTypes.Interact, @this => @this.DoInteract()},
            //{Event.EventTypes.Crouch, @this => @this.Crouch()},
            {Event.EventTypes.Grapple, @this => @this.LaunchHook()}
        };

    public bool isDashing;

    public float gravityValue = baseGravity;

    public BoostZone currentBoostZone;

    public bool isInverted;
    public SpawnAreaController currentArea;
    public bool isMetronomeLocked;
    public float recentImpulseTime;

    public List<Vector3> pointsInTime = new List<Vector3>();
    public bool doRecord;
    public bool isGrappling;
    public bool leftGrapple;

    public bool isCrouching;

    //private int wallJumpFramesLeft;
    public BeatPlatform mostRecentlyTouchedPlatform;

    [FormerlySerializedAs("lifetime")] public float emitFadesTime;

    public float dashTrailEmitTime;
    public int forcedMoveVector;
    public float forcedMoveTime;
    public bool disabledMovement;

    // Trackers
    private readonly bool canFunction = true;

    private readonly LinkedList<Event> eventQueue = new LinkedList<Event>();

    private readonly HashSet<IEnumerator> toInterrupt = new HashSet<IEnumerator>();
    private bool _isGrounded;

    // ReSharper disable once InconsistentNaming
    private Vector3 attachmentPoint;

    private float currentSpeedBar;
    private IEnumerator dashCoroutine;

    private bool doubleJumpAvailable;
    private int facingDirection;

    private int fadeSpriteIterator;

    private bool groundedAfterBoost = true;
    private float inputVector;

    private Rigidbody2D instantiatedProjectile;
    private bool isNearWallOnLeft;

    private bool isNearWallOnRight;

    private bool isPlatformGrounded;
    private bool isRewinding;

    private bool jumpAvailable = true;

    private bool justJumped;

    //private float lastDashTime;
    private float lastBoostTime;
    private float lastJumpTime;
    private float maxSpeedBar;

    private float moveVector;

    // animator values beforehand to save time later
    // protected static readonly int AnimState = Animator.StringToHash("AnimState");
    // protected static readonly int Idle = Animator.StringToHash("Idle");
    // protected static readonly int Jump = Animator.StringToHash("Jump");
    // protected static readonly int Death = Animator.StringToHash("Death");
    // protected static readonly int Grounded = Animator.StringToHash("Grounded");
    // protected static readonly int Dash = Animator.StringToHash("Dash");


    private LayerMask obstacleLayerMask;
    private LayerMask obstaclePlusLayerMask;
    private Color originalBoostVisualColor;
    private Vector2 originalColliderOffset;

    private Vector2 originalColliderSize;
    private LayerMask platformLayerMask;
    private float prevInVector = 2;
    private bool recentlyBoosted;

    private float runSpeed;

    private float savedRotationalVelocity;

    //private bool canDoubleJump;


    private bool wallJumpAvailable;
    private int wallJumpDir;
    private LayerMask wallSlideLayerMask;

    public bool IsGrounded
    {
        get => _isGrounded;
        private set
        {
            if (_isGrounded && !value)
                StartCoroutine(JumpBufferCoroutine());
            //Update the boolean variable
            _isGrounded = value;
        }
    }

    private bool IsWallSliding { get; set; }

    private bool IsAbleToMove()
    {
        return canFunction
               && !isRewinding
               && !isMetronomeLocked
               && !GameManager.instance.isMenu
               && !isGrappling;
    }

    private bool RecentlyImpulsed()
    {
        return recentImpulseTime > 0;
    }

    public bool IsFacingLeft()
    {
        return facingDirection == -1;
    }

    // private IEnumerator WallJumpBufferCoroutine()
    // {
    //
    //     print("walljump buffer");
    //     const float buffer = .25f;
    //     wallJumpAvailable = true;
    //     yield return new WaitForSeconds(buffer);
    //     wallJumpAvailable = false;
    // }

    private IEnumerator JumpBufferCoroutine()
    {
        const float buffer = .20f;
        jumpAvailable = true;
        yield return new WaitForSeconds(buffer);
        jumpAvailable = false;
    }

    private bool IsAbleToAct()
    {
        return !isDashing
               && !disabledMovement
               && canFunction
               && !isRewinding
               && !isMetronomeLocked
               && !GameManager.instance.isMenu
               && !isGrappling;
    }

    protected void FaceLeft()
    {
        //Debug.Log("face left");
        // Transform t = transform; // more efficient, according to Rider
        // Vector3 s = t.localScale;
        // t.localScale = new Vector3(Mathf.Abs(s.x), s.y, s.z);
        facingDirection = -1;
    }

    protected void FaceRight()
    {
        //Debug.Log("face right");
        // Transform t = transform; // more efficient, according to Rider
        // Vector3 s = t.localScale;
        // t.localScale = new Vector3(-Mathf.Abs(s.x), s.y, s.z);
        facingDirection = 1;
    }

    public void SpawnExtendedFadeSprite()
    {
        var newFadeSprite = Instantiate(fadeSprite, transform.position, transform.rotation);
        newFadeSprite
            .GetComponent<FadeSprite>()
            .Initialize(spriteRenderer.sprite, transform.localScale.x > 0, isInverted, true);
    }

    public void Invert()
    {
        if (isInverted)
            return;
        emitFadesTime += .2f;
        rigidbody.AddForce(Vector2.up * InversionForce, ForceMode2D.Impulse);

        gravityValue = -Mathf.Abs(gravityValue);
        //transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        particleChild.transform.localScale = new Vector3(
            particleChild.transform.localScale.x,
            -Mathf.Abs(particleChild.transform.localScale.y),
            0
        );
        spriteRenderer.flipY = true;
        charCollider.offset = new Vector2(originalColliderOffset.x, -originalColliderOffset.y);
        isInverted = true;
    }

    public void DeInvert()
    {
        if (!isInverted)
            return;
        rigidbody.AddForce(Vector2.down * InversionForce, ForceMode2D.Impulse);

        emitFadesTime += .2f;
        gravityValue = Mathf.Abs(gravityValue);
        particleChild.transform.localScale = new Vector3(
            particleChild.transform.localScale.x,
            Mathf.Abs(particleChild.transform.localScale.y),
            0
        );
        //transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        spriteRenderer.flipY = false;
        charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y);
        isInverted = false;
    }

    public void Die()
    {
        if (disabledMovement) //if already in dying anim, dont do anything
            return;
        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        CameraManager.instance.DoTransition(true);
        //Animator.SetTrigger(Death);

        yield return new WaitForSeconds(CameraManager.instance.totalDelayToSpawn);

        if (currentArea == null || currentArea.spawnLocation == null)
            transform.position = SceneInformation.instance.GetInitialSpawnPosition();
        else
            transform.position = currentArea.spawnLocation.position;

        GameManager.instance.PlayerDeath();
    }

    private void Interrupt()
    {
        foreach (var co in toInterrupt)
            if (co != null)
                StopCoroutine(co);

        isCrouching = false;
        isDashing = false;
    }

    public void PrepForScene()
    {
        Interrupt();
    }

    private class Event
    {
        public enum EventTypes
        {
            Dash,
            Jump,
            Interact,
            Crouch,
            Grapple,
            Boost
        }

        public const float EventTimeout = 0.25f;
        public readonly EventTypes eventType;
        public readonly float timeCreated;

        public Event(EventTypes type, float time)
        {
            eventType = type;
            timeCreated = time;
        }
    }
}