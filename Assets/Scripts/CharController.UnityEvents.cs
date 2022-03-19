using System;
using System.Collections.Generic;
using UnityEngine;

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
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        
        canCast = true;
        canDoubleJump = false;
        fadeSpriteIterator = 0;
        
        lineRenderer = transform.GetComponent<LineRenderer>();
        lineRenderer.enabled = false;
        
        particleChild = transform.Find("Particles");
        CurrentHealth = MaxHealth;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.GetComponent<Animator>();
        charCollider = transform.GetComponent<BoxCollider2D>();
        
        dust = particleChild.Find("DustPS").GetComponent<ParticleSystem>();
        sliceDashPS = particleChild.Find("SliceDashPS").GetComponent<ParticleSystem>();
        parryPS = particleChild.Find("ParryPS").GetComponent<ParticleSystem>();
        switchPS = particleChild.Find("SwitchPS").GetComponent<ParticleSystem>();
        trailRenderer = particleChild.Find("FX").GetComponent<TrailRenderer>();
        //fadePS = particleChild.Find("FadePS").GetComponent<ParticleSystem>();
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        
        screenShakeController = ScreenShakeController.Instance;
        grappleController = GetComponent<RadialGrapple>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        trailRenderer.emitting = false;
    }
    
    private void FixedUpdate() {
        // Debug.Log("touching" + isWallTouching);
        // Debug.Log("sliding" + isWallSliding);
        
        inputVector = Input.GetAxisRaw("Horizontal");
        if (!IsAbleToMove()) return;
        // movement animations
        Animator.SetInteger(AnimState, Mathf.Abs(moveVector) > float.Epsilon? 2 : 0);
        
        StandardMovement_FixedUpdate();

        // WallJumpDetection_FixedUpdate();
        
        TurnAround_FixedUpdate();
        
    }

    private void StandardMovement_FixedUpdate()
    {
        Vector2 v = Rigidbody.velocity;
        float xVel = v.x;
        float yVel = v.y;
        moveVector = inputVector;
        if (forcedMoveTime > 0)
        {
            moveVector = forcedMoveVector;
            forcedMoveTime -= Time.deltaTime;
        }

        // regular ground movement
        if (isGrounded)
        {
            int moveDir = Math.Sign(moveVector);
            if (moveDir == 0 && Mathf.Abs(xVel) >= MinGroundSpeed)
            {
                // Debug.Log("here: " + xVel);
                int antiMoveDir = -Math.Sign(xVel);

                // TODO change this if we choose to add ice or something
                Rigidbody.AddForce(antiMoveDir * OnGroundDeceleration * Vector2.right, ForceMode2D.Force);
            }
            else if (moveDir != 0)
            {
                Rigidbody.AddForce(moveDir * OnGroundAcceleration * Vector2.right, ForceMode2D.Force);
            }

            // apply max velocity
            if (Mathf.Abs(xVel) > speed) // if newXVel != xVel
            {
                Rigidbody.velocity = new Vector2(Mathf.Clamp(xVel, -speed, speed), yVel);
            }

            // apply min velocity
            if (Mathf.Abs(xVel) < MinGroundSpeed)
            {
                Rigidbody.velocity = new Vector2(0, yVel);
            }
        }
        // in-air movement
        else
        {
            bool isHighVel = Mathf.Abs(xVel) > speed;
            bool isMovingSameDir = Math.Sign(moveVector) == Math.Sign(xVel);

            bool move = !(isRecentlyGrappled && isHighVel && isMovingSameDir);
            bool applyMaxVel = !(isRecentlyGrappled && isHighVel) && !isLineGrappling; //TODO: ??? fix

            if (move && !isWallSliding /*&& wallJumpFramesLeft == 0*/)
            {
                Rigidbody.AddForce(Math.Sign(moveVector) * InAirAcceleration * Vector2.right, ForceMode2D.Force);
            }

            // slow down if player is not inputting horizontal movement (not technically drag)
            if (moveVector == 0 && !grappleController.isGrappling && !isLineGrappling)
            {
                //Debug.Log("drag (ma balls)");
                // drag as a function of current x velocity
                Rigidbody.AddForce(-xVel * InAirDrag * Vector2.right, ForceMode2D.Force);
            }

            if (applyMaxVel)
            {
                Vector2 vel = Rigidbody.velocity;
                Rigidbody.velocity = new Vector2(Mathf.Clamp(vel.x, -speed, speed), yVel);
            }
        }
    }
    
    // private void WallJumpDetection_FixedUpdate() {
    //     if (wallJumpFramesLeft > 0)
    //     {
    //         // Debug.Log("------- WALL JUMPING (FIXED) UPDATE - "
    //         //           + wallJumpFramesLeft + " left, dir = " + wallJumpDir + " -------");
    //         // transform.position += new Vector3((int) wallJumpDir * speed * Time.deltaTime, 0, 0);
    //         Rigidbody.velocity = new Vector2(wallJumpDir * speed, Rigidbody.velocity.y);
    //         wallJumpFramesLeft--;
    //     }
    // }



    private void TurnAround_FixedUpdate() {
        // feet dust logic
        if (Math.Abs(xDir - moveVector) > 0.01f && isGrounded && moveVector != 0) {
            dust.Play();
        }
        
        xDir = moveVector;

        Vector3 scale = transform.localScale;
        // direction switching
        if (moveVector > 0 && Math.Abs(scale.x + 1) > float.Epsilon) {
            FaceRight();
        }
        else if (moveVector < 0 && Math.Abs(scale.x - 1) > float.Epsilon) {
            FaceLeft();
        }
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.P))
            Debug.Log("grounded? " + isGrounded + "; wall sliding? " + isWallSliding);

        if (Input.GetKeyDown(KeyCode.G))
        {
            LaunchLine(FindObjectOfType<GrapplePoint>());
        }
        
        CheckGrounded_Update();
        
        EventHandling_Update();
        
        // jump animation
        if (isGrounded) {
            Animator.SetBool(Jump, false);
        }
        // short jump
        ShortJumpDetection_Update();
        JumpCooldown_Update();
        // wall slide detection
        WallSlideDetection_Update();
        
        FadeParticle_Update();
        // slice-dash detection
        SliceDashDetection_Update();
        LineGrappleUpdate();
        //WallJumpDetection_FixedUpdate();

    }

    private void LineGrappleUpdate()
    {
        if (isGrappleLaunched && sentProjectile != null)
        {
            lineRenderer.SetPosition(1, transform.position);
            lineRenderer.SetPosition(0, sentProjectile.transform.position);
        }
        const float grappleSpeed = 15f;
        const float disconnectDistance = .3f;
        if (isLineGrappling)
        {
            Vector3 direction = (grapplePoint - transform.position).normalized;
            Rigidbody.velocity = direction * grappleSpeed;
            lineRenderer.SetPosition(1, transform.position);
            lineRenderer.SetPosition(0, grapplePoint);
            

            
            if ((grapplePoint - transform.position).magnitude < disconnectDistance)
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
        Vector2 aLittleDown = 2 * groundDistance * Vector2.down;
        
        Debug.DrawLine(bottomLeft, bottomLeft + aLittleDown, Color.magenta);
        Debug.DrawLine(bottomRight, bottomRight + aLittleDown, Color.magenta);

        RaycastHit2D hit1 = Physics2D.Linecast(bottomLeft, bottomLeft + aLittleDown, obstacleLayerMask);
        RaycastHit2D hit2 = Physics2D.Linecast(bottomRight, bottomRight + aLittleDown, obstacleLayerMask);

        bool newlyGrounded = hit1 || hit2;
        if (!isGrounded && newlyGrounded){
            OnLanding();
        }

        isGrounded = newlyGrounded;
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
                slicePoint.position, attackRange, hitColliders, enemyLayers);
            
            if (hitColliders[0] != null) {
                //Debug.Log("execute");
                StartCoroutine(SliceExecuteCoroutine(hitColliders[0].GetComponent<Enemy>()));
            }
        }
    }

    private void ShortJumpDetection_Update() {
        if (Input.GetButtonUp("Jump") && !isGrounded && 
            ((!isInverted && Rigidbody.velocity.y > 0) || (isInverted && Rigidbody.velocity.y < 0))) {
            
            Rigidbody.velocity = Vector2.Scale(Rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }
    private void JumpCooldown_Update()
    {
        //Debug.Log(justJumped);
        if (Input.GetKeyUp(KeyCode.Space) && justJumped)
        {
            canDoubleJump = true;
            justJumped = false;
        }
    }

    private void FadeParticle_Update()
    {
        if (fadeTime > 0)
        {
            const int fadeSpriteLimiter = 23;
            fadeSpriteIterator += 1;
            fadeTime -= Time.deltaTime;
            if (fadeSpriteIterator == fadeSpriteLimiter)
            {
                fadeSpriteIterator = 0;
                GameObject newFadeSprite = Instantiate(fadeSprite, transform.position, transform.rotation);
                newFadeSprite.GetComponent<FadeSprite>()
                    .Initialize(spriteRenderer.sprite, transform.localScale.x < 0);
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
        Vector2 bottomLeft = middleLeft + halfHeight * Vector2.down;
        Vector2 aLittleLeft = 2 * groundDistance * Vector2.left;
        
        // right points
        Vector2 middleRight = center + halfWidth * Vector2.right;
        Vector2 bottomRight = middleRight + halfHeight * Vector2.down;
        Vector2 aLittleRight = 2 * groundDistance * Vector2.right;

        // left linecasts
        RaycastHit2D bottomLeftHit = 
            Physics2D.Linecast(bottomLeft, bottomLeft + aLittleLeft, obstacleLayerMask);
        bool isNearWallOnLeft = bottomLeftHit;

        // right linecasts
        RaycastHit2D bottomRightHit = 
            Physics2D.Linecast(bottomRight, bottomRight + aLittleRight, obstacleLayerMask);
        bool isNearWallOnRight = bottomRightHit;

        isWallSliding = v.y <= 0 && ((moveVector > 0 && isNearWallOnRight) 
                                     || (moveVector < 0 && isNearWallOnLeft)) && IsAbleToMove();

        if (isNearWallOnLeft)
        {
            wallJumpDir = +1;
            canDoubleJump = false;
        }
        else if (isNearWallOnRight)
        {
            wallJumpDir = -1;
            canDoubleJump = false;
        }

        
        if (isWallSliding)
            
            Rigidbody.velocity = new Vector2(v.x, Mathf.Max(v.y, -wallSlideSpeed));
    }
    
}
