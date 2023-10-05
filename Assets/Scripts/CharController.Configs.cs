using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharController //CONFIGS
{
    private const float MaxMoveSpeedGround = 10f;
    private const float MaxMoveSpeedAir = 10.5f;
    private const float BoostCooldown = .75f;
    private const float JumpCooldown = .2f;

    private const float GroundDragThreshholdA = 15f;
    private const float GroundDragThreshholdB = 24f;
    private const float AirDragThreshholdA = 15f;
    private const float AirDragThreshholdB = 25f;
    private const float AbsoluteMaxVelocity = 50f;

    private const float MaxSpeedBar = 100f;
    private const float SpeedBarDecayMultiplier = 1f;
    private const float SpeedBarGainMultiplier = 1f;
    private const float SpeedBarVelThreshhold = 20f;
    private const float StartingSpeedBar = 70f;

    private const float MinGroundSpeed = 0.5f;
    private const float OnGroundAcceleration = 30f;
    private const float OnGroundDeceleration = 40f;
    private const float OnGroundDrag = .4f;
    private const float InAirAcceleration = 20f;
    private const float InAirDrag = .01f;
    private const float MaxDownwardSpeedFromGravity = 23f;

    private const float GrappleGravityBoostModifier = .55f;
    private const float MinGrappleVelocity = 15f;
    private const float GrappleVerticalDisplacement = .5f;
    private const float GrappleAcceleration = 1.0015f;
    private const float GrappleLaunchSpeed = 40f;

    const float MaxRotationSpeed = 4f;
    const float MinRotationSpeed = 3f;

    private const float HeightReducer = 3f;

    private const float BoostRefreshCooldown = .1f;

    private const float InversionForce = 3f;
    private const float JumpForce = 11f;
    public static float baseGravity = 20f;

    private const int RewindSpeed = 5;

    public Rigidbody2D projectilePrefab;
    public GameObject fadeSprite;
}
