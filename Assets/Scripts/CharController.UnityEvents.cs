using System;
using System.Collections;
using Cinemachine;
using UnityEngine;

public partial class CharController
{
    private void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;
        //DontDestroyOnLoad(gameObject);
        PrepForScene();
        rigidbody = transform.GetComponent<Rigidbody2D>();
        GrabComponents_Awake();
    }

    protected override void Start()
    {
        fadeSpriteIterator = 0;
        runSpeed = MaxMoveSpeedGround;

        originalColliderSize = charCollider.size;
        originalColliderOffset = charCollider.offset;

        charLight.enabled = false;

        transform.position = SceneInformation.instance.GetInitialSpawnPosition();
        originalBoostVisualColor = boostUseIndicator.color;
        maxSpeedBar = MaxSpeedBar;
        currentSpeedBar = StartingSpeedBar;
        base.Start();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (GameManager.instance.isPaused)
                GameManager.instance.Unpause();
            else
                GameManager.instance.Pause();
        }

        // if (Input.GetKeyDown(KeyCode.LeftBracket))
        //     SaveData.SaveToFile(1);
        // if (Input.GetKeyDown(KeyCode.RightBracket))
        //     SaveData.LoadFromFile(1);


        CheckGrounded_Update();
        CheckPlatformGrounded_Update();
        EventHandling_Update();
        ShortJumpDetection_Update();
        WallSlideDetection_Update();
        //LineGrappleUpdate();
        GrappleInput_Update();
        Crouching_Update();
        LookAhead_Update();
        BoostRefreshCheck_Update();

        recentImpulseTime -= Time.deltaTime;
    }

    private void FixedUpdate()
    {
        charDebugText.text =
            Math.Floor(rigidbody.velocity.magnitude)
            + "   "
            + '\n'
            + Math.Floor(rigidbody.velocity.x)
            + "   "
            + Math.Floor(rigidbody.velocity.y);
        //Animstate_FixedUpdate();
        inputVector = Input.GetAxisRaw("Horizontal");
        FadeParticle_FixedUpdate();
        DashTrail_FixedUpdate();
        Gravity_FixedUpdate();
        // AdjustCape_FixedUpdate();
        if (disabledMovement)
            moveVector = 0;

        if (IsAbleToMove())
        {
            StandardMovement_FixedUpdate();
            ApplyForcedMovement_FixedUpdate();
            TurnAround_FixedUpdate();
        }

        GrapplePhysics_FixedUpdate();
        // movement animations
        //gravityValue = isInverted ? -Mathf.Abs(gravityValue) : Mathf.Abs(gravityValue);

        Record_FixedUpdate();
        SpeedBar_FixedUpdate();
    }

    private void SpeedBar_FixedUpdate()
    {
        print(currentSpeedBar);
        var currentSpeed = rigidbody.velocity.magnitude;
        var multiplier =
            currentSpeed > SpeedBarVelThreshhold ? SpeedBarGainMultiplier : SpeedBarDecayMultiplier;
        currentSpeedBar +=
            (currentSpeed - SpeedBarVelThreshhold) * Time.fixedDeltaTime * multiplier;

        if (currentSpeedBar < 1)
            currentSpeedBar = StartingSpeedBar;
        //Die();
    }

    private void Record_FixedUpdate()
    {
        if (!doRecord || isRewinding)
            return;

        if (pointsInTime.Count == 0)
        {
            pointsInTime.Insert(0, transform.position);
            return;
        }

        var last = pointsInTime[0];
        if (last != transform.position)
            pointsInTime.Insert(0, transform.position);
    }

    public IEnumerator StartRewind()
    {
        var savedPriority = CameraManager.instance.currentCam.Priority;
        CameraManager.instance.currentCam.Priority = 50;
        doRecord = false;
        rigidbody.velocity = Vector3.zero; //diving fix (LOL RERUN XD)
        charCollider.isTrigger = true;
        isRewinding = true;
        rigidbody.bodyType = RigidbodyType2D.Static;
        emitFadesTime += .45f;
        //while (pointsInTime.Count > 0) {
        for (var i = 0; i < pointsInTime.Count; i += RewindSpeed)
        {
            var pointInTime = pointsInTime[i];
            transform.position = pointInTime;
            yield return new WaitForFixedUpdate();
        }

        pointsInTime.Clear();
        isRewinding = false;
        rigidbody.bodyType = RigidbodyType2D.Dynamic;
        charCollider.isTrigger = false;
        CameraManager.instance.currentCam.Priority = savedPriority;
    }

    // private void Animstate_FixedUpdate()
    // {
    //     if (isWallSliding)
    //     {
    //         //Debug.Log("wallslide anim");
    //         Animator.SetInteger(AnimState, 3);
    //     }
    //     else if (isGrounded && (Mathf.Abs(moveVector) > float.Epsilon))
    //     {
    //         //Debug.Log("run anim");
    //         Animator.SetInteger(AnimState, 2);
    //     }
    //     else
    //     {
    //         //Debug.Log("idle anim");
    //         Animator.SetInteger(AnimState, 1);
    //     }
    //     //print(Animator.GetCurrentAnimatorStateInfo(0).IsName("Death"));
    // }


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
        var v = rigidbody.velocity;
        var xVel = v.x;
        var yVel = v.y;

        // regular ground movement
        if (IsGrounded)
        {
            var moveDir = Math.Sign(moveVector);
            // if user is not moving and has runSpeed, then slow down
            if (moveDir == 0 && Mathf.Abs(xVel) >= MinGroundSpeed && !RecentlyImpulsed())
            {
                var antiMoveDir = -Math.Sign(xVel);
                rigidbody.AddForce(
                    antiMoveDir * OnGroundDeceleration * Vector2.right,
                    ForceMode2D.Force
                );
            }
            // otherwise move the player in the direction (only accelerate if below max runSpeed)
            else if ((moveDir > 0 && xVel < runSpeed) || (moveDir < 0 && xVel > -runSpeed))
            {
                rigidbody.AddForce(
                    moveDir * OnGroundAcceleration * Vector2.right,
                    ForceMode2D.Force
                );
            }

            // decelerate if above limit
            if (!RecentlyImpulsed())
            {
                if (Mathf.Abs(xVel) > GroundDragThreshholdB) //decelerate a lot if above threshold B
                {
                    var xAfterDrag = rigidbody.velocity.x - Math.Sign(xVel) * OnGroundDrag * 1.5f;
                    rigidbody.velocity = new Vector2(xAfterDrag, rigidbody.velocity.y);
                }
                else if (Mathf.Abs(xVel) > GroundDragThreshholdA) //otherwise, decelerate a little if above threshold A
                {
                    var xAfterDrag = rigidbody.velocity.x - Math.Sign(xVel) * OnGroundDrag;
                    rigidbody.velocity = new Vector2(xAfterDrag, rigidbody.velocity.y);
                }
            }

            // apply min velocity
            if (Mathf.Abs(xVel) < MinGroundSpeed)
                rigidbody.velocity = new Vector2(0, yVel);

            //recentlyImpulsed = false;
        }
        // in-air movement
        else
        {
            if (Math.Abs(xVel) < MaxMoveSpeedAir || Math.Sign(moveVector) != Math.Sign(xVel))
                rigidbody.AddForce(
                    Math.Sign(moveVector)
                    * InAirAcceleration
                    * Vector2.right
                    * (Math.Sign(moveVector) != Math.Sign(xVel) ? 2 : 1),
                    ForceMode2D.Force
                );

            // // slow down if player is not inputting horizontal movement
            // // and don't apply if grappling
            // if (moveVector == 0)
            // {
            //     // apply horizontal "drag" based on current x velocity
            //     Rigidbody.AddForce(-xVel * InAirDrag * Vector2.right, ForceMode2D.Force);
            // }
            if (Math.Sign(moveVector) == Math.Sign(xVel) && Math.Abs(xVel) > AirDragThreshholdA) //apply drag
                rigidbody.AddForce(-xVel * InAirDrag * Vector2.right, ForceMode2D.Force);
        }

        rigidbody.velocity = Vector2.ClampMagnitude(rigidbody.velocity, AbsoluteMaxVelocity);
        //boostUseIndicator.fillAmount = Rigidbody.velocity.magnitude / AbsoluteMaxVelocity;
        if (rigidbody.velocity.magnitude > 20f && !isDashing)
            emitFadesTime = .3f;
    }

    private void TurnAround_FixedUpdate()
    {
        if (disabledMovement)
            return;

        // feet dust logic
        if (Math.Abs(prevInVector - inputVector) > 0.01f && IsGrounded && inputVector != 0)
            dust.Play();
        //parentWindFX.transform.localScale = new Vector3(-parentWindFX.transform.localScale.x, 1, 1);
        prevInVector = inputVector;

        //Vector3 scale = transform.localScale;
        // direction switching
        // if (isWallSliding || (wallJumpAvailable && !justJumped))
        // {
        //     return;
        // }

        // if (isWallSliding)
        // {
        //     return;
        // }
        //
        if (moveVector > 0)
            FaceRight();
        else if (moveVector < 0)
            FaceLeft();
    }

    private void Gravity_FixedUpdate()
    {
        var v = rigidbody.velocity;
        var yVelToSet =
            v.y
            - (
                IsWallSliding
                    ? v.y < 0f
                        ? .4f
                        : .3f
                    : 1
            )
            * gravityValue
            * Time.fixedDeltaTime;
        yVelToSet = Math.Max(-MaxDownwardSpeedFromGravity, yVelToSet);
        //print(yVelToSet);
        rigidbody.velocity = new Vector2(v.x, yVelToSet);
    }

    private void BoostRefreshCheck_Update()
    {
        if (
            !groundedAfterBoost
            && (IsGrounded || isNearWallOnLeft || isNearWallOnRight)
            && lastBoostTime + BoostRefreshCooldown < Time.time
            && recentlyBoosted
        )
            StartCoroutine(BoostRefreshCoroutine());
        if (recentlyBoosted && lastBoostTime + BoostCooldown < Time.time && groundedAfterBoost)
            recentlyBoosted = false;
    }

    private void GrappleInput_Update()
    {
        if (Input.GetKeyUp(KeyCode.Q))
        {
            if (isGrappling)
                EndGrapple();
            if (instantiatedProjectile != null)
                Destroy(instantiatedProjectile.gameObject);
        }
    }

    private void GrapplePhysics_FixedUpdate()
    {
        grappleLineRenderer.widthMultiplier = 1;
        if (isGrappling)
        {
            //const float offsetMultiplier = 1f;
            //float offset = Mathf.Cos(Vector3.Angle(transform.position - attachmentPoint, Vector3.down)) * offsetMultiplier;
            //Debug.Log(Vector3.Angle(transform.position - attachmentPoint, Vector3.up));
            //Debug.Log(offset);
            //Rigidbody.velocity *= 1.001f;
            grappleLineRenderer.SetPosition(1, transform.position);
            grappleLineRenderer.SetPosition(0, attachmentPoint);

            var rotationToSet = Vector2.Angle(Vector2.right, attachmentPoint - transform.position);

            //print(rotationToSet);
            savedRotationalVelocity = rotationToSet - spriteHandler.eulerAngles.z; //calculate rotational velocity
            savedRotationalVelocity *= 1.5f; //boost
            //print(rotationToSet + "  -- " +  spriteHandler.eulerAngles.z + " -- " + savedRotationalVelocity);

            spriteHandler.rotation = Quaternion.Euler(
                spriteHandler.rotation.x,
                spriteHandler.rotation.y,
                rotationToSet
            );

            var grappleLength = (attachmentPoint - transform.position).magnitude;
            grappleDistanceJoint.distance = grappleLength;
            if (rigidbody.velocity.y < 0 && rigidbody.velocity.magnitude < AirDragThreshholdB)
                rigidbody.velocity *= GrappleAcceleration;

            var xVel = rigidbody.velocity.x;
            if ((xVel > 0 && isNearWallOnRight) || (xVel < 0 && isNearWallOnLeft))
                EndGrapple();

            //grappleLineRenderer.SetPosition(0, instantiatedProjectile.transform.position);
        }
        else if (instantiatedProjectile != null)
        {
            grappleLineRenderer.SetPosition(1, transform.position);
            grappleLineRenderer.SetPosition(0, instantiatedProjectile.transform.position);
        }
        else
        {
            grappleLineRenderer.widthMultiplier = 0;
        }
    }

    private void LaunchHook()
    {
        //grappleLineRenderer.enabled = true;
        var direction = new Vector2(1, 1.4f);
        if (IsFacingLeft())
            direction.x = -direction.x;

        if (isInverted)
            direction.y = -direction.y;

        //print("hook launched");
        var offset = isInverted ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        if (instantiatedProjectile != null)
            Destroy(instantiatedProjectile);

        instantiatedProjectile = Instantiate(
            projectilePrefab,
            transform.position + offset,
            transform.rotation
        );
        instantiatedProjectile.gameObject
            .GetComponent<GrappleProjectile>()
            .Initialize(direction.normalized, GrappleLaunchSpeed);
        leftGrapple = IsFacingLeft();
    }

    public void StartGrapple(Vector3 grapplePoint)
    {
        BoostRefresh();
        ForceRotationEnd();
        doubleJumpAvailable = true;
        gravityValue = baseGravity * .3f;
        attachmentPoint = grapplePoint;

        //Vector3 diffNormalized = (grapplePoint - transform.position).normalized ;
        transform.position += new Vector3(0, GrappleVerticalDisplacement, 0);
        ReduceSize();
        dashTrail.emitting = true;

        isGrappling = true;
        //charController.isRecentlyGrappled = true;
        //grappleLineRenderer.SetPosition(0, grapplePoint);
        //grappleLineRenderer.SetPosition(1, transform.position);
        grappleDistanceJoint.connectedAnchor = grapplePoint;

        grappleDistanceJoint.enabled = true;
        //grappleDistanceJoint.distance = Math.Max(grappleDistanceJoint.distance, 3f);
        //grappleLineRenderer.enabled = true;


        //const float boostForce = 5f;
        var initialGrappleBoost =
            MinGrappleVelocity * Math.Max(grappleDistanceJoint.distance / 5, 1);

        if (leftGrapple)
        {
            //facing left
            rigidbody.velocity -=
                new Vector2(Math.Abs(rigidbody.velocity.y), 0) * GrappleGravityBoostModifier;
            if (rigidbody.velocity.x > -MinGrappleVelocity)
                rigidbody.velocity = Vector2.left * initialGrappleBoost;
            //Debug.Log("left boost");
        }
        else
        {
            rigidbody.velocity +=
                new Vector2(Math.Abs(rigidbody.velocity.y), 0) * GrappleGravityBoostModifier;
            if (rigidbody.velocity.x < MinGrappleVelocity)
                rigidbody.velocity = Vector2.right * initialGrappleBoost;
            //Debug.Log("right boost");
        }
    }

    public void EndGrapple()
    {
        if (instantiatedProjectile != null)
            Destroy(instantiatedProjectile.gameObject);

        ReturnSize();
        dashTrail.emitting = false;
        grappleDistanceJoint.enabled = false;
        isGrappling = false;
        gravityValue = baseGravity;
        SmoothRotationEnd();
        //ForceRotationEnd();
    }

    private void LookAhead_Update()
    {
        if (CameraManager.instance.currentCam is null)
            return;

        var transposer =
            CameraManager.instance.currentCam.GetCinemachineComponent<CinemachineFramingTransposer>();

        if (
            Input.GetKeyDown(KeyCode.F)
            && CameraManager.instance.currentCam.gameObject.CompareTag("DynamicCamera")
            && !(transposer is null)
        )
            transposer.m_TrackedObjectOffset.x =
                CameraManager.instance.currentCam.m_Lens.OrthographicSize
                * -1.8f
                * transform.localScale.x;

        if (
            !(transposer is null)
            && (
                Input.GetKeyUp(KeyCode.F)
                || !CameraManager.instance.currentCam.gameObject.CompareTag("DynamicCamera")
            )
        )
            transposer.m_TrackedObjectOffset.x = 0;
    }

    private void Crouching_Update()
    {
        if (isCrouching && CheckSpace())
            UnCrouch();
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
        var relativeDown = isInverted ? Vector2.up : Vector2.down;

        var bounds = charCollider.bounds.extents;
        var halfWidth = Mathf.Abs(bounds.x) - groundDistance;
        var halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        var center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;

        var bottomMiddle = center + halfHeight * relativeDown;
        var bottomLeft = bottomMiddle + halfWidth * Vector2.left;
        var bottomRight = bottomMiddle + halfWidth * Vector2.right;
        var aLittleDown = 3 * groundDistance * relativeDown;

        Debug.DrawLine(bottomLeft, bottomLeft + aLittleDown, Color.magenta);
        Debug.DrawLine(bottomRight, bottomRight + aLittleDown, Color.magenta);

        var hit1 = Physics2D.Linecast(bottomLeft, bottomLeft + aLittleDown, obstaclePlusLayerMask);
        var hit2 = Physics2D.Linecast(
            bottomRight,
            bottomRight + aLittleDown,
            obstaclePlusLayerMask
        );

        var newlyGrounded = (hit1 || hit2) && !IsWallSliding;
        if (!IsGrounded && newlyGrounded)
            OnGroundLanding();

        IsGrounded = newlyGrounded;

        //Animator.SetBool(Grounded, isGrounded);
    }

    private void CheckPlatformGrounded_Update()
    {
        const float groundDistance = 0.05f;
        var relativeDown = isInverted ? Vector2.up : Vector2.down;

        var bounds = charCollider.bounds.extents;
        var halfWidth = Mathf.Abs(bounds.x) - groundDistance;
        var halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        var center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;

        var bottomMiddle = center + halfHeight * relativeDown;
        var bottomLeft = bottomMiddle + halfWidth * Vector2.left;
        var bottomRight = bottomMiddle + halfWidth * Vector2.right;
        var aLittleDown = 3 * groundDistance * relativeDown;

        Debug.DrawLine(bottomLeft, bottomLeft + aLittleDown, Color.magenta);
        Debug.DrawLine(bottomRight, bottomRight + aLittleDown, Color.magenta);

        var hit1 = Physics2D.Linecast(bottomLeft, bottomLeft + aLittleDown, platformLayerMask);
        var hit2 = Physics2D.Linecast(bottomRight, bottomRight + aLittleDown, platformLayerMask);

        isPlatformGrounded = hit1 || hit2;
    }

    private void EventHandling_Update()
    {
        // add events if their respective buttons are pressed
        foreach (var pair in KeyToEventType)
            if (pair.Key.Invoke())
                // Debug.Log("enqueueing " + pair.Value + " event");
                eventQueue.AddLast(new Event(pair.Value, Time.time));

        // parse event queue
        for (var node = eventQueue.First; node != null; node = node.Next)
        {
            var e = node.Value;

            // remove expired events
            if (Time.time > e.timeCreated + Event.EventTimeout)
            {
                // Debug.Log(e.EventType + " event timed out");
                eventQueue.Remove(node);
            }
            // execute events whose conditions are met
            else
            {
                var conditions = EventConditions[e.eventType];
                var actionToDo = EventActions[e.eventType];

                if (conditions.Invoke(this))
                {
                    // Debug.Log("reached enqueued " + e.EventType + ", invoking");
                    actionToDo.Invoke(this);
                    eventQueue.Remove(node);
                }
            }
        }
    }

    private void ShortJumpDetection_Update()
    {
        if (
                Input.GetButtonUp("Jump")
                && !IsGrounded
                && (
                    (!isInverted && rigidbody.velocity.y > 0)
                    || (isInverted && rigidbody.velocity.y < 0)
                )
            )
            //print("short");
            rigidbody.velocity = Vector2.Scale(rigidbody.velocity, new Vector2(1f, 0.5f));
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
    private void DashTrail_FixedUpdate()
    {
        dashTrailEmitTime -= Time.deltaTime;
        dashTrail.emitting = dashTrailEmitTime > 0;
    }

    private void FadeParticle_FixedUpdate()
    {
        if (emitFadesTime > 0)
        {
            const int fadeSpriteLimiter = 4;
            fadeSpriteIterator += 1;
            emitFadesTime -= Time.deltaTime;
            if (fadeSpriteIterator == fadeSpriteLimiter)
            {
                fadeSpriteIterator = 0;
                var newFadeSprite = Instantiate(fadeSprite, transform.position, transform.rotation);
                newFadeSprite
                    .GetComponent<FadeSprite>()
                    .Initialize(spriteRenderer.sprite, transform.localScale.x > 0, isInverted);
            }
        }
    }

    private void WallSlideDetection_Update()
    {
        //const float wallSlideSpeed = 0.75f;
        const float groundDistance = 0.05f;
        const float horizontalBuffer = .15f;

        var bounds = charCollider.bounds.extents;
        var halfWidth = Mathf.Abs(bounds.x) - groundDistance + horizontalBuffer;
        var halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        var center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;

        // left points
        var middleLeft = center + halfWidth * Vector2.left;
        var topLeft = middleLeft + halfHeight * Vector2.up;
        var bottomLeft = middleLeft + halfHeight * Vector2.down;
        var aLittleLeft = 2 * groundDistance * Vector2.left;

        // right points
        var middleRight = center + halfWidth * Vector2.right;
        var topRight = middleRight + halfHeight * Vector2.up;
        var bottomRight = middleRight + halfHeight * Vector2.down;
        var aLittleRight = 2 * groundDistance * Vector2.right;

        // left linecasts
        var bottomLeftHit = Physics2D.Linecast(
            bottomLeft,
            bottomLeft + aLittleLeft,
            obstacleLayerMask
        );
        var topLeftHit = Physics2D.Linecast(topLeft, topLeft + aLittleLeft, obstacleLayerMask);
        isNearWallOnLeft =
            (
                bottomLeftHit
                && bottomLeftHit.transform.GetComponent<BeatPlatform>() != null
                && bottomLeftHit.transform.GetComponent<BeatPlatform>().isWallSlideable
            )
            || (
                topLeftHit
                && topLeftHit.transform.GetComponent<BeatPlatform>() != null
                && topLeftHit.transform.GetComponent<BeatPlatform>().isWallSlideable
            );

        // right linecasts
        var bottomRightHit = Physics2D.Linecast(
            bottomRight,
            bottomRight + aLittleRight,
            obstacleLayerMask
        );
        var topRightHit = Physics2D.Linecast(topRight, topRight + aLittleRight, obstacleLayerMask);
        isNearWallOnRight =
            (
                bottomRightHit
                && bottomRightHit.transform.GetComponent<BeatPlatform>() != null
                && bottomRightHit.transform.GetComponent<BeatPlatform>().isWallSlideable
            )
            || (
                topRightHit
                && topRightHit.transform.GetComponent<BeatPlatform>() != null
                && topRightHit.transform.GetComponent<BeatPlatform>().isWallSlideable
            );

        var newlyWallSliding =
            (isNearWallOnRight || isNearWallOnLeft) && IsAbleToMove() && !IsGrounded;
        if (!IsWallSliding && newlyWallSliding)
            OnWallLanding();

        IsWallSliding = newlyWallSliding;

        if (isNearWallOnLeft)
        {
            if (!leftWallPS.isEmitting)
            {
                print("left play");
                leftWallPS.Play();
            }

            //inputVector = Mathf.Clamp(inputVector, 0f, 1f);

            leftWallIndicator.enabled = true;
            rightWallIndicator.enabled = false;
            wallJumpDir = 1;
        }
        else if (isNearWallOnRight)
        {
            if (!rightWallPS.isEmitting)
            {
                print("right play");
                rightWallPS.Play();
            }

            //inputVector = Mathf.Clamp(inputVector, -1f, 0f);

            leftWallIndicator.enabled = false;
            rightWallIndicator.enabled = true;
            wallJumpDir = -1;
            //canDoubleJump = false;
        }
        else
        {
            leftWallIndicator.enabled = false;
            rightWallIndicator.enabled = false;
        }

        if (IsWallSliding)
            justJumped = false;
    }
}