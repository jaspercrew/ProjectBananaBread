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
        float dashSpeed = 9f * dashDir;
        const float dashTime = .23f;

        fadeTime = .28f;
        isDashing = true;
        trailRenderer.emitting = true;
        StartCoroutine(DashCoroutine(dashTime, dashSpeed));
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
        dust.Play();
        Animator.SetTrigger(Dash);
        lastDashTime = Time.time;
    }

    // dash coroutine handles stopping the dash
    private IEnumerator DashCoroutine(float dashTime, float dashSpeed) {
        //Debug.Log(savedVel);
        yield return new WaitForSeconds(dashTime);
        Rigidbody.velocity = new Vector2(Rigidbody.velocity.x - dashSpeed, Rigidbody.velocity.y);
        isDashing = false;
        trailRenderer.emitting = false;
    }

    private void DoJump()
    {
        dust.Play();
        int jumpDir = 0;
        const float horizontalJumpForce = 2f;

        if (isWallSliding && !isGrounded)
        {
            forcedMoveTime = .1f;
            if (wallJumpDir == -1)
            {
                FaceLeft();
                forcedMoveVector = -1;
                jumpDir = -1;
            }
            else if (wallJumpDir == 1)
            {
                FaceRight();
                forcedMoveVector = 1;
                jumpDir = 1;
            }
            else
                Debug.LogError("wall jump dir is bad");
        }
        
        Rigidbody.AddForce(new Vector2(jumpDir * horizontalJumpForce, isInverted ? -JumpForce : JumpForce), ForceMode2D.Impulse);
        Animator.SetBool(Grounded, false);
        Animator.SetTrigger(Jump);
        justJumped = true;

    }

    
    private void DoDoubleJump()
    {
        dust.Play();
        float doubleJumpForce = JumpForce * .9f;
        canDoubleJump = false;
        
        Rigidbody.AddForce(new Vector2(0, isInverted ? -doubleJumpForce : doubleJumpForce), ForceMode2D.Impulse);
        Animator.SetBool(Grounded, false);
        Animator.SetTrigger(Jump);
    }
    

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
        canDoubleJump = false;
        isRecentlyGrappled = false;
        dust.Play();
        Animator.SetBool(Grounded, true);
        // Debug.Log("sus");
    }
}
