using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CharController
{

    private void DoDash()
    {
        if (!IsAbleToAct() && !isAttacking) {
            return;
        }
        Interrupt();
        
        // float xScale = transform.localScale.x;
        //float dashDir = moveVector == 0 ? -xScale : moveVector;
        float dashDir = inputVector;
        float dashSpeed = DashBoost * dashDir;
        const float dashTime = .23f;

        emitFadesTime = .28f;
        IsDashing = true;
        trailRenderer.emitting = true;
        
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
        IsDashing = false;
        trailRenderer.emitting = false;
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
        Vector3 bounds = charCollider.bounds.extents;
        float halfWidth = Mathf.Abs(bounds.x);
        float halfHeight = Mathf.Abs(bounds.y);
        Vector2 center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
        Vector2 topMiddle = center + halfHeight * Vector2.up;
        Vector2 topLeft = topMiddle + halfWidth * Vector2.left;
        Vector2 topRight = topMiddle + halfWidth * Vector2.right;
        Vector2 aLittleUp =  Vector2.up;
        
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
        if (Input.GetKey(KeyCode.S) && isPlatformGrounded)
        {
            TileStateManager.Instance.DeactivatePlatforms();
            return;
        }

        if (Rigidbody.velocity.y > 0.01)
        {
            return;
        }
        
        //print("jumpcall");
        justJumped = true;
        dust.Play();
        const float wallJumpModX = .8f;
        const float wallJumpModY = 1.2f;
        Animator.SetBool(Grounded, false);
        Animator.SetTrigger(Jump);

        if ((isWallSliding || wallJumpAvailable) && !isGrounded)
        {
            //Debug.Log("WALLJUMP");
            forcedMoveTime = .28f;
            if (wallJumpDir == -1)
            {
                //FaceLeft();
                forcedMoveVector = -1;
            }
            else if (wallJumpDir == 1)
            {
                //FaceRight();
                forcedMoveVector = 1;
            }
            else
            {
                Debug.LogError("wall jump dir is bad");
            }
            
            Rigidbody.AddForce(new Vector2(jumpForce * wallJumpModX * wallJumpDir, 
                    (isInverted ? -jumpForce : jumpForce) * wallJumpModY), ForceMode2D.Impulse);
        }
        else
        {
            Rigidbody.AddForce(new Vector2(0, isInverted ? -jumpForce : jumpForce), 
                ForceMode2D.Impulse); 
        }
        
        
        justJumped = true;
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
        charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y - 
                                                                    ((originalColliderSize.y - (originalColliderSize.y / heightReducer)) / 2));
    }

    private void ReturnHeight()
    {
        charCollider.size = originalColliderSize;
        charCollider.offset = originalColliderOffset;
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
