using System.Collections;
using UnityEngine;

public partial class CharController
{
    private void DoDash()
    {
        if (!IsAbleToAct() && !isAttacking) {
            return;
        }
        Interrupt();
        
        float xScale = transform.localScale.x;
        //float dashDir = moveVector == 0 ? -xScale : moveVector;
        float dashDir = inputVector;
        float dashSpeed = dashBoost * dashDir;
        const float dashTime = .23f;

        emitFadesTime = .28f;
        isDashing = true;
        trailRenderer.emitting = true;
        
        dashCoroutine = DashCoroutine(dashTime, dashSpeed);
        toInterrupt.Add(dashCoroutine);
        StartCoroutine(dashCoroutine);
        
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
        dust.Play();
        Animator.SetTrigger(Dash);
        lastDashTime = Time.time;
    }

    // dash coroutine handles stopping the dash
    private IEnumerator DashCoroutine(float dashTime, float dashSpeed)
    {
        charCollider.enabled = false;
        yield return new WaitForSeconds(dashTime);
        charCollider.enabled = true;
        //Rigidbody.velocity = new Vector2(Rigidbody.velocity.x - dashSpeed, Rigidbody.velocity.y);
        isDashing = false;
        trailRenderer.emitting = false;
    }

    private void DoJump()
    {
        //print("jumpcall");
        justJumped = true;
        dust.Play();
        const float wallJumpModX = .8f;
        const float wallJumpModY = 1.1f;
        Animator.SetBool(Grounded, false);
        Animator.SetTrigger(Jump);

        if ((isWallSliding || wallJumpAvailable) && !isGrounded)
        {
            //Debug.Log("WALLJUMP");
            forcedMoveTime = .17f;
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
            
            Rigidbody.AddForce(new Vector2(JumpForce * wallJumpModX * wallJumpDir, 
                    (isInverted ? -JumpForce : JumpForce) * wallJumpModY), ForceMode2D.Impulse);
        }
        else
        {
            Rigidbody.AddForce(new Vector2(0, isInverted ? -JumpForce : JumpForce), 
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
    

    // TODO: add variable isCrouching and set to true/false here instead of changing speed directly
    // and use isCrouching in movement and affect speed there
    private void Crouch()
    {
        if (!IsAbleToAct())
            return;
        isCrouching = !isCrouching;
        speed *= isCrouching? 0.5f : 2;
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
        if (GrapplePoint.targetPoint != null && !isGrappleLaunched && !isLineGrappling && !grappleBlocked)
        {
            LaunchLine(GrapplePoint.targetPoint);
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
