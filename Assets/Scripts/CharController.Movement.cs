using System.Collections;
using UnityEngine;
using UnityEngine.Assertions;

public partial class CharController {

    private void DoDash()
    {
        if (!IsAbleToAct() && !isAttacking) {
            return;
        }
        
        //attack cancel
        // if (isAttacking)
        // {
        //     Animator.SetTrigger(Idle); // TODO: dash animation
        //     isAttacking = false;
        //     Assert.IsNotNull(attackCoroutine);
        //     StopCoroutine(attackCoroutine);
        // }
        Interrupt();

        const float dashSpeed = 9f;
        const float dashTime = .23f;

        float xScale = transform.localScale.x;
        VelocityDash(xScale > 0? 3 : 1, dashSpeed, dashTime);
        dust.Play();
        Animator.SetTrigger(Dash);
        

        lastDashTime = Time.time;
    }

    private void DoJump()
    {
        // if (!IsGrounded() && !isWallSliding || !IsAbleToMove()){
        //     return;
        // }
        //Debug.Log("jump run");
        
        // Debug.Log("isGrounded: " + isGrounded + ", so jumping");

        
        dust.Play();

        const int wallJumpFrames = 60;

        if (isWallSliding && !isGrounded)
        {
            Debug.Log("resetting wallJumpFrames to " + wallJumpFrames);
            // Rigidbody.velocity = new Vector2(wallJumpDir * speed, Rigidbody.velocity.y);
            wallJumpFramesLeft = wallJumpFrames;
            if (wallJumpDir == -1)
                FaceLeft();
            else if (wallJumpDir == 1)
                FaceRight();
            else
                Debug.LogError("wall jump dir is bad");
        }
        StartCoroutine(JumpCooldownCoroutine());
        Rigidbody.AddForce(new Vector2(0, isInverted ? -JumpForce : JumpForce), ForceMode2D.Impulse);
        Animator.SetBool(Grounded, false);
        Animator.SetTrigger(Jump);
        
    }

    private IEnumerator JumpCooldownCoroutine()
    {
        //Debug.Log("j-cd run");
        const float jumpCooldown = 0.4f;
        yield return new WaitForSeconds(jumpCooldown);
        canDoubleJump = true;
    }

    private void DoDoubleJump()
    {
        //Debug.Log("double jump");
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
