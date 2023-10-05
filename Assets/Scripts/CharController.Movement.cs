using System;
using System.Collections;
using UnityEngine;

//using TreeEditor;

public partial class CharController
{
    private IEnumerator smoothRotationCoroutineInstance;

    // private void DoDash()
    // {
    //     if (!IsAbleToAct() || !isGrounded) {
    //         return;
    //     }
    //     Interrupt();
    //
    //     // float xScale = transform.localScale.x;
    //     //float dashDir = moveVector == 0 ? -xScale : moveVector;
    //     float dashDir = inputVector;
    //     float dashSpeed = DashBoost * dashDir;
    //     const float dashTime = .23f;
    //     //emitFadesTime = .28f;
    //     isDashing = true;
    //
    //     dashCoroutine = DashCoroutine(dashTime /*, dashSpeed*/);
    //     toInterrupt.Add(dashCoroutine);
    //     StartCoroutine(dashCoroutine);
    //
    //     Rigidbody.velocity = new Vector2(Rigidbody.velocity.x + dashSpeed, Rigidbody.velocity.y);
    //     dust.Play();
    //     //Animator.SetTrigger(Dash);
    //     lastDashTime = Time.time;
    // }
    //
    // // dash coroutine handles stopping the dash
    // private IEnumerator DashCoroutine(float dashTime /*, float dashSpeed*/)
    // {
    //     UnCrouch();
    //     ReduceSize();
    //     yield return new WaitForSeconds(dashTime);
    //     //check if space
    //     isDashing = false;
    //     // if (isCrouching && CheckSpace())
    //     // {
    //     //     UnCrouch();
    //     // }
    //     if (CheckSpace())
    //     {
    //         ReturnSize();
    //     }
    //     else if (!isCrouching)
    //     {
    //         Crouch();
    //     }
    //
    // }

    private void Boost()
    {
        StartCoroutine(BoostCoroutine());
    }

    private IEnumerator BoostCoroutine()
    {
        //float boostFreezeDelay = .0f;
        var isGoldenBoost = false;
        isDashing = true;
        var delayGravTime = .25f;
        var boostForceMultiplier = 4.5f;
        //forcedMoveTime = .3f;
        //recentImpulseTime = .40f;
        if (currentBoostZone != null)
        {
            boostForceMultiplier = 13f;
            isGoldenBoost = true;
        }

        forcedMoveVector = 0;
        recentlyBoosted = true;
        groundedAfterBoost = false;

        var boostDirection = Vector2.zero;
        if (Input.GetKey(KeyCode.A))
            boostDirection = Vector2.left;
        if (Input.GetKey(KeyCode.W))
            boostDirection = Vector2.up;
        if (Input.GetKey(KeyCode.S))
            boostDirection = Vector2.down;
        if (Input.GetKey(KeyCode.D))
            boostDirection = Vector2.right;

        if (boostDirection == Vector2.zero)
            boostDirection = IsFacingLeft() ? Vector2.left : Vector2.right;

        ScreenShakeController.instance.LightShake();
        boostDirection = boostDirection.normalized;
        StartCoroutine(BoostUseVisualEffect());
        //disabledMovement = true;
        lastBoostTime = Time.time;
        //emitFadesTime = .28f;
        //Animator.SetTrigger(Dash);
        gravityValue = 0;
        boostDirection.Scale(new Vector2(1.4f, 1.4f));
        var boost = boostDirection * boostForceMultiplier;
        dashTrailEmitTime = .3f;

        if (Math.Sign(rigidbody.velocity.x) != Math.Sign(boost.x))
        {
            rigidbody.velocity = new Vector2(0, rigidbody.velocity.y);
            boost *= 1.5f;
        }

        if (Math.Sign(rigidbody.velocity.y) != Math.Sign(boost.y))
        {
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, 0);
            boost *= 1.5f;
        }

        // if (Math.Sign(Rigidbody.velocity.x) != Math.Sign(boost.x) && boost.x != 0)
        // {
        //     Rigidbody.velocity = new Vector2(0, Rigidbody.velocity.y);
        // }
        // if (Math.Sign(Rigidbody.velocity.y) != Math.Sign(boost.y) && boost.y != 0)
        // {
        //     Rigidbody.velocity = new Vector2(Rigidbody.velocity.x, 0);
        // }

        rigidbody.velocity += boost * 1f;

        yield return new WaitForSeconds(delayGravTime);

        var doVelSubtraction = true;
        if (
            Math.Sign(rigidbody.velocity.x) == Math.Sign(rigidbody.velocity.x - boost.x)
            && boostDirection.x != 0
            && doVelSubtraction
        )
        {
            //float xToSet = Rigidbody.velocity.x - (boost.x * .8f);
            if (IsGrounded)
            {
                if (rigidbody.velocity.x > 0)
                    rigidbody.velocity = new Vector2(
                        Math.Min(rigidbody.velocity.x, GroundDragThreshholdA),
                        rigidbody.velocity.y
                    );
                else
                    rigidbody.velocity = new Vector2(
                        Math.Max(rigidbody.velocity.x, -GroundDragThreshholdA),
                        rigidbody.velocity.y
                    );
            }
            else
            {
                if (rigidbody.velocity.x > 0)
                    rigidbody.velocity = new Vector2(
                        Math.Min(rigidbody.velocity.x, AirDragThreshholdB),
                        rigidbody.velocity.y
                    );
                else
                    rigidbody.velocity = new Vector2(
                        Math.Max(rigidbody.velocity.x, -AirDragThreshholdB),
                        rigidbody.velocity.y
                    );
            }
        }

        //disabledMovement = false;
        gravityValue = baseGravity;
        if (isGoldenBoost)
            recentlyBoosted = false;

        isDashing = false;
    }

    public void BoostRefresh()
    {
        StartCoroutine(BoostRefreshCoroutine());
    }

    private IEnumerator BoostRefreshCoroutine()
    {
        groundedAfterBoost = true;
        //print("boost visual effect: refresh");
        var original = boostUseIndicator.color;
        var destination = originalBoostVisualColor;
        var elapsedTime = 0f;
        var fadeTime = .3f;

        while (elapsedTime < fadeTime)
        {
            boostUseIndicator.color = Color.Lerp(original, destination, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        boostUseIndicator.color = destination;
    }

    private IEnumerator BoostUseVisualEffect()
    {
        var original = boostUseIndicator.color;
        var destination = Color.gray;
        var elapsedTime = 0f;
        var fadeTime = .3f;

        while (elapsedTime < fadeTime)
        {
            boostUseIndicator.color = Color.Lerp(original, destination, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        boostUseIndicator.color = destination;
    }

    private bool CheckSpace()
    {
        var relativeUp = isInverted ? Vector2.down : Vector2.up;
        var bounds = charCollider.bounds.extents;
        var halfWidth = Mathf.Abs(bounds.x);
        var halfHeight = Mathf.Abs(bounds.y);
        var center = (Vector2) transform.position + charCollider.offset.y * Vector2.up;
        var topMiddle = center + halfHeight * relativeUp;
        var topLeft = topMiddle + halfWidth * Vector2.left;
        var topRight = topMiddle + halfWidth * Vector2.right;
        var aLittleUp = relativeUp;

        Debug.DrawLine(topLeft, topLeft + aLittleUp, Color.magenta);
        Debug.DrawLine(topRight, topRight + aLittleUp, Color.magenta);

        var hit1 = Physics2D.Linecast(topLeft, topLeft + aLittleUp, obstaclePlusLayerMask);
        var hit2 = Physics2D.Linecast(topRight, topRight + aLittleUp, obstaclePlusLayerMask);

        return !hit1 && !hit2;
    }

    private void DoJump()
    {
        //GameManager.Instance.TextPop(isJumpBoosted.ToString(), 2f);
        if (Input.GetKey(KeyCode.S) && isPlatformGrounded)
        {
            TileStateManager.instance.DeactivatePlatforms();
            return;
        }

        lastJumpTime = Time.time;
        //print("jumpcall");
        justJumped = true;
        dust.Play();
        const float wallJumpModX = .55f;
        const float wallJumpModY = 1.2f;

        var overallJumpImpulse = new Vector2(0, JumpForce);
        var momentumVector = Vector2.zero;
        //bool doWallJump = (isWallSliding || wallJumpAvailable) && !isGrounded;
        var doWallJump = isNearWallOnLeft || isNearWallOnRight;
        var momentumBreakpoint = 10f;
        if (
            mostRecentlyTouchedPlatform != null
            && mostRecentlyTouchedPlatform.type == PlatformType.Moving
        ) //jump from moving platform
        {
            var momentumPercentage = .5f;

            momentumVector = mostRecentlyTouchedPlatform.movingVelocity;
            momentumVector = new Vector2(
                momentumVector.x * momentumPercentage,
                momentumVector.y < 0 ? 0 : momentumVector.y * momentumPercentage
            );
            if (Mathf.Sign(moveVector) != Mathf.Sign(momentumVector.x))
                //print("wrong sign, set momentum to 0");
                momentumVector.x = 0;

            if (momentumVector.magnitude > momentumBreakpoint)
            {
                recentImpulseTime = .1f;
                emitFadesTime = .4f;
            }

            print("momentum vector: " + momentumVector);
        }

        if (doWallJump) //walljump
        {
            //Debug.Log("WALLJUMP");
            forcedMoveTime = .30f;
            dashTrailEmitTime = .3f;
            forcedMoveVector = wallJumpDir;
            overallJumpImpulse = new Vector2(
                JumpForce * wallJumpModX * wallJumpDir + wallJumpDir * rigidbody.velocity.x,
                Math.Max(Math.Max(overallJumpImpulse.y * wallJumpModY, 0), rigidbody.velocity.y)
            );
            rigidbody.velocity = Vector2.zero;
        }
        else if (!IsGrounded)
        {
            if (lastJumpTime + JumpCooldown < Time.time)
                return;
            rigidbody.velocity = new Vector2(
                rigidbody.velocity.x,
                Math.Max(rigidbody.velocity.y, 0)
            );
            doubleJumpAvailable = false;
        }
        else
        {
            //regular jump
            if (lastJumpTime + JumpCooldown < Time.time)
                return;

            if (isInverted)
                overallJumpImpulse = new Vector2(overallJumpImpulse.x, -overallJumpImpulse.y);

            if (momentumVector.magnitude > momentumBreakpoint)
                overallJumpImpulse += momentumVector;
            else
                overallJumpImpulse += momentumVector / 2;
        }

        //print("impulse: " + overallJumpImpulse);
        //savedRotationalVelocity = 5f * Math.Sign(Rigidbody.velocity.x);
        StartCoroutine(JumpRotationDelay(doWallJump));
        rigidbody.velocity += overallJumpImpulse;
    }

    private IEnumerator JumpRotationDelay(bool wallJump = false)
    {
        if (wallJump)
        {
            savedRotationalVelocity = -.5f * wallJumpDir;
            //savedRotationalVelocity = Mathf.Clamp(savedRotationalVelocity, -maxRotationSpeed, maxRotationSpeed);
            yield return new WaitForSeconds(.05f);
            SmoothRotationEnd();
        }
        else
        {
            savedRotationalVelocity =
                .5f * (IsFacingLeft() ? 1 : -1) * Math.Abs(rigidbody.velocity.x);
            //savedRotationalVelocity = Mathf.Clamp(savedRotationalVelocity, -maxRotationSpeed, maxRotationSpeed);
            yield return new WaitForSeconds(.05f);
            SmoothRotationEnd();
        }
    }

    // private IEnumerator TemporarilyDisablePlatformCollision(Transform parent)
    // {
    //     if (parent == null)
    //     {
    //         Debug.LogError("cannot temporarily disable collision on null!");
    //         yield break;
    //     }
    //
    //     BoxCollider2D box = parent.GetComponent<BoxCollider2D>();
    //     Debug.Log("disabling box collision");
    //     box.enabled = false;
    //
    //     yield return new WaitForSeconds(0.25f);
    //
    //     Debug.Log("re-enabling box collision");
    //     box.enabled = true;
    // }


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

    private void ReduceSize()
    {
        charCollider.size = new Vector2(
            originalColliderSize.x / HeightReducer,
            originalColliderSize.y / HeightReducer
        );
        // charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y -
        //                                                             ((originalColliderSize.y - (originalColliderSize.y / heightReducer)) / 2));
        // if (isInverted)
        // {
        //     charCollider.offset = new Vector2(originalColliderOffset.x,
        //         -originalColliderOffset.y + (originalColliderSize.y / 2) * (1 - (1 / heightReducer)));
        // }
        // else
        // {
        //     charCollider.offset = new Vector2(originalColliderOffset.x, originalColliderOffset.y -
        //                                                                 (shiftMiddle ? 0 : 1) * ((originalColliderSize.y - (originalColliderSize.y / heightReducer)) / 2));
        // }
    }

    private void ReturnSize()
    {
        charCollider.size = originalColliderSize;
        //charCollider.offset = new Vector2(originalColliderOffset.x, (isInverted ? -1 : 1) * originalColliderOffset.y);
    }

    private void Crouch()
    {
        //Assert.IsTrue(!isCrouching);
        //Animator.SetBool("isCrouching", true);
        ReduceSize();
        isCrouching = true;
        runSpeed = MaxMoveSpeedGround / 2;
    }

    private void SmoothRotationEnd()
    {
        smoothRotationCoroutineInstance = SmoothRotationEndCoroutine();
        StartCoroutine(smoothRotationCoroutineInstance);
    }

    private IEnumerator SmoothRotationEndCoroutine()
    {
        while (!isGrappling)
        {
            //print("continue rotation");
            //print("before clamp: " + savedRotationalVelocity);
            //savedRotationalVelocity /= 3f;
            //savedRotationalVelocity = savedRotationalVelocity > 0 ? Math.Max(savedRotationalVelocity, maxRotationSpeed) : Math.Min(savedRotationalVelocity, -maxRotationSpeed);
            savedRotationalVelocity = Mathf.Clamp(
                savedRotationalVelocity,
                -MaxRotationSpeed,
                MaxRotationSpeed
            );
            if (savedRotationalVelocity != 0)
                savedRotationalVelocity =
                    savedRotationalVelocity > 0
                        ? Math.Max(savedRotationalVelocity, MinRotationSpeed)
                        : Math.Min(savedRotationalVelocity, -MinRotationSpeed);

            //print("after clamp: " + savedRotationalVelocity);
            spriteHandler.Rotate(Vector3.forward, savedRotationalVelocity);
            if (IsGrounded || isNearWallOnLeft || isNearWallOnRight)
            {
                ForceRotationEnd();
                break;
            }

            yield return new WaitForFixedUpdate();
        }
    }

    private void ForceRotationEnd()
    {
        //GameManager.Instance.TextPop("forced rotation end");

        if (smoothRotationCoroutineInstance != null)
            StopCoroutine(smoothRotationCoroutineInstance);

        spriteHandler.rotation = Quaternion.Euler(Vector3.zero);
    }

    private void UnCrouch()
    {
        //Assert.IsTrue(isCrouching);
        //Animator.SetBool("isCrouching", false);
        ReturnSize();
        runSpeed = MaxMoveSpeedGround;
        isCrouching = false;
    }

    private void OnGroundLanding()
    {
        //canDoubleJump = false;
        if (isGrappling)
            EndGrapple();

        //TryRefreshBoost();
        doubleJumpAvailable = true;
        justJumped = false;
        dust.Play();
        ForceRotationEnd();
    }

    private void OnWallLanding()
    {
        // if (isGrappling)
        // {
        //     EndGrapple();
        // }
        if (rigidbody.velocity.y < 0)
            rigidbody.velocity = new Vector2(rigidbody.velocity.x, rigidbody.velocity.y / 2);

        doubleJumpAvailable = true;
        StartCoroutine(BoostRefreshCoroutine());
        //TryRefreshBoost();

        ForceRotationEnd();
    }

    // private void TryRefreshBoost()
    // {
    //     if (lastBoostTime + boostRefreshCooldown < Time.time)
    //     {
    //         recentlyBoosted = false;
    //     }
    // }

    // private void AttemptLaunchGrapple()
    // {
    //     if (GrapplePoint.TargetPoint != null && !isGrappleLaunched && !isLineGrappling && !grappleBlocked)
    //     {
    //         LaunchLine(GrapplePoint.TargetPoint);
    //     }
    // }
    //
    // private void LaunchLine(GrapplePoint point)
    // {
    //     StartCoroutine(LaunchLineCoroutine());
    //     launchedPoint = point;
    //     isGrappleLaunched = true;
    //     grappleLineRenderer.enabled = true;
    //     const float launchSpeed = 25f;
    //     const float offset = 1f;
    //     Vector3 direction = (point.transform.position - transform.position).normalized;
    //     sentProjectile = Instantiate(grappleProjectile, transform.position + (direction * offset), transform.rotation);
    //     sentProjectile.gameObject.GetComponent<GrappleProjectile>().Initialize(direction, launchSpeed);
    // }
    //
    // private IEnumerator LaunchLineCoroutine()
    // {
    //     const float maxGrappleLaunchTime = 2f;
    //     yield return new WaitForSeconds(maxGrappleLaunchTime);
    //     if (!isLineGrappling)
    //     {
    //         launchedPoint = null;
    //         hookedPoint = null;
    //         grappleLineRenderer.enabled = false;
    //         isLineGrappling = false;
    //         isGrappleLaunched = false;
    //     }
    // }
    //
    // public void StartLineGrapple(GrapplePoint point)
    // {
    //     StopCoroutine(LaunchLineCoroutine());
    //     hookedPoint = point;
    //     isGrappleLaunched = false;
    //     grappleLineRenderer.enabled = true;
    //     isLineGrappling = true;
    //     isRecentlyGrappled = true;
    // }
    //
    // private void DisconnectGrapple()
    // {
    //     launchedPoint = null;
    //     hookedPoint = null;
    //     //Debug.Log("Disconnect");
    //     grappleLineRenderer.enabled = false;
    //     isLineGrappling = false;
    //     Rigidbody.velocity = Vector2.ClampMagnitude(Rigidbody.velocity, runSpeed * 2);
    // }
}