using System.Collections;
//using TreeEditor;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CharController
{

    private void DoDash()
    {
        if (!IsAbleToAct() || !isGrounded) {
            return;
        }
        Interrupt();
        
        // float xScale = transform.localScale.x;
        //float dashDir = moveVector == 0 ? -xScale : moveVector;
        float dashDir = inputVector;
        float dashSpeed = DashBoost * dashDir;
        const float dashTime = .23f;
        emitFadesTime = .28f;
        isDashing = true;

        dashCoroutine = DashCoroutine(dashTime /*, dashSpeed*/);
        toInterrupt.Add(dashCoroutine);
        StartCoroutine(dashCoroutine);
        
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
        dust.Play();
        Animator.SetTrigger(Dash);
        lastDashTime = Time.time;
    }

    // dash coroutine handles stopping the dash
    private IEnumerator DashCoroutine(float dashTime /*, float dashSpeed*/)
    {
        UnCrouch();
        ReduceHeight();
        yield return new WaitForSeconds(dashTime);
        //check if space
        isDashing = false;
        // if (isCrouching && CheckSpace())
        // {
        //     UnCrouch();
        // }
        if (CheckSpace())
        {
            ReturnHeight();
        }
        else if (!isCrouching)
        {
            Crouch();
        }
        
    }

    private bool CheckSpace()
    {
        Vector2 relativeUp = isInverted ? Vector2.down : Vector2.up;
        Vector3 bounds = charCollider.bounds.extents;
        float halfWidth = Mathf.Abs(bounds.x);
        float halfHeight = Mathf.Abs(bounds.y);
        Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
        Vector2 topMiddle = center + halfHeight * relativeUp;
        Vector2 topLeft = topMiddle + halfWidth * Vector2.left;
        Vector2 topRight = topMiddle + halfWidth * Vector2.right;
        Vector2 aLittleUp = relativeUp;
        
        Debug.DrawLine(topLeft, topLeft + aLittleUp, Color.magenta);
        Debug.DrawLine(topRight, topRight + aLittleUp, Color.magenta);

        RaycastHit2D hit1 = 
            Physics2D.Linecast(topLeft, topLeft + aLittleUp, obstaclePlusLayerMask);
        RaycastHit2D hit2 = 
            Physics2D.Linecast(topRight, topRight + aLittleUp, obstaclePlusLayerMask);

        return !hit1 && !hit2;
    }

    private void DoJump()
    {
        //GameManager.Instance.TextPop(isJumpBoosted.ToString(), 2f);
        if (Input.GetKey(KeyCode.S) && isPlatformGrounded)
        {
            TileStateManager.Instance.DeactivatePlatforms();
            return;
        }

        if ((!isInverted && Rigidbody.velocity.y > 0.01) || (isInverted && Rigidbody.velocity.y < -0.01))
        {
            return;
        }
        
        //print("jumpcall");
        justJumped = true;
        dust.Play();
        const float wallJumpModX = .7f;
        const float wallJumpModY = 1.4f;
        float xWallJumpBoostedModifier = 3.0f;
        float yWallJumpBoostedModifier = 1.1f;
        Animator.SetBool(Grounded, false);
        Animator.SetTrigger(Jump);
        //float adjustedJumpForce = isJumpBoosted ? jumpForce * 2 : jumpForce;
        Vector2 overallJumpImpulse = new Vector2(0, jumpForce);
        Vector2 momentumVector = Vector2.zero;
        bool doWallJump = (isWallSliding || wallJumpAvailable) && !isGrounded;
        
        if (mostRecentlyTouchedPlatform != null && mostRecentlyTouchedPlatform.type == PlatformType.Moving) //jump from moving platform
        {
            float momentumPercentage = .5f;
            momentumVector = mostRecentlyTouchedPlatform.movingVelocity;
            momentumVector = new Vector2(momentumVector.x * momentumPercentage, momentumVector.y < 0 ? 0 : momentumVector.y * momentumPercentage);
            if (Mathf.Sign(moveVector) != Mathf.Sign(momentumVector.x))
            {
                momentumVector.x = 0;
            }

            if (momentumVector.x != 0)
            {
                recentlyImpulsed = true;
            }
            print(momentumVector);

            
        }
        
        if (doWallJump) //walljump 
        {
            //Debug.Log("WALLJUMP");
            forcedMoveTime = .38f;
            forcedMoveVector = wallJumpDir;
            overallJumpImpulse =
                new Vector2(jumpForce * wallJumpModX * wallJumpDir, overallJumpImpulse.y * wallJumpModY);

        }
        // if (isJumpBoosted)
        // {
        //     emitFadesTime += .3f;
        //     overallJumpImpulse 
        //     
        //     // Rigidbody.AddForce(new Vector2(jumpForce * wallJumpModX * wallJumpDir * xWallJumpBoostedModifier, 
        //     //     (isInverted ? -jumpForce : jumpForce) * wallJumpModY * yWallJumpBoostedModifier), ForceMode2D.Impulse);
        // }
        //
        // if (doWallJump && isJumpBoosted)
        // {
        //     transform.parent = null;
        //     isWallSliding = false;
        //     recentlyImpulsed = true;
        //     float forcedTeleportDistance = .5f;
        //     Vector2 forcedTeleportVector = new Vector2(forcedTeleportDistance * wallJumpDir, 0);
        //     transform.position += (Vector3) forcedTeleportVector;
        // }

        if (isInverted)
        {
            overallJumpImpulse = new Vector2(overallJumpImpulse.x, -overallJumpImpulse.y);
        }

        overallJumpImpulse += momentumVector;
        Rigidbody.AddForce(overallJumpImpulse, ForceMode2D.Impulse);
    }

    private IEnumerator TemporarilyDisablePlatformCollision(Transform parent)
    {
        if (parent == null)
        {
            Debug.LogError("cannot temporarily disable collision on null!");
            yield break;
        }

        BoxCollider2D box = parent.GetComponent<BoxCollider2D>();
        Debug.Log("disabling box collision");
        box.enabled = false;

        yield return new WaitForSeconds(0.25f);

        Debug.Log("re-enabling box collision");
        box.enabled = true;
    }

    
    // private void DoDoubleJump()
    // {
    //     dust.Play();
    //     float doubleJumpForce = JumpForce * .9f;
    //     //canDoubleJump = false;
    //     
    //     Rigidbody.AddForce(new Vector2(0, isInverted ? -doubleJumpForce : doubleJumpForce), ForceMode2D.Impulse);
    //     Animator.SetBool(Grounded, false);
    //     Animator.SetTrigger(Jump);
    // }

    private void ReduceHeight()
    {
        charCollider.size = new Vector2(originalColliderSize.x, originalColliderSize.y / heightReducer);
        // charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y -  
        //                                                             ((originalColliderSize.y - (originalColliderSize.y / heightReducer)) / 2));
        if (isInverted)
        {
            charCollider.offset = new Vector2(originalColliderOffset.x,
                -originalColliderOffset.y + (originalColliderSize.y / 2) * (1 - (1 / heightReducer)));
        }
        else
        {
            charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y -  
                                                                        ((originalColliderSize.y - (originalColliderSize.y / heightReducer)) / 2));
        }

    }

    private void ReturnHeight()
    {
        charCollider.size = originalColliderSize;
        charCollider.offset = new Vector2(originalColliderOffset.x, (isInverted ? -1 : 1) * originalColliderOffset.y);
    }
    

    private void Crouch()
    {
        //Assert.IsTrue(!isCrouching);
        Animator.SetBool("isCrouching", true);
        ReduceHeight();
        isCrouching = true;
        speed = baseSpeed / 2;
    }

    private void UnCrouch()
    {
        //Assert.IsTrue(isCrouching);
        Animator.SetBool("isCrouching", false);
        ReturnHeight();
        speed = baseSpeed;
        isCrouching = false;

    }
    
    private void OnLanding() {
        //canDoubleJump = false;
        justJumped = false;
        isRecentlyGrappled = false;
        dust.Play();
        Animator.SetBool(Grounded, true);
        // Debug.Log("sus");
    }

    private void AttemptLaunchGrapple()
    {
        if (GrapplePoint.TargetPoint != null && !isGrappleLaunched && !isLineGrappling && !grappleBlocked)
        {
            LaunchLine(GrapplePoint.TargetPoint);
        }
    }

    private void LaunchLine(GrapplePoint point)
    {
        StartCoroutine(LaunchLineCoroutine());
        launchedPoint = point;
        isGrappleLaunched = true;
        grappleLineRenderer.enabled = true;
        const float launchSpeed = 25f;
        const float offset = 1f;
        Vector3 direction = (point.transform.position - transform.position).normalized;
        sentProjectile = Instantiate(grappleProjectile, transform.position + (direction * offset), transform.rotation);
        sentProjectile.gameObject.GetComponent<GrappleProjectile>().SetStats(direction, launchSpeed);
    }

    private IEnumerator LaunchLineCoroutine()
    {
        const float maxGrappleLaunchTime = 2f;
        yield return new WaitForSeconds(maxGrappleLaunchTime);
        if (!isLineGrappling)
        {
            launchedPoint = null;
            hookedPoint = null;
            grappleLineRenderer.enabled = false;
            isLineGrappling = false;
            isGrappleLaunched = false;
        }
    }

    public void StartLineGrapple(GrapplePoint point)
    {
        StopCoroutine(LaunchLineCoroutine());
        hookedPoint = point;
        isGrappleLaunched = false;
        grappleLineRenderer.enabled = true;
        isLineGrappling = true;
        isRecentlyGrappled = true;
    }

    private void DisconnectGrapple()
    {
        launchedPoint = null;
        hookedPoint = null;
        //Debug.Log("Disconnect");
        grappleLineRenderer.enabled = false;
        isLineGrappling = false;
        Rigidbody.velocity = Vector2.ClampMagnitude(Rigidbody.velocity, speed * 2);
    }
    
}
