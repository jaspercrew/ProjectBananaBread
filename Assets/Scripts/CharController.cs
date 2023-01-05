using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class CharController : BeatEntity
{
    public static CharController Instance;


    private Vector2 originalColliderSize;
    private Vector2 originalColliderOffset;
    
    private float runSpeed;

    public bool isDashing;
    
    
    
    public float gravityValue = BaseGravity;

    
    
    // Trackers
    private bool canFunction = true;

    public BoostZone currentBoostZone;
    

    private HashSet<IEnumerator> toInterrupt = new HashSet<IEnumerator>();
    private IEnumerator dashCoroutine;

    //private float lastDashTime;
    private float lastBoostTime;

    public bool isInverted;
    public SpawnAreaController currentArea;

    private bool isPlatformGrounded;
    public bool isMetronomeLocked;
    private bool recentlyBoosted;
    private bool _isGrounded;
    public bool isGrounded
    {
        get => _isGrounded;
        private set 
        {
            if (_isGrounded && !value)
            {
                StartCoroutine(JumpBufferCoroutine());
            }
            //Update the boolean variable
            _isGrounded = value;
        }
    }

    private bool groundedAfterBoost = true;
    
    public float recentImpulseTime;

    public List<Vector3> pointsInTime = new List<Vector3>();
    private bool isRewinding;
    public bool doRecord;

    private Rigidbody2D instantiatedProjectile;
    private Vector3 attachmentPoint;

    private bool jumpAvailable = true;
    private int facingDirection;
    public bool isGrappling;

    private bool justJumped;
    private float lastJumpTime;

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
            // if (_isWallSliding && !value)
            // {
            //     StartCoroutine(WallJumpBufferCoroutine());
            // }
            //Update the boolean variable
            _isWallSliding = value;
        }
    }
    private int wallJumpDir;

    private bool isNearWallOnRight;
    private bool isNearWallOnLeft;
    //private int wallJumpFramesLeft;
    public BeatPlatform mostRecentlyTouchedPlatform;

    private float savedRotationalVelocity;

    private int fadeSpriteIterator;
    [FormerlySerializedAs("lifetime")] public float emitFadesTime;
    
    private float moveVector;
    private float inputVector;
    private float prevInVector = 2;
    public int forcedMoveVector;
    public float forcedMoveTime;
    public bool disabledMovement;
    
    // animator values beforehand to save time later
    // protected static readonly int AnimState = Animator.StringToHash("AnimState");
    // protected static readonly int Idle = Animator.StringToHash("Idle");
    // protected static readonly int Jump = Animator.StringToHash("Jump");
    // protected static readonly int Death = Animator.StringToHash("Death");
    // protected static readonly int Grounded = Animator.StringToHash("Grounded");
    // protected static readonly int Dash = Animator.StringToHash("Dash");
    
    
    private LayerMask obstacleLayerMask;
    private LayerMask obstaclePlusLayerMask;
    private LayerMask platformLayerMask;
    private LayerMask wallSlideLayerMask;
    
    private readonly LinkedList<Event> eventQueue = new LinkedList<Event>();

    private class Event
    {
        public const float EventTimeout = 0.25f;
        public readonly EventTypes EventType;
        public readonly float TimeCreated;
        
        public enum EventTypes
        {
            Dash, Jump, Interact, 
             Crouch, Grapple, Boost
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
            {() => Input.GetKeyDown(KeyCode.LeftShift), Event.EventTypes.Boost},
            //{() => Input.GetKeyDown(KeyCode.H), Event.EventTypes.Boost},
            {() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.Jump},
            //{() => Input.GetKeyDown(KeyCode.Space), Event.EventTypes.DoubleJump},
            //{() => Input.GetKeyDown(KeyCode.E), Event.EventTypes.Interact},
            //{() => Input.GetKeyDown(KeyCode.LeftControl), Event.EventTypes.Crouch},
            {() => Input.GetKeyDown(KeyCode.Q), Event.EventTypes.Grapple},
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
            // {Event.EventTypes.Dash, @this =>
            //     (@this.IsAbleToAct()) && Time.time > @this.lastDashTime + DashCooldown &&
            //     !@this.isCrouching},
            {Event.EventTypes.Boost, @this =>
                (@this.IsAbleToAct()) && Time.time > @this.lastBoostTime + BoostCooldown && !@this.recentlyBoosted},
            {Event.EventTypes.Jump, @this => 
                @this.IsAbleToMove() && 
                (@this.isGrounded || (@this.jumpAvailable && !@this.justJumped) || @this.isNearWallOnLeft || @this.isNearWallOnRight) &&
                !@this.isCrouching && !@this.disabledMovement} ,
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
            //{Event.EventTypes.Dash, @this => @this.DoDash()},
            {Event.EventTypes.Jump, @this => @this.DoJump()},
            {Event.EventTypes.Boost, @this => @this.Boost()},
            //{Event.EventTypes.DoubleJump, @this => @this.DoDoubleJump()},
            //{Event.EventTypes.Interact, @this => @this.DoInteract()},
            //{Event.EventTypes.Crouch, @this => @this.Crouch()},
            {Event.EventTypes.Grapple, @this => @this.LaunchHook()},
        };


    private bool IsAbleToMove()
    {
        return canFunction && !isRewinding && !isMetronomeLocked && !GameManager.Instance.isMenu && !isGrappling;
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
        const float buffer = .15f;
        jumpAvailable = true;
        yield return new WaitForSeconds(buffer);
        jumpAvailable = false;
    }

    private bool IsAbleToAct() {
        return !isDashing  && !disabledMovement && canFunction && !isRewinding && !isMetronomeLocked && !GameManager.Instance.isMenu && !isGrappling;
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
        GameObject newFadeSprite = Instantiate(fadeSprite, transform.position, transform.rotation);
        newFadeSprite.GetComponent<FadeSprite>()
            .Initialize(spriteRenderer.sprite, transform.localScale.x > 0, isInverted, true);
    }

    public void Invert() {
        if (isInverted)
        {
            return;
        }
        emitFadesTime += .2f;
        Rigidbody.AddForce(Vector2.up * inversionForce, ForceMode2D.Impulse);

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
        Rigidbody.AddForce(Vector2.down * inversionForce, ForceMode2D.Impulse);

        emitFadesTime += .2f;
        gravityValue = Mathf.Abs(gravityValue);
        particleChild.transform.localScale =
            new Vector3(particleChild.transform.localScale.x, Mathf.Abs(particleChild.transform.localScale.y), 0);
        //transform.RotateAround(spriteRenderer.bounds.center, Vector3.forward, 180);
        spriteRenderer.flipY = false;
        charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y);
        isInverted = false;
    }
    
    public void Die()
    {
        if (disabledMovement) //if already in dying anim, dont do anything
        {
            return;
        }
        StartCoroutine(DieCoroutine());
    }

    private IEnumerator DieCoroutine()
    {
        CameraManager.Instance.DoTransition(true);
        //Animator.SetTrigger(Death);

        yield return new WaitForSeconds(CameraManager.Instance.totalDelayToSpawn);

        if (currentArea == null || currentArea.spawnLocation == null)
        {
            transform.position = SceneInformation.Instance.GetInitialSpawnPosition();
        }
        else
        {
            transform.position = currentArea.spawnLocation.position;
        }
        
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
