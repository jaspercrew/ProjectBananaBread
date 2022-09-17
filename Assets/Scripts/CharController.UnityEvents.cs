using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;

public partial class CharController
{
    public bool recentlyImpulsed;
    public GameObject fadeSprite;
    public List<Vector3> pointsInTime = new List<Vector3>();
    private bool isRewinding;
    public bool doRecord;
    private int rewindSpeed = 5;
    
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
    }

    protected override void Start()
    {

        //SaveData.LoadFromFile(1);
        //Interactor.interactors.Clear();
        //canDoubleJump = false;
        fadeSpriteIterator = 0;
        //capeAnchor = transform.Find("Cape").Find("CapeAnchor").GetComponent<CapeController>();
        //capeOutlineAnchor = transform.Find("Cape").Find("CapeAnchor1").GetComponent<CapeController>();
        speed = baseSpeed;
        grappleLineRenderer = transform.GetComponent<LineRenderer>();
        grappleLineRenderer.enabled = false;
        grappleLOSRenderer = transform.Find("GrappleLOS").GetComponent<LineRenderer>();
        grappleLOSRenderer.enabled = false;
        grappleClearRenderer = transform.Find("GrappleClear").GetComponent<LineRenderer>();
        grappleClearRenderer.enabled = false;
        groundCheck = transform.Find("GroundCheck").GetComponent<BoxCollider2D>();

        charLight = transform.Find("Light").GetComponent<Light2D>();
        particleChild = transform.Find("Particles");
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.Find("SpriteHandler").GetComponent<Animator>();
        charCollider = transform.GetComponent<BoxCollider2D>();
        
        dust = particleChild.Find("DustPS").GetComponent<ParticleSystem>();
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

        // set char's spawn
        //Debug.Log(SceneInformation.Instance.GetSpawnPos());
        transform.position = SceneInformation.Instance.GetSpawnPos();
        base.Start();
    }

    private void FixedUpdate() {

        Animstate_FixedUpdate();
        inputVector = Input.GetAxisRaw("Horizontal");
        FadeParticle_FixedUpdate();
        // AdjustCape_FixedUpdate();
        if (!IsAbleToMove()) return;
        // movement animations
        
        //gravityValue = isInverted ? -Mathf.Abs(gravityValue) : Mathf.Abs(gravityValue);
        StandardMovement_FixedUpdate();
        
        ApplyForcedMovement_FixedUpdate();

        TurnAround_FixedUpdate();
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
    }
    

    private void ApplyForcedMovement_FixedUpdate()
    {
        moveVector = inputVector;
        if (forcedMoveTime > 0)
        {
            moveVector = forcedMoveVector;
            forcedMoveTime -= Time.fixedDeltaTime;
        }
    }

    private void StandardMovement_FixedUpdate()
    {
        Vector2 v = Rigidbody.velocity;
        float xVel = v.x;
        float yVel = v.y;

        // regular ground movement
        if (isGrounded)
        {
            recentlyImpulsed = false;
            int moveDir = Math.Sign(moveVector);
            // if user is not moving and has speed, then slow down
            if (moveDir == 0 && Mathf.Abs(xVel) >= MinGroundSpeed)
            {
                int antiMoveDir = -Math.Sign(xVel);

                // TODO change this if we choose to add ice or something
                Rigidbody.AddForce(antiMoveDir * OnGroundDeceleration * Vector2.right, ForceMode2D.Force);
            }
            // otherwise move the player in the direction 
            {
                Rigidbody.AddForce(moveDir * OnGroundAcceleration * Vector2.right, ForceMode2D.Force);
            }
            

            // apply max velocity
            Rigidbody.velocity = new Vector2(
                Mathf.Clamp(xVel, -speed, speed),
                Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));

            // apply min velocity
            if (Mathf.Abs(xVel) < MinGroundSpeed)
            {
                Rigidbody.velocity = new Vector2(0, yVel);
            }
        }
        // in-air movement
        else
        {
            if (!(Math.Sign(moveVector) == Math.Sign(xVel) && Math.Abs(xVel) > speed))
            {
            Rigidbody.AddForce(Math.Sign(moveVector) * InAirAcceleration * Vector2.right, ForceMode2D.Force);
            }

            // slow down if player is not inputting horizontal movement
            // and don't apply if grappling
            if (moveVector == 0 && !isLineGrappling)
            {
                // apply horizontal "drag" based on current x velocity
                Rigidbody.AddForce(-xVel * InAirDrag * Vector2.right, ForceMode2D.Force);
            }

            if (recentlyImpulsed && Math.Abs(xVel) <= speed)
            {
                recentlyImpulsed = false;
            }

            // apply max velocity if not grappling
            if (!isLineGrappling && !recentlyImpulsed)
            {
                Rigidbody.velocity = new Vector2(
                    Mathf.Clamp(xVel, -speed, speed),
                    Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));
            }
        }
    }

    

    private void TurnAround_FixedUpdate() {
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
        Rigidbody.velocity = new Vector2(v.x, v.y - (gravityValue * Time.deltaTime));
        CheckGrounded_Update();
        CheckPlatformGrounded_Update();
        EventHandling_Update();
        ShortJumpDetection_Update();
        WallSlideDetection_Update();
        LineGrappleUpdate();
        Crouching_Update();

    }
    
    

    private void Crouching_Update()
    {
        if (isCrouching && CheckSpace())
        {
            UnCrouch();
        }
    }
    



    private void LineGrappleUpdate()
    {

        if (!isLineGrappling && !isGrappleLaunched && GrapplePoint.TargetPoint != null)
        {
            Vector3 targetPosition = GrapplePoint.TargetPoint.transform.position;
            Vector3 direction = (targetPosition - transform.position).normalized;
            Vector3 offset = Vector3.up * .5f;
            
            RaycastHit2D hit = Physics2D.Raycast(transform.position +  offset, direction,
                Vector2.Distance(transform.position, targetPosition), obstacleLayerMask);
            
            if (hit.collider != null)
            {
                GrapplePoint.TargetPoint.Blocked();
                grappleClearRenderer.enabled = false;
                grappleBlocked = true;
                grappleLOSRenderer.enabled = true;
                grappleLOSRenderer.SetPosition(1, transform.position + offset);
                grappleLOSRenderer.SetPosition(0, GrapplePoint.TargetPoint.transform.position);
            }
            else
            {
                GrapplePoint.TargetPoint.Cleared();
                grappleBlocked = false;
                grappleLOSRenderer.enabled = false;
                grappleClearRenderer.enabled = true;
                grappleClearRenderer.SetPosition(1, transform.position + offset);
                grappleClearRenderer.SetPosition(0, GrapplePoint.TargetPoint.transform.position);
            }
        }
        else
        {
            grappleBlocked = false;
            //Debug.Log("red off");
            grappleLOSRenderer.enabled = false;
            grappleClearRenderer.enabled = false;
        }
        
        if (isGrappleLaunched && sentProjectile != null)
        {
            grappleLineRenderer.SetPosition(1, transform.position);
            grappleLineRenderer.SetPosition(0, sentProjectile.transform.position);
        }
        const float grappleSpeed = 15f;
        const float disconnectDistance = .3f;
        if (isLineGrappling)
        {
            Vector3 direction = (launchedPoint.transform.position - transform.position).normalized;
            Rigidbody.velocity = direction * grappleSpeed;
            grappleLineRenderer.SetPosition(1, transform.position);
            grappleLineRenderer.SetPosition(0, launchedPoint.transform.position);
            

            
            if (isLineGrappling && 
                Vector2.Distance(transform.position, launchedPoint.transform.position) < disconnectDistance)
            {
                DisconnectGrapple();
            }
        }
    }

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

        bool newlyGrounded = hit1 || hit2;
        if (!isGrounded && newlyGrounded){
            OnLanding();
        }

        isGrounded = newlyGrounded;
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
        isWallSliding = (isInverted ? -v.y : v.y) <= 0 && 
                        ((isNearWallOnRight && transform.localScale.x < 0) || (isNearWallOnLeft && transform.localScale.x > 0)) && IsAbleToMove() && !isGrounded;
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
            Rigidbody.velocity = new Vector2(v.x,
                isInverted ? Mathf.Max(-v.y, wallSlideSpeed) : Mathf.Max(v.y, -wallSlideSpeed));
        }

    }
    
}
