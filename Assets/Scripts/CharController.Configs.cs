using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharController //CONFIGS
{
    private const float maxMoveSpeedGround = 10f;
    private const float maxMoveSpeedAir = 10.5f;
    private const float BoostCooldown = .75f;
    private const float JumpCooldown = .2f;
    
    private const float groundDragThreshholdA = 15f;
    private const float groundDragThreshholdB = 24f;
    private const float airDragThreshholdA = 20f;
    private const float airDragThreshholdB = 30f;
    private const float AbsoluteMaxVelocity = 50f;

    private const float MinGroundSpeed = 0.5f;
    private const float OnGroundAcceleration = 30f;
    private const float OnGroundDeceleration = 40f;
    private const float OnGroundDrag = .2f;
    private const float InAirAcceleration = 20f;
    private const float InAirDrag = .05f;
    private const float MaxDownwardSpeedFromGravity = 23f;
    
    const float maxRotationSpeed = 4f;
    const float minRotationSpeed = 3f;

    private const float heightReducer = 3f;

    private const float boostRefreshCooldown = .1f;
    
    private const float inversionForce = 3f;
    private const float jumpForce = 11f;
    public static float BaseGravity = 20f;
    
    private const int rewindSpeed = 5;
    
    public Rigidbody2D projectilePrefab;
    public GameObject fadeSprite;
    
    


    
}
