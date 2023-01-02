using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;

public partial class CharController
{
    public float recentImpulseTime;
    //public float disableInputTime;
    public GameObject fadeSprite;
    public FixedJoint2D fixedJoint2D;
    public List<Vector3> pointsInTime = new List<Vector3>();
    private bool isRewinding;
    public bool doRecord;
    private int rewindSpeed = 5;
    
    private LineRenderer grappleLineRenderer;
    private DistanceJoint2D grappleDistanceJoint;
    private Rigidbody2D rigidbody2d;
    public bool isGrappling;
    private Rigidbody2D instantiatedProjectile;
    public Rigidbody2D projectilePrefab;
    private Vector3 attachmentPoint;
    
    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        //DontDestroyOnLoad(gameObject);
        PrepForScene();
        Rigidbody = transform.GetComponent<Rigidbody2D>();
    }

    protected override void Start()
    {
        
        fadeSpriteIterator = 0;
        runSpeed = baseSpeed;

        fixedJoint2D = GetComponent<FixedJoint2D>();
        charLight = transform.Find("Light").GetComponent<Light2D>();
        particleChild = transform.Find("Particles");
        
        Animator = transform.Find("SpriteHandler").GetComponent<Animator>();
        charCollider = transform.GetComponent<BoxCollider2D>();

        dashTrail = transform.Find("Particles").Find("DashFX").GetComponent<ParticleSystem>();
        
        dust = particleChild.Find("WhiteDust").GetComponent<ParticleSystem>();
        //fadePS = particleChild.Find("FadePS").GetComponent<ParticleSystem>();
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        obstaclePlusLayerMask = LayerMask.GetMask("Obstacle", "Slide", "Platform");
        wallSlideLayerMask = LayerMask.GetMask("Slide");
        platformLayerMask = LayerMask.GetMask("Platform");
        
        
        screenShakeController = ScreenShakeController.Instance;
        // grappleController = GetComponent<RadialGrapple>();
        spriteRenderer = transform.Find("SpriteHandler").GetComponent<SpriteRenderer>();
        originalColliderSize = charCollider.size;
        originalColliderOffset = charCollider.offset;
        
        charLight.enabled = false;
        
        grappleLineRenderer = GetComponent<LineRenderer>();
        grappleDistanceJoint = GetComponent<DistanceJoint2D>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        // grappleLineRenderer.enabled = false;
        // grappleDistanceJoint.enabled = false;

        // set char's spawn
        //Debug.Log(SceneInformation.Instance.GetInitialSpawnPosition());
        transform.position = SceneInformation.Instance.GetInitialSpawnPosition();
        base.Start();
    }

    private void FixedUpdate() {

        Animstate_FixedUpdate();
        inputVector = Input.GetAxisRaw("Horizontal");
        FadeParticle_FixedUpdate();
        // AdjustCape_FixedUpdate();
        if (disabledMovement)
        {
            moveVector = 0;
        }
        if (IsAbleToMove())
        {
            StandardMovement_FixedUpdate();
            ApplyForcedMovement_FixedUpdate();
            TurnAround_FixedUpdate();
        }
        // movement animations
        //gravityValue = isInverted ? -Mathf.Abs(gravityValue) : Mathf.Abs(gravityValue);

        Record_FixedUpdate();
    }

    private void Record_FixedUpdate()
    {
        if (!doRecord || isRewinding)
        {
            return;
        }
        if (pointsInTime.Count == 0)
        {
            pointsInTime.Insert(0, transform.position);
            return;
        }
        Vector3 last = pointsInTime[0];
        if (last != transform.position)
        {
            pointsInTime.Insert(0, transform.position);
        }
    }

    public IEnumerator StartRewind()
    {
        int savedPriority = CameraManager.Instance.currentCam.Priority;
        CameraManager.Instance.currentCam.Priority = 50;
        doRecord = false;
        Rigidbody.velocity = Vector3.zero; //diving fix (LOL RERUN XD)
        charCollider.isTrigger = true;
        isRewinding = true;
        Rigidbody.bodyType = RigidbodyType2D.Static;
        emitFadesTime += .45f;
        //while (pointsInTime.Count > 0) {
        for (int i = 0; i < pointsInTime.Count; i += rewindSpeed){
            Vector3 pointInTime = pointsInTime[i];
            transform.position = pointInTime;
            yield return new WaitForFixedUpdate();
        }
        pointsInTime.Clear();
        isRewinding = false;
        Rigidbody.bodyType = RigidbodyType2D.Dynamic;
        charCollider.isTrigger = false;
        CameraManager.Instance.currentCam.Priority = savedPriority;
    }

    private void Animstate_FixedUpdate()
    {
        if (isWallSliding)
        {
            //Debug.Log("wallslide anim");
            Animator.SetInteger(AnimState, 3);
        }
        else if (isGrounded && (Mathf.Abs(moveVector) > float.Epsilon))
        {
            //Debug.Log("run anim");
            Animator.SetInteger(AnimState, 2);
        }
        else
        {
            //Debug.Log("idle anim");
            Animator.SetInteger(AnimState, 1);
        }
        //print(Animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
    }
    

    private void ApplyForcedMovement_FixedUpdate()
    {
        moveVector = inputVector;
        if (forcedMoveTime > 0)
        {
            moveVector = forcedMoveVector;
            forcedMoveTime -= Time.fixedDeltaTime;
        }
        // else if (disableInputTime > 0)
        // {
        //     moveVector = 0;
        //     disableInputTime -= Time.fixedDeltaTime;
        // }
    }

    private void StandardMovement_FixedUpdate()
    {
        Vector2 v = Rigidbody.velocity;
        float xVel = v.x;
        float yVel = v.y;

        // regular ground movement
        if (isGrounded)
        {
            
            int moveDir = Math.Sign(moveVector);
            // if user is not moving and has runSpeed, then slow down
            if (moveDir == 0 && Mathf.Abs(xVel) >= MinGroundSpeed && !RecentlyImpulsed())
            {
                int antiMoveDir = -Math.Sign(xVel);

                // TODO change this if we choose to add ice or something
                Rigidbody.AddForce(antiMoveDir * OnGroundDeceleration * Vector2.right, ForceMode2D.Force);
            }
            // otherwise move the player in the direction (only accelerate if below max runSpeed)
            else if ((moveDir > 0 && xVel < runSpeed) || (moveDir < 0 && xVel > -runSpeed))
            {
                Rigidbody.AddForce(moveDir * OnGroundAcceleration * Vector2.right, ForceMode2D.Force);
            }
            

            // decelerate if above limit
            if (!RecentlyImpulsed())
            {
                if (Mathf.Abs(xVel) > boostedSpeed)
                {
                    float xAfterDrag = Rigidbody.velocity.x - (Math.Sign(xVel) * OnGroundDrag * 1.5f);
                    //
                    if (xVel > 0)
                    {
                        Rigidbody.velocity = new Vector2(Math.Max(xAfterDrag, boostedSpeed), Rigidbody.velocity.y);
                    }
                    else if (xVel < 0)
                    {
                        Rigidbody.velocity = new Vector2(Math.Min(xAfterDrag, -boostedSpeed), Rigidbody.velocity.y);
                    }
                }

                else if (Mathf.Abs(xVel) > runSpeed)
                {
                    float xAfterDrag = Rigidbody.velocity.x - (Math.Sign(xVel) * OnGroundDrag);
                    //
                    if (xVel > 0)
                    {
                        Rigidbody.velocity = new Vector2(Math.Max(xAfterDrag, runSpeed), Rigidbody.velocity.y);
                    }
                    else if (xVel < 0)
                    {
                        Rigidbody.velocity = new Vector2(Math.Min(xAfterDrag, -runSpeed), Rigidbody.velocity.y);
                    }
                }
                // Rigidbody.velocity = new Vector2(
                //     Mathf.Clamp(xVel, -runSpeed, runSpeed),
                //     Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));
            }
            

            // apply min velocity
            if (Mathf.Abs(xVel) < MinGroundSpeed)
            {
                Rigidbody.velocity = new Vector2(0, yVel);
            }
            //recentlyImpulsed = false;
        }
        // in-air movement
        else
        {
            if (!(Math.Sign(moveVector) == Math.Sign(xVel) && Math.Abs(xVel) > runSpeed))
            {
            Rigidbody.AddForce(Math.Sign(moveVector) * InAirAcceleration * Vector2.right, ForceMode2D.Force);
            }

            // slow down if player is not inputting horizontal movement
            // and don't apply if grappling
            if (moveVector == 0)
            {
                // apply horizontal "drag" based on current x velocity
                Rigidbody.AddForce(-xVel * InAirDrag * Vector2.right, ForceMode2D.Force);
            }
            else if (Math.Sign(moveVector) == Math.Sign(xVel)) //ONLY APPLY PARTIAL DRAG IF PLAYER IS MATCHING AIR BOOST
            {
                Rigidbody.AddForce(-xVel * (InAirDrag / 2) * Vector2.right, ForceMode2D.Force);
            }
            
            
            // if (recentlyImpulsed && Math.Abs(xVel) <= runSpeed)
            // {
            //     recentlyImpulsed = false;
            // }

            // apply max velocity if not grappling
            // if (!isLineGrappling && !RecentlyImpulsed())
            // {
            //     Rigidbody.velocity = new Vector2(
            //         Mathf.Clamp(xVel, -runSpeed, runSpeed),
            //         Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));
            // }
        }
    }

    

    private void TurnAround_FixedUpdate() {
        if (disabledMovement)
        {
            return;
        }
        // feet dust logic
        if (Math.Abs(prevInVector - inputVector) > 0.01f && isGrounded && inputVector != 0) {
            dust.Play();
            //parentWindFX.transform.localScale = new Vector3(-parentWindFX.transform.localScale.x, 1, 1);
        }
        
        prevInVector = inputVector;

        Vector3 scale = transform.localScale;
        // direction switching
        // if (isWallSliding || (wallJumpAvailable && !justJumped))
        // {
        //     return;
        // }

        if (isWallSliding)
        {
            return;
        }
        if (moveVector > 0 && Math.Abs(scale.x + 1) > float.Epsilon) {
            FaceRight();
        }
        else if (moveVector < 0 && Math.Abs(scale.x - 1) > float.Epsilon) {
            FaceLeft();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.Instance.isPaused)
            {
                GameManager.Instance.Unpause();  
            }
            else
            {
                GameManager.Instance.Pause();  
            }
            
        }
            
        // if (Input.GetKeyDown(KeyCode.LeftBracket))
        //     SaveData.SaveToFile(1);
        // if (Input.GetKeyDown(KeyCode.RightBracket))
        //     SaveData.LoadFromFile(1);
        

        

        Vector2 v = Rigidbody.velocity;
        Rigidbody.velocity = new Vector2(v.x, v.y - ((isWallSliding ? (v.y < 0f ? .8f : .3f) : 1) * gravityValue * Time.deltaTime));
        CheckGrounded_Update();
        CheckPlatformGrounded_Update();
        EventHandling_Update();
        ShortJumpDetection_Update();
        WallSlideDetection_Update();
        //LineGrappleUpdate();
        Crouching_Update();
        LookAhead_Update();
        Grapple_Update();
        recentImpulseTime -= Time.deltaTime;

    }

    private void Grapple_Update()
    {
 
        if (Input.GetKeyUp(KeyCode.Q)) {
            //TODO kill projectilePrefab as well
            
            if (instantiatedProjectile != null) { 
                Destroy(instantiatedProjectile.gameObject);
            }

            EndGrapple();
        }
        
        if (instantiatedProjectile != null) //isLaunched
        {
            grappleLineRenderer.SetPosition(1, transform.position);
            grappleLineRenderer.SetPosition(0, instantiatedProjectile.transform.position);
        }
        
        if (isGrappling) {
            //const float offsetMultiplier = 1f;
            //float offset = Mathf.Cos(Vector3.Angle(transform.position - attachmentPoint, Vector3.down)) * offsetMultiplier;
            //Debug.Log(Vector3.Angle(transform.position - attachmentPoint, Vector3.up));
            //Debug.Log(offset);
            //Rigidbody.velocity *= 1.001f;
            float grappleLength = (attachmentPoint - transform.position).magnitude;
            grappleDistanceJoint.distance = grappleLength;
            if (Rigidbody.velocity.y < 0 && Rigidbody.velocity.magnitude < grappleSpeedLimit)
            {
                Rigidbody.velocity *= 1.01f;
            }
            //grappleLineRenderer.SetPosition(0, instantiatedProjectile.transform.position);
        }
    }
    
    private void LaunchHook()
    {
        Vector2 direction = new Vector2(1, 1.4f);
        if (transform.localScale.x > 0.5) {
            direction.x = -direction.x;
        }

        if (isInverted) {
            direction.y = -direction.y;
        }
        
        print("hook launched");
        const float speed = 30f;
        Vector3 offset = isInverted ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        instantiatedProjectile = Instantiate(projectilePrefab, transform.position + offset, transform.rotation);
        instantiatedProjectile.gameObject.GetComponent<GrappleProjectile>().Initialize(direction.normalized, speed);
        grappleLineRenderer.enabled = true;
        
    }

    public void StartGrapple(Vector3 grapplePoint)
    {

        gravityValue = BaseGravity * .5f;
        attachmentPoint = grapplePoint;
        const float verticalDisplacementOffset = .5f;
        //Vector3 diffNormalized = (grapplePoint - transform.position).normalized ;
        transform.position += new Vector3(0, verticalDisplacementOffset, 0);
        ReduceHeight(true);
        
        
        isGrappling = true;
        //charController.isRecentlyGrappled = true;
        //grappleLineRenderer.SetPosition(0, grapplePoint);
        //grappleLineRenderer.SetPosition(1, transform.position);
        grappleDistanceJoint.connectedAnchor = grapplePoint;
        
        grappleDistanceJoint.enabled = true;
        grappleLineRenderer.enabled = true;
        

        //const float boostForce = 5f;
        const float gravModifier = .8f;
        const float minVel = 15f;

        if (IsFacingLeft()) {
            //facing left
            rigidbody2d.velocity += (new Vector2(rigidbody2d.velocity.y, 0) * gravModifier);
            if (rigidbody2d.velocity.x > -minVel) {
                rigidbody2d.velocity = (Vector2.left * minVel);
                //Debug.Log("left boost");
            }
        }
        else {
            rigidbody2d.velocity -= (new Vector2(rigidbody2d.velocity.y, 0) * gravModifier);
            if (rigidbody2d.velocity.x < minVel) {
                rigidbody2d.velocity = (Vector2.right * minVel);
                //Debug.Log("right boost");
            }
        }
    }

    public void EndGrapple() {
        ReturnHeight();
        grappleDistanceJoint.enabled = false;
        grappleLineRenderer.enabled = false;
        isGrappling = false;
        gravityValue = BaseGravity;
    }

    private void LookAhead_Update()
    {
        if (CameraManager.Instance.currentCam is null)
        {
            return;
        }
        CinemachineFramingTransposer transposer =
            CameraManager.Instance.currentCam.GetCinemachineComponent<CinemachineFramingTransposer>();
        
        if (Input.GetKeyDown(KeyCode.F) && CameraManager.Instance.currentCam.gameObject.CompareTag("DynamicCamera") && !(transposer is null))
        {
            transposer
                .m_TrackedObjectOffset.x = CameraManager.Instance.currentCam.m_Lens.OrthographicSize * -1.8f * transform.localScale.x;
        }

        if (!(transposer is null) && (Input.GetKeyUp(KeyCode.F) || !CameraManager.Instance.currentCam.gameObject.CompareTag("DynamicCamera")))
        {
            transposer.m_TrackedObjectOffset.x = 0;
        }
    }
    
    

    private void Crouching_Update()
    {
        if (isCrouching && CheckSpace())
        {
            UnCrouch();
        }
    }
    



    // private void LineGrappleUpdate()
    // {
    //
    //     if (!isLineGrappling && !isGrappleLaunched && GrapplePoint.TargetPoint != null)
    //     {
    //         Vector3 targetPosition = GrapplePoint.TargetPoint.transform.position;
    //         Vector3 direction = (targetPosition - transform.position).normalized;
    //         Vector3 offset = Vector3.up * .5f;
    //         
    //         RaycastHit2D hit = Physics2D.Raycast(transform.position +  offset, direction,
    //             Vector2.Distance(transform.position, targetPosition), obstacleLayerMask);
    //         
    //         if (hit.collider != null)
    //         {
    //             GrapplePoint.TargetPoint.Blocked();
    //             grappleClearRenderer.enabled = false;
    //             grappleBlocked = true;
    //             grappleLOSRenderer.enabled = true;
    //             grappleLOSRenderer.SetPosition(1, transform.position + offset);
    //             grappleLOSRenderer.SetPosition(0, GrapplePoint.TargetPoint.transform.position);
    //         }
    //         else
    //         {
    //             GrapplePoint.TargetPoint.Cleared();
    //             grappleBlocked = false;
    //             grappleLOSRenderer.enabled = false;
    //             grappleClearRenderer.enabled = true;
    //             grappleClearRenderer.SetPosition(1, transform.position + offset);
    //             grappleClearRenderer.SetPosition(0, GrapplePoint.TargetPoint.transform.position);
    //         }
    //     }
    //     else
    //     {
    //         grappleBlocked = false;
    //         //Debug.Log("red off");
    //         grappleLOSRenderer.enabled = false;
    //         grappleClearRenderer.enabled = false;
    //     }
    //     
    //     if (isGrappleLaunched && sentProjectile != null)
    //     {
    //         grappleLineRenderer.SetPosition(1, transform.position);
    //         grappleLineRenderer.SetPosition(0, sentProjectile.transform.position);
    //     }
    //     const float grappleSpeed = 15f;
    //     const float disconnectDistance = .3f;
    //     if (isLineGrappling)
    //     {
    //         Vector3 direction = (launchedPoint.transform.position - transform.position).normalized;
    //         Rigidbody.velocity = direction * grappleSpeed;
    //         grappleLineRenderer.SetPosition(1, transform.position);
    //         grappleLineRenderer.SetPosition(0, launchedPoint.transform.position);
    //         
    //
    //         
    //         if (isLineGrappling && 
    //             Vector2.Distance(transform.position, launchedPoint.transform.position) < disconnectDistance)
    //         {
    //             DisconnectGrapple();
    //         }
    //     }
    // }

    // private void GetBounds_Update()
    // {
    //     Vector2 relativeDown = isInverted ? Vector2.up : Vector2.down;
    //
    //     Vector3 bounds = charCollider.bounds.extents;
    //     float halfWidth = Mathf.Abs(bounds.x);
    //     float halfHeight = Mathf.Abs(bounds.y);
    //     Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
    //
    //     Vector2 bottomMiddle = center + halfHeight * relativeDown;
    //     Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
    //     Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
    // }


    private void CheckGrounded_Update()
    {
        const float groundDistance = 0.05f;
        Vector2 relativeDown = isInverted ? Vector2.up : Vector2.down;

        Vector3 bounds = charCollider.bounds.extents;
        float halfWidth = Mathf.Abs(bounds.x) - groundDistance;
        float halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;

        Vector2 bottomMiddle = center + halfHeight * relativeDown;
        Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
        Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
        Vector2 aLittleDown = 3 * groundDistance * relativeDown;
        
        Debug.DrawLine(bottomLeft, bottomLeft + aLittleDown, Color.magenta);
        Debug.DrawLine(bottomRight, bottomRight + aLittleDown, Color.magenta);

        RaycastHit2D hit1 = 
            Physics2D.Linecast(bottomLeft, bottomLeft + aLittleDown, obstaclePlusLayerMask);
        RaycastHit2D hit2 = 
            Physics2D.Linecast(bottomRight, bottomRight + aLittleDown, obstaclePlusLayerMask);

        bool newlyGrounded = (hit1 || hit2) && !isWallSliding;
        if (!isGrounded && newlyGrounded){
            OnLanding();
        }

        isGrounded = newlyGrounded;
        if (newlyGrounded && !isWallSliding)
        {
            recentlyBoosted = false;
        }
        Animator.SetBool(Grounded, isGrounded);
    }
    
    private void CheckPlatformGrounded_Update()
    {
        const float groundDistance = 0.05f;
        Vector2 relativeDown = isInverted ? Vector2.up : Vector2.down;

        Vector3 bounds = charCollider.bounds.extents;
        float halfWidth = Mathf.Abs(bounds.x) - groundDistance;
        float halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
        
        Vector2 bottomMiddle = center + halfHeight * relativeDown;
        Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
        Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
        Vector2 aLittleDown = 3 * groundDistance * relativeDown;
        
        Debug.DrawLine(bottomLeft, bottomLeft + aLittleDown, Color.magenta);
        Debug.DrawLine(bottomRight, bottomRight + aLittleDown, Color.magenta);

        RaycastHit2D hit1 = 
            Physics2D.Linecast(bottomLeft, bottomLeft + aLittleDown, platformLayerMask);
        RaycastHit2D hit2 = 
            Physics2D.Linecast(bottomRight, bottomRight + aLittleDown, platformLayerMask);

        isPlatformGrounded = hit1 || hit2;
    }

    private void EventHandling_Update() {
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
            }
            // execute events whose conditions are met
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
    }

   
    private void ShortJumpDetection_Update() {
        if (Input.GetButtonUp("Jump") && !isGrounded && 
            ((!isInverted && Rigidbody.velocity.y > 0) || (isInverted && Rigidbody.velocity.y < 0))) {
            //print("short");
            
            Rigidbody.velocity = Vector2.Scale(Rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }
    // private void JumpCooldown_Update()
    // {
    //     //Debug.Log(justJumped);
    //     if (Input.GetKeyUp(KeyCode.Space) && justJumped)
    //     {
    //         //canDoubleJump = true;
    //         justJumped = false;
    //     }
    // }

    private void FadeParticle_FixedUpdate()
    {
        if (emitFadesTime > 0)
        {
            const int fadeSpriteLimiter = 5;
            fadeSpriteIterator += 1;
            emitFadesTime -= Time.deltaTime;
            if (fadeSpriteIterator == fadeSpriteLimiter)
            {
                fadeSpriteIterator = 0;
                GameObject newFadeSprite = Instantiate(fadeSprite, transform.position, transform.rotation);
                newFadeSprite.GetComponent<FadeSprite>()
                    .Initialize(spriteRenderer.sprite, transform.localScale.x > 0, isInverted);
            }
        }
    }




    private void WallSlideDetection_Update() {
        const float wallSlideSpeed = 0.75f;
        const float groundDistance = 0.05f;
        
        Vector2 v = Rigidbody.velocity;

        Vector3 bounds = charCollider.bounds.extents;
        float halfWidth = Mathf.Abs(bounds.x) - groundDistance;
        float halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
        
        // left points
        Vector2 middleLeft = center + halfWidth * Vector2.left;
        Vector2 topLeft = middleLeft + halfHeight * Vector2.up;
        Vector2 bottomLeft = middleLeft + halfHeight * Vector2.down;
        Vector2 aLittleLeft = 2 * groundDistance * Vector2.left;
        
        // right points
        Vector2 middleRight = center + halfWidth * Vector2.right;
        Vector2 topRight = middleRight + halfHeight * Vector2.up;
        Vector2 bottomRight = middleRight + halfHeight * Vector2.down;
        Vector2 aLittleRight = 2 * groundDistance * Vector2.right;

        // left linecasts
        RaycastHit2D bottomLeftHit = 
            Physics2D.Linecast(bottomLeft, bottomLeft + aLittleLeft, obstacleLayerMask);
        RaycastHit2D topLeftHit = 
            Physics2D.Linecast(topLeft, topLeft + aLittleLeft, obstacleLayerMask);
        bool isNearWallOnLeft = bottomLeftHit && topLeftHit && 
                                bottomLeftHit.transform.GetComponent<BeatPlatform>() != null && 
                                bottomLeftHit.transform.GetComponent<BeatPlatform>().isWallSlideable && 
                                topLeftHit.transform.GetComponent<BeatPlatform>() != null && 
                                topLeftHit.transform.GetComponent<BeatPlatform>().isWallSlideable;
        
        

        // right linecasts
        RaycastHit2D bottomRightHit = 
            Physics2D.Linecast(bottomRight, bottomRight + aLittleRight, obstacleLayerMask);
        RaycastHit2D topRightHit = 
            Physics2D.Linecast(topRight, topRight + aLittleRight, obstacleLayerMask);
        bool isNearWallOnRight = bottomRightHit && topRightHit && 
                                 bottomRightHit.transform.GetComponent<BeatPlatform>() != null && 
                                 bottomRightHit.transform.GetComponent<BeatPlatform>().isWallSlideable && 
                                 topRightHit.transform.GetComponent<BeatPlatform>() != null && 
                                 topRightHit.transform.GetComponent<BeatPlatform>().isWallSlideable;
        
        //print(bottomRightHit.transform.gameObject);

        // isWallSliding = v.y <= 0 && ((moveVector > 0 && isNearWallOnRight) 
        //                              || (moveVector < 0 && isNearWallOnLeft)) && IsAbleToMove();
        // isWallSliding = (isInverted ? -v.y : v.y) <= 0 && 
        //                 ((isNearWallOnRight && moveVector >= 0)|| (isNearWallOnLeft && moveVector <= 0)) &&
        //                 IsAbleToMove();
        isWallSliding = //(isInverted ? -v.y : v.y) <= 0 && 
                        ((isNearWallOnRight && transform.localScale.x < 0) || 
                         (isNearWallOnLeft && transform.localScale.x > 0)) && IsAbleToMove() && !isGrounded;
        // print("tr" + (bool)topRightHit + "br:" + (bool)bottomRightHit);
        Debug.DrawLine(bottomRight, bottomRight + aLittleRight);
        Debug.DrawLine(topRight, topRight + aLittleRight);
        Debug.DrawLine(bottomLeft, bottomLeft + aLittleLeft);
        Debug.DrawLine(topLeft, topLeft + aLittleLeft);

        if (isNearWallOnLeft)
        {
            wallJumpDir = +1;

        }
        else if (isNearWallOnRight)
        {
            wallJumpDir = -1;
            //canDoubleJump = false;
        }


        if (isWallSliding)
        {
            justJumped = false;
            // Rigidbody.velocity = new Vector2(v.x,
            //     isInverted ? Mathf.Max(-v.y, wallSlideSpeed) : Mathf.Max(v.y, -wallSlideSpeed));
        }

    }
    
}
