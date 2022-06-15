using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.SceneManagement;

public partial class CharController
{
    public GameObject fadeSprite;
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
        SaveData.LoadFromFile(1);
        fury = 0;
        lastShiftTime = 0f;
        Interactor.interactors.Clear();
        canCast = true;
        //canDoubleJump = false;
        fadeSpriteIterator = 0;
        //capeAnchor = transform.Find("Cape").Find("CapeAnchor").GetComponent<CapeController>();
        //capeOutlineAnchor = transform.Find("Cape").Find("CapeAnchor1").GetComponent<CapeController>();
        grappleLineRenderer = transform.GetComponent<LineRenderer>();
        grappleLineRenderer.enabled = false;
        grappleLOSRenderer = transform.Find("GrappleLOS").GetComponent<LineRenderer>();
        grappleLOSRenderer.enabled = false;
        grappleClearRenderer = transform.Find("GrappleClear").GetComponent<LineRenderer>();
        grappleClearRenderer.enabled = false;
        groundCheck = transform.Find("GroundCheck").GetComponent<BoxCollider2D>();

        charLight = transform.Find("Light").GetComponent<Light2D>();
        lightBuffer = MaxLightBuffer;
        particleChild = transform.Find("Particles");
        // CurrentHealth = MaxHealth;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.Find("SpriteHandler").GetComponent<Animator>();
        charCollider = transform.GetComponent<BoxCollider2D>();
        
        dust = particleChild.Find("DustPS").GetComponent<ParticleSystem>();
        sliceDashPS = particleChild.Find("SliceDashPS").GetComponent<ParticleSystem>();
        parryPS = particleChild.Find("ParryPS").GetComponent<ParticleSystem>();
        switchPS = particleChild.Find("SwitchPS").GetComponent<ParticleSystem>();
        trailRenderer = particleChild.Find("FX").GetComponent<TrailRenderer>();
        //fadePS = particleChild.Find("FadePS").GetComponent<ParticleSystem>();
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        obstaclePlusLayerMask = LayerMask.GetMask("Obstacle", "Moveable");
        
        
        screenShakeController = ScreenShakeController.Instance;
        // grappleController = GetComponent<RadialGrapple>();
        spriteRenderer = transform.Find("SpriteHandler").GetComponent<SpriteRenderer>();
        
        trailRenderer.emitting = false;
        charLight.enabled = false;
        if (SceneInformation.Instance.isDarkScene)
        {
            Debug.Log("light true");
            charLight.enabled = true;
        }

        // set char's spawn
        //Debug.Log(SceneInformation.Instance.GetSpawnPos());
        transform.position = SceneInformation.Instance.GetSpawnPos();
        base.Start();
    }

    private void FixedUpdate() {
        // if (wallJumpAvailable && !justJumped)
        // {
        //     Debug.Log("wj aiv");
        // }
        
        // Debug.Log("touching" + isWallTouching);
        // Debug.Log("sliding" + isWallSliding);
        Animstate_FixedUpdate();
        
        
        inputVector = Input.GetAxisRaw("Horizontal");
        FadeParticle_FixedUpdate();
        // AdjustCape_FixedUpdate();
        if (!IsAbleToMove()) return;
        // movement animations
        
        
        

        if (SceneInformation.Instance.isWindScene)
        {
            WindMovement_FixedUpdate();
        }
        else
        {
            gravityValue = isInverted ? -Mathf.Abs(gravityValue) : Mathf.Abs(gravityValue);
            StandardMovement_FixedUpdate();
        }
        ApplyForcedMovement_FixedUpdate();

        TurnAround_FixedUpdate();
        
    }

    private void Animstate_FixedUpdate()
    {
        if (isWallSliding)
        {
            //Debug.Log("wallslide anim");
            Animator.SetInteger(AnimState, 3);
        }
        else if (isGrounded && Mathf.Abs(moveVector) > float.Epsilon)
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

    // private const float verticalConst = .35f;
    // private const float horizontalConst = .35f;
    // private void AdjustCape_FixedUpdate()
    // {
    //     Vector2 currentOffset = Vector2.zero;
    //     if (Rigidbody.velocity.x == 0 && Rigidbody.velocity.y == 0)
    //     {
    //         //Debug.Log("idle");
    //         currentOffset = idleOffset;
    //     }
    //     else if (Rigidbody.velocity.y > .1f)
    //     {
    //         //Debug.Log("jump");
    //         currentOffset = new Vector2( jumpOffset.x, jumpOffset.y * 
    //                                                    Mathf.Abs(Rigidbody.velocity.y) * verticalConst);
    //     }
    //     else if (Rigidbody.velocity.y < -.1f)
    //     {
    //         //Debug.Log("fall");
    //         currentOffset = new Vector2( fallOffset.x, fallOffset.y * 
    //                                                    Mathf.Abs(Rigidbody.velocity.y) * verticalConst);
    //     }
    //     else if (Rigidbody.velocity.x != 0)
    //     {
    //         //Debug.Log("run");
    //         currentOffset = new Vector2( runOffset.x * 
    //                                      Mathf.Abs(Rigidbody.velocity.x) * horizontalConst, runOffset.y);
    //     }
    //
    //     if (transform.localScale.x < 0)
    //     {
    //         currentOffset.x = currentOffset.x * -1;
    //     }
    //     
    //     //capeAnchor.partOffset = currentOffset;
    //     //capeOutlineAnchor.partOffset = currentOffset;
    //
    //
    // }

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
            // move if not wall sliding (?)
            //if (!isWallSliding)
            //{
            Rigidbody.AddForce(Math.Sign(moveVector) * InAirAcceleration * Vector2.right, ForceMode2D.Force);
            //}

            // slow down if player is not inputting horizontal movement
            // and don't apply if grappling
            if (moveVector == 0 && !isLineGrappling)
            {
                // apply horizontal "drag" based on current x velocity
                Rigidbody.AddForce(-xVel * InAirDrag * Vector2.right, ForceMode2D.Force);
            }

            // apply max velocity if not grappling
            if (!isLineGrappling)
            {
                Rigidbody.velocity = new Vector2(
                    Mathf.Clamp(xVel, -speed, speed),
                    Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));
            }
        }
    }

    private void WindMovement_FixedUpdate()
    {
        if (currentWind == null)
        {
            currentWind = GameManager.Instance.isGameShifted
                ? SceneInformation.Instance.altStateWind
                : SceneInformation.Instance.realStateWind;
        }
        Debug.Assert(SceneInformation.Instance.isWindScene);
        
        Vector2 v = Rigidbody.velocity;
        float xVel = v.x;
        float yVel = v.y;
        
        float horizWindSpeed = currentWind.isHorizontal ? currentWind.speedOnPlayer : 0;
        // float vertWindSpeed = isHorizWind? 0 : currentWindZone.windSpeedOnPlayer;

        // regular ground movement
        if (isGrounded)
        {
            int moveDir = Math.Sign(moveVector);
            // if user is not moving and has speed, then slow down
            if (moveDir == 0 && !Mathf.Approximately(xVel, horizWindSpeed))
            {
                int antiMoveDir = -Math.Sign(xVel - horizWindSpeed);

                // TODO change this if we choose to add ice or something
                Rigidbody.AddForce(antiMoveDir * OnGroundDeceleration * Vector2.right, ForceMode2D.Force);
            }
            // otherwise move the player in the direction
            else if (moveDir != 0)
            {
                Rigidbody.AddForce(moveDir * OnGroundAcceleration * Vector2.right, ForceMode2D.Force);
            }
            
        }
        // in-air movement
        else
        {
            // move if not wall sliding (?)
            if (!isWallSliding)
            {
                Rigidbody.AddForce(Math.Sign(moveVector) * InAirAcceleration * Vector2.right, ForceMode2D.Force);
            }

            // slow down if player is not inputting horizontal movement
            // and don't apply if grappling
            if (moveVector == 0 && !isLineGrappling)
            {
                // apply horizontal "drag" based on current x velocity
                Rigidbody.AddForce(-(xVel - horizWindSpeed) * InAirDrag * Vector2.right, ForceMode2D.Force);
            }

            // apply max velocity if not grappling
            if (!isLineGrappling)
            {
                Vector2 vel = Rigidbody.velocity;
                Rigidbody.velocity = new Vector2(Mathf.Clamp(vel.x, -speed, speed), yVel);
            }
        }
        
        // if no wind,
        // if (!SceneInformation.Instance.isWindScene)
        // {
        //     // TODO: if yVel out of range, use a force to slow down, don't just clamp
        //     // TODO: also apply this in non-wind movement
        //     Rigidbody.velocity = new Vector2(
        //         Mathf.Clamp(xVel, -speed, speed),
        //         Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));
        // }
        // else if sideways wind,
        if (currentWind.isHorizontal)
        {
            // apply horizontal wind max velocity
            int windDir = (currentWind.speedOnPlayer < 0)? -1 : 1; // -1 if left, 1 if right
            // int velDir = Math.Sign(xVel);
            float maxSpeedSameDir = speed + Mathf.Abs(currentWind.speedOnPlayer);
            float maxSpeedOppDir = speed - Mathf.Abs(currentWind.speedOnPlayer);
            float maxLeft, maxRight;
            if (windDir == 1) // if right wind
            {
                maxLeft = -maxSpeedOppDir;
                maxRight = maxSpeedSameDir;
            }
            else
            {
                maxLeft = -maxSpeedSameDir;
                maxRight = maxSpeedOppDir;
            }

            Rigidbody.velocity = new Vector2(
                Mathf.Clamp(xVel, maxLeft, maxRight),
                Mathf.Clamp(yVel, -MaxYSpeed, MaxYSpeed));
        }
        // else if vertical wind
        else
        {
            Debug.Log("vertical wind");
            // apply vertical wind max velocity
            
            // affect gravity
            float windSpeed = currentWind.speedOnPlayer;
            float gravChange = -0.1f * windSpeed; // TODO: constant
            gravityValue = (isInverted ? -Math.Abs(gravityValue) : Math.Abs(gravityValue)) + gravChange; // TODO: change back
            
            // apply max vel
            int windDir = (windSpeed < 0)? -1 : 1; // -1 if left, 1 if right
            // int velDir = Math.Sign(yVel);
            float maxSpeedSameDir = MaxYSpeed + Mathf.Abs(windSpeed);
            float maxSpeedOppDir = MaxYSpeed - Mathf.Abs(windSpeed);
            float maxDown, maxUp;
            if (windDir == 1) // if up wind
            {
                maxDown = -maxSpeedOppDir;
                maxUp = maxSpeedSameDir;
            }
            else
            {
                maxDown = -maxSpeedSameDir;
                maxUp = maxSpeedOppDir;
            }

            Rigidbody.velocity = new Vector2(
                Mathf.Clamp(xVel, -speed, speed),
                Mathf.Clamp(yVel, maxDown, maxUp));

            // max velocity but approach it rather than clamp it
            // if (yVel < maxDown || yVel > maxUp)
            // {
                // Debug.Log("applying drag");
                // float g = Physics2D.gravity.y;
                // float vMax = (velDir == 1) ? maxUp : maxDown;
                // float drag = -g / vMax;
                // float dir = (yVel < 0) ? 1 : -1;
                // Rigidbody.AddForce(dir * VerticalDrag * Vector2.up);
            // }

            // if (windDir == velDir) 
            // {
            //     Rigidbody.velocity = new Vector2(
            //         Mathf.Clamp(xVel, -speed, speed), 
            //         Mathf.Clamp(yVel, -maxSpeedSameDir, maxSpeedSameDir));
            // }
            // else if (windDir == -velDir) 
            // {
            //     Rigidbody.velocity = new Vector2(
            //         Mathf.Clamp(xVel, -speed, speed), 
            //         Mathf.Clamp(yVel, -maxSpeedOppDir, maxSpeedOppDir));
            // }
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
        if (moveVector > 0 && Math.Abs(scale.x + 1) > float.Epsilon) {
            FaceRight();
        }
        else if (moveVector < 0 && Math.Abs(scale.x - 1) > float.Epsilon) {
            FaceLeft();
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.P))
            SceneManager.LoadScene("TTHub");
        if (Input.GetKeyDown(KeyCode.LeftBracket))
            SaveData.SaveToFile(1);
        if (Input.GetKeyDown(KeyCode.RightBracket))
            SaveData.LoadFromFile(1);

        if (!isAttacking)
        {
            Animator.speed = 1;
        }
        
        
        // if (WindEmitterChild.targetWind == null) {
        //     currentWind = null;
        // }
        // else
        // {
        //     currentWind = WindEmitterChild.targetWind.GetComponentInParent<WindEmitter>();
        // }

        Vector2 v = Rigidbody.velocity;
        Rigidbody.velocity = new Vector2(v.x, v.y - (gravityValue * Time.deltaTime));
        CheckGrounded_Update();
        EventHandling_Update();

        // apply wind min velocity lolol
        if (!(currentWind == null) && Math.Sign(moveVector) == 0)
        {
            float xVel = Rigidbody.velocity.x;
            float horizWindSpeed = currentWind.speedOnPlayer;
            // apply min velocity
            if (Math.Sign(xVel) == Math.Sign(horizWindSpeed) && Mathf.Abs(xVel) < Math.Abs(horizWindSpeed))
            {
                Rigidbody.velocity = new Vector2(horizWindSpeed, Rigidbody.velocity.y);
                // Debug.Log("applied min vel, new x vel is " + Rigidbody.velocity.x);
            }
        }
        
        // // jump animation
        // if (isGrounded) {
        //     Animator.SetBool(Jump, false);
        // }
        ShortJumpDetection_Update();
        //JumpCooldown_Update();
        WallSlideDetection_Update();
        SliceDashDetection_Update();
        LineGrappleUpdate();
        if (SceneInformation.Instance.isDarkScene)
        {
            //LightCheckUpdate();
        }
    }
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, AttackRange);
    }

    // private void LightCheckUpdate()
    // {
    //     if (lightBuffer < 0)
    //     {
    //         TakeDamage(1);
    //     }
    //     else
    //     {
    //         lightBuffer -= Time.deltaTime;
    //         charLight.pointLightOuterRadius = MaxOuterLightRadius * (lightBuffer / MaxLightBuffer);
    //         charLight.pointLightInnerRadius = MaxInnerLightRadius * (lightBuffer / MaxLightBuffer);
    //         charLight.intensity = MaxLightIntensity * (lightBuffer / MaxLightBuffer);
    //     }
    // }



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


    private void CheckGrounded_Update()
    {
        const float groundDistance = 0.05f;

        Vector3 bounds = charCollider.bounds.extents;
        float halfWidth = Mathf.Abs(bounds.x) - groundDistance;
        float halfHeight = Mathf.Abs(bounds.y) - groundDistance;
        Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
        Vector2 bottomMiddle = center + halfHeight * Vector2.down;
        Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
        Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
        Vector2 aLittleDown = 5 * groundDistance * Vector2.down;
        
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

    private void SliceDashDetection_Update() {
        if (isSliceDashing) {
            const int maxEnemiesHit = 1;
            Collider2D[] hitColliders = new Collider2D[maxEnemiesHit];

            // scan for hit enemies
            Physics2D.OverlapCircleNonAlloc(
                slicePoint.position, AttackRange, hitColliders, enemyLayers);
            
            if (hitColliders[0] != null) {
                //Debug.Log("execute");
                StartCoroutine(SliceExecuteCoroutine(hitColliders[0].GetComponent<Enemy>()));
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
                    .Initialize(spriteRenderer.sprite, transform.localScale.x > 0);
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
        bool isNearWallOnLeft = bottomLeftHit && topLeftHit;
        

        // right linecasts
        RaycastHit2D bottomRightHit = 
            Physics2D.Linecast(bottomRight, bottomRight + aLittleRight, obstacleLayerMask);
        RaycastHit2D topRightHit = 
            Physics2D.Linecast(topRight, topRight + aLittleRight, obstacleLayerMask);
        bool isNearWallOnRight = bottomRightHit && topRightHit;

        // isWallSliding = v.y <= 0 && ((moveVector > 0 && isNearWallOnRight) 
        //                              || (moveVector < 0 && isNearWallOnLeft)) && IsAbleToMove();
        // isWallSliding = (isInverted ? -v.y : v.y) <= 0 && 
        //                 ((isNearWallOnRight && moveVector >= 0)|| (isNearWallOnLeft && moveVector <= 0)) && IsAbleToMove();
        isWallSliding = (isInverted ? -v.y : v.y) <= 0 && 
                        ((isNearWallOnRight)|| (isNearWallOnLeft)) && IsAbleToMove() && !isGrounded;
        //Debug.Log(wallJumpAvailable);

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
        else
        {
        }
    }
    
}
