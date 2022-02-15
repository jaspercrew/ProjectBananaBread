using System;
using System.Collections.Generic;
using UnityEditor.VersionControl;
using UnityEngine;

public partial class CharController {
    private void Start()
    {
        canDoubleJump = false;
        
        particleChild = transform.Find("Particles");
        CurrentHealth = MaxHealth;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.GetComponent<Animator>();
        charCollider = transform.GetComponent<BoxCollider2D>();
        
        dust = particleChild.Find("DustPS").GetComponent<ParticleSystem>();
        sliceDashPS = particleChild.Find("SliceDashPS").GetComponent<ParticleSystem>();
        parryPS = particleChild.Find("ParryPS").GetComponent<ParticleSystem>();
        switchPS = particleChild.Find("SwitchPS").GetComponent<ParticleSystem>();
        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        
        screenShakeController = ScreenShakeController.Instance;
        grappleController = GetComponent<RadialGrapple>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void FixedUpdate() {
        // Debug.Log("touching" + isWallTouching);
        // Debug.Log("sliding" + isWallSliding);
        
        moveVector = Input.GetAxisRaw("Horizontal");
        if (!IsAbleToMove()) return;
        // movement animations
        Animator.SetInteger(AnimState, Mathf.Abs(moveVector) > float.Epsilon? 2 : 0);
        
        MovementAndVelocityOverriding_FixedUpdate();

        // WallJumpDetection_FixedUpdate();
        
        TurnAround_FixedUpdate();
        
    }

    private void MovementAndVelocityOverriding_FixedUpdate() {
        float xVel = Rigidbody.velocity.x;
        // regular ground movement
        if (isGrounded)
        {
            Rigidbody.velocity = new Vector2(moveVector * speed, Rigidbody.velocity.y);
            // Debug.Log("setting vel in movement");
        }
        // in-air movement
        else
        {
            bool isHighVel = Mathf.Abs(xVel) > speed;
            bool isMovingSameDir = Math.Sign(moveVector) == Math.Sign(xVel);

            bool move = !(isRecentlyGrappled && isHighVel && isMovingSameDir);
            bool applyMaxVel = !(isRecentlyGrappled && isHighVel);
            
            if (move && !isWallSliding /*&& wallJumpFramesLeft == 0*/)
            {
                Rigidbody.velocity += moveVector * new Vector2(InAirAcceleration, 0);
            }

            if (moveVector == 0 && !isRecentlyGrappled)
            {
                Rigidbody.velocity = new Vector2(Rigidbody.velocity.x * .8f, Rigidbody.velocity.y);
            }

            if (applyMaxVel)
            {
                Vector2 vel = Rigidbody.velocity;
                float newXVel = vel.x;
                float yVel = vel.y;
                // if (newXVel > speed)
                // {
                //     Rigidbody.velocity = new Vector2(speed, yVel);
                // }
                // else if (newXVel < -speed)
                // {
                //     Rigidbody.velocity = new Vector2(-speed, yVel);
                // }

                // simpler:
                // if (Mathf.Abs(newXVel) > speed)
                // {
                //     Rigidbody.velocity = new Vector2(Mathf.Sign(newXVel) * speed, yVel);
                // }
                
                // simplest:
                Rigidbody.velocity = new Vector2(Mathf.Clamp(newXVel, -speed, speed), yVel);
            }
            
        }
    }

    private void WallJumpDetection_FixedUpdate() {
        if (wallJumpFramesLeft > 0)
        {
            // Debug.Log("------- WALL JUMPING (FIXED) UPDATE - "
            //           + wallJumpFramesLeft + " left, dir = " + wallJumpDir + " -------");
            // transform.position += new Vector3((int) wallJumpDir * speed * Time.deltaTime, 0, 0);
            Rigidbody.velocity = new Vector2(wallJumpDir * speed, Rigidbody.velocity.y);
            // Debug.Log("setting velocity in wall jumping");
            wallJumpFramesLeft--;
        }
    }

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
        
        CheckGrounded_Update();
        
        EventHandling_Update();
        
        // jump animation
        if (isGrounded) {
            Animator.SetBool(Jump, false);
        }
        // short jump
        ShortJumpDetection_Update();

        // wall slide detection
        WallSlideDetection_Update();
        
        // slice-dash detection
        SliceDashDetection_Update();
        
        WallJumpDetection_FixedUpdate();

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

            // if (numHitEnemies > 0) {
            //     // pause swing animation if an enemy is hit
            //     StartCoroutine(PauseAnimatorCoroutine(hitConfirmDelay));
            //     screenShakeController.MediumShake();
            // }
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
        RaycastHit2D topLeftHit = 
            Physics2D.Linecast(topLeft, topLeft + aLittleLeft, obstacleLayerMask);
        RaycastHit2D bottomLeftHit = 
            Physics2D.Linecast(bottomLeft, bottomLeft + aLittleLeft, obstacleLayerMask);
        bool isNearWallOnLeft = topLeftHit || bottomLeftHit;

        // right linecasts
        RaycastHit2D topRightHit = 
            Physics2D.Linecast(topRight, topRight + aLittleRight, obstacleLayerMask);
        RaycastHit2D bottomRightHit = 
            Physics2D.Linecast(bottomRight, bottomRight + aLittleRight, obstacleLayerMask);
        bool isNearWallOnRight = topRightHit || bottomRightHit;

        Debug.DrawLine(topLeft, topLeft + aLittleLeft, Color.magenta);
        Debug.DrawLine(bottomLeft, bottomLeft + aLittleLeft, Color.magenta);
        Debug.DrawLine(topRight, topRight + aLittleRight, Color.magenta);
        Debug.DrawLine(bottomRight, bottomRight + aLittleRight, Color.magenta);

        isWallSliding = v.y <= 0 && ((moveVector > 0 && isNearWallOnRight) || (moveVector < 0 && isNearWallOnLeft));

        if (isNearWallOnLeft)
        {
            // Collider2D hit1 = topLeftHit.collider;
            // Collider2D hit2 = bottomLeftHit.collider;
            // wallTouchingCollider = hit1 ? hit1 : hit2;
            wallJumpDir = +1;
        }
        else if (isNearWallOnRight)
        {
            // Collider2D hit1 = topRightHit.collider;
            // Collider2D hit2 = bottomRightHit.collider;
            // wallTouchingCollider = hit1 ? hit1 : hit2;
            wallJumpDir = -1;
        }
        else
        {
            // wallTouchingCollider = null;
            // wallJumpDir = 0;
        }


        // wall sliding
        // if (isWallSliding)
        // {
        //     if (v.y <= 0) {
        //         Rigidbody.velocity = new Vector2(v.x, Mathf.Max(v.y, -wallSlideSpeed));
        //     }
        //
        //     if (v.y > 0 || moveVector == 0) {
        //         //Debug.Log("no longer wall sliding - velocity/move vector check");
        //         isWallSliding = false;
        //     }
        //     
        // }
        
        // else if (isWallTouching && wallTouchingCollider != null && 
        //          Math.Sign(moveVector) == 
        //          Math.Sign(wallTouchingCollider.transform.position.x - transform.position.x) 
        //          && Rigidbody.velocity.y <= 0) 
        // {
        //     //Debug.Log("now wall sliding");
        //     // TODO: snap to wall?
        //     isWallSliding = true;
        // }
        
        if (isWallSliding)
            Rigidbody.velocity = new Vector2(v.x, Mathf.Max(v.y, -wallSlideSpeed));
    }
    
    // private void OnCollisionEnter2D(Collision2D other)
    // {
    //     //Debug.Log("col enter" + Time.time);
    //     
    //     // Grounding Controller
    //     Collider2D wallCol = other.collider;
    //
    //     if (wallCol.isTrigger)
    //         return;
    //     
    //     float colX = wallCol.transform.position.x;
    //     float charX = transform.position.x;
    //     float colW = wallCol.bounds.extents.x;
    //     float charW = boxCollider.bounds.extents.x;
    //     // horizontal distance between char and incoming object
    //     float dx = Mathf.Abs(charX - colX);
    //     float maxDx = Mathf.Abs(colW) + Mathf.Abs(charW);
    //     const float maxWallSlideDistance = 0.03f;
    //
    //     if (dx < maxDx)
    //     {
    //         //Debug.Log("new colliding: " + other.gameObject.name);
    //         if (colliding.Count == 0) {
    //             OnLanding();
    //         }
    //         colliding.Add(wallCol);
    //     }
    //     
    //     else if (dx < maxDx + maxWallSlideDistance) {
    //         canDoubleJump = false;
    //         isWallTouching = true;
    //         wallTouchingCollider = wallCol;
    //     }
    //     
    //     // wall sliding 
    // else if (dx < maxDx + maxWallSlideDistance && Math.Sign(moveVector) == Math.Sign(colX - charX) 
    //          && Rigidbody.velocity.y <= 0) 
    //     // {
    //     //     //Debug.Log("now wall sliding");
    //     //     // TODO: snap to wall?
    //     //     isWallSliding = true;
    //     // }
    // }
    
    // private void OnCollisionExit2D(Collision2D other)
    // {
    //     if (other.collider.Equals(wallTouchingCollider))
    //     {
    //        // Debug.Log("not wall sliding - exit collider");
    //         isWallTouching = false;
    //         isWallSliding = false;
    //         wallTouchingCollider = null;
    //         // Debug.Log("stopped wall sliding!");
    //     }
    //     else
    //     {
    //         colliding.Remove(other.collider);
    //     }
    //     
    //     // if (isSliceDashing && other.gameObject.GetComponent<Enemy>() != null) {
    //     //     Debug.Log("execute");
    //     //     StartCoroutine(SliceExecuteCoroutine(other.gameObject.GetComponent<Enemy>()));
    //     // }
    // }

    // show gizmos in editor
    
    private void OnDrawGizmosSelected() {
        if (attackPoint != null) {
            Gizmos.DrawWireSphere(attackPoint.position, attackRange);
        }

        Gizmos.color = Color.magenta;
        Gizmos.DrawWireCube(transform.position + (Vector3) charCollider.offset, charCollider.size);
    }

}
