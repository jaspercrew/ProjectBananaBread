using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public partial class CharController {
    private void Start() {
        particleChild = transform.Find("Particles");
        CurrentHealth = MaxHealth;
        Rigidbody = transform.GetComponent<Rigidbody2D>();
        Animator = transform.GetComponent<Animator>();
        boxCollider = transform.GetComponent<BoxCollider2D>();
        dust = particleChild.Find("DustPS").GetComponent<ParticleSystem>();
        SlicedashPS = particleChild.Find("SliceDashPS").GetComponent<ParticleSystem>();
        screenShakeController = FindObjectOfType<CinemachineVirtualCamera>().GetComponent<ScreenShakeController>();
        grappleController = GetComponent<RadialGrapple>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void FixedUpdate() {
        // Debug.Log("touching" + isWallTouching);
        // Debug.Log("sliding" + isWallSliding);
        
        moveVector = Input.GetAxisRaw("Horizontal");
        WallJumpDetection_FixedUpdate();
        if (!IsAbleToMove()) return;
        // movement animations
        Animator.SetInteger(AnimState, Mathf.Abs(moveVector) > float.Epsilon? 2 : 0);
        
        VelocityOverriding_FixedUpdate();

        TurnAround_FixedUpdate();
        
    }

    private void VelocityOverriding_FixedUpdate() {
        float xVel = Rigidbody.velocity.x;
        if (IsGrounded())
        {
            
            Rigidbody.velocity = new Vector2(moveVector * speed, Rigidbody.velocity.y);
        }
        else
        {
           
            bool isHighVel = Mathf.Abs(xVel) > speed;
            bool isMovingSameDir = Math.Sign(moveVector) == Math.Sign(xVel);

            bool move = !(isRecentlyGrappled && isHighVel && isMovingSameDir);
            bool applyMaxVel = !(isRecentlyGrappled && isHighVel);
            
            if (move && !(isWallSliding || (isWallTouching && Rigidbody.velocity.y > 0))) //causes sticking
            {
                //Debug.Log("before " + Rigidbody.velocity);
                Rigidbody.velocity += moveVector * new Vector2(InAirAcceleration, 0);
                //Debug.Log("after " + Rigidbody.velocity);
            }

            if (applyMaxVel)
            {
                Vector2 vel = Rigidbody.velocity;
                float newXVel = vel.x;
                float yVel = vel.y;
                if (newXVel > speed)
                {
                    Rigidbody.velocity = new Vector2(speed, yVel);
                }
                else if (newXVel < -speed)
                {
                    Rigidbody.velocity = new Vector2(-speed, yVel);
                }
            }
        }
    }

    private void WallJumpDetection_FixedUpdate() {
        if (wallJumpDir != WallJumpDirection.None)
        {
            transform.position += new Vector3((int) wallJumpDir * speed * Time.deltaTime, 0, 0);
            wallJumpFramesLeft--;
            if (wallJumpFramesLeft == 0) {
                wallJumpDir = WallJumpDirection.None;
            }
        }
    }

    private void TurnAround_FixedUpdate() {
        // feet dust logic
        if (Math.Abs(xDir - moveVector) > 0.01f && IsGrounded() && moveVector != 0) {
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
        
        EventHandling_Update();
        
        //jump animation
        if (IsGrounded()) {
            Animator.SetBool(Jump, false);
        }
        // short jump
        ShortJumpDetection_Update();

        //wall slide detection
        WallSlideDetection_Update();
        
        //slicedash detection
        SliceDashDetection_Update();
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
                // continue;
            }
            // execute events whose conditions are met, and remove those whose aren't
            else
            {
                Func<CharController, bool> conditions = EventConditions[e.EventType];
                Action<CharController> actionToDo = EventActions[e.EventType];
                // if (e.EventType == Event.EventTypes.Jump)
                // Debug.Log("jump reached");
                if (conditions.Invoke(this))
                {
                    // if (e.EventType == Event.EventTypes.Jump)
                    // Debug.Log("jump executed");
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
        if (Input.GetButtonUp("Jump") && !IsGrounded() && 
            ((!isInverted && Rigidbody.velocity.y > 0) || (isInverted && Rigidbody.velocity.y < 0))) {
            Rigidbody.velocity = Vector2.Scale(Rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }

    private void WallSlideDetection_Update() {
        const float wallSlideSpeed = 0.75f;
        Vector2 v = Rigidbody.velocity;

        
        // wall sliding
        if (isWallSliding)
        {
            if (v.y <= 0) {
                Rigidbody.velocity = new Vector2(v.x, Mathf.Max(v.y, -wallSlideSpeed));
            }

            if (v.y > 0 || moveVector == 0) {
                //Debug.Log("no longer wallsliding - velocity/movevector check");
                isWallSliding = false;
            }
            
        }
        
        else if (isWallTouching && wallTouchingCollider != null && 
                 Math.Sign(moveVector) == Math.Sign(wallTouchingCollider.transform.position.x - transform.position.x) && Rigidbody.velocity.y <= 0) 
        {
            //Debug.Log("now wallslFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFiding");
            // TODO: snap to wall?
            isWallSliding = true;
        }
        
        
    }
    
    private void OnCollisionEnter2D(Collision2D other)
    {
        //Debug.Log("colenter" + Time.time);
        
        // Grounding Controller
        Collider2D wallCol = other.collider;

        if (wallCol.isTrigger)
            return;
        
        float colX = wallCol.transform.position.x;
        float charX = transform.position.x;
        float colW = wallCol.bounds.extents.x;
        float charW = boxCollider.bounds.extents.x;
        // horizontal distance between char and incoming object
        float dx = Mathf.Abs(charX - colX);
        float maxDx = Mathf.Abs(colW) + Mathf.Abs(charW);
        const float maxWallSlideDistance = 0.03f;

        if (dx < maxDx)
        {
            //Debug.Log("new colliding: " + other.gameObject.name);
            if (colliding.Count == 0) {
                OnLanding();
            }
            colliding.Add(wallCol);
        }
        
        else if (dx < maxDx + maxWallSlideDistance) {
            isWallTouching = true;
            wallTouchingCollider = wallCol;
        }
        
        //wallsliding 
        // else if (dx < maxDx + maxWallSlideDistance && Math.Sign(moveVector) == Math.Sign(colX - charX) && Rigidbody.velocity.y <= 0) 
        // {
        //     //Debug.Log("now wallslFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFiding");
        //     // TODO: snap to wall?
        //     isWallSliding = true;
        // }
    }
    
    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.collider.Equals(wallTouchingCollider))
        {
           // Debug.Log("not wallsliding - exit collider");
            isWallTouching = false;
            isWallSliding = false;
            wallTouchingCollider = null;
            // Debug.Log("stopped wall sliding!");
        }
        else
        {
            colliding.Remove(other.collider);
        }
        
        // if (isSliceDashing && other.gameObject.GetComponent<Enemy>() != null) {
        //     Debug.Log("execute");
        //     StartCoroutine(SliceExecuteCoroutine(other.gameObject.GetComponent<Enemy>()));
        // }
    }
    
    // show gizmos in editor
    private void OnDrawGizmosSelected() {
        if (attackPoint == null) {
            return;
        }
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

}
