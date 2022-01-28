using UnityEngine;
using UnityEngine.Assertions;

public partial class CharController {

    private void DoDash()
    {
        if (!IsAbleToAct() && !isAttacking) {
            return;
        }
        
        // attack cancel
        if (isAttacking)
        {
            Animator.SetTrigger(Idle); // TODO: dash animation
            isAttacking = false;
            Assert.IsNotNull(attackCoroutine);
            StopCoroutine(attackCoroutine);
        }

        const float dashSpeed = 9f;
        const float dashTime = .23f;

        float xScale = transform.localScale.x;
        if (Mathf.Abs(xScale) > .5) {
            VelocityDash(xScale > 0? 3 : 1, dashSpeed, dashTime);
            dust.Play();
            Animator.SetTrigger(Dash);
        }

        lastDashTime = Time.time;
    }

    private void DoJump()
    {
        if (!IsGrounded() && !isWallSliding || !IsAbleToMove())
            return;
        
        dust.Play();

        const int wallJumpFrames = 10;

        if (isWallSliding)
        {
            wallJumpDir = transform.position.x - wallSlidingCollider.transform.position.x > 0? 
                WallJumpDirection.Right : WallJumpDirection.Left;
            wallJumpFramesLeft = wallJumpFrames;
            if (wallJumpDir == WallJumpDirection.Left)
                FaceLeft();
            else if (wallJumpDir == WallJumpDirection.Right)
                FaceRight();
            else
                Debug.LogError("wall jump dir is bad");
        }
        
        Rigidbody.AddForce(new Vector2(0, isInverted ? -JumpForce : JumpForce), ForceMode2D.Impulse);
        Animator.SetTrigger(Jump);
        Animator.SetBool(Grounded, false);
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
        dust.Play();
        Animator.SetBool(Grounded, true);
        // Debug.Log("sus");
    }
}
