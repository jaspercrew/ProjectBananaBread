using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class CharController //CONFIGS
{
    private const float baseSpeed = 12f;
    private const float BoostCooldown = .75f;
    private const float JumpCooldown = .2f;
    
    private const float groundDragThreshholdA = 15f;
    private const float groundDragThreshholdB = 24f;
    private const float airDragThreshholdA = 15f;
    private const float airDragThreshholdB = 30f;
    private const float AbsoluteMaxVelocity = 50f;

    private const float MinGroundSpeed = 0.5f;
    private const float OnGroundAcceleration = 30f;
    private const float OnGroundDeceleration = 40f;
    private const float OnGroundDrag = 5f;
    private const float InAirAcceleration = 30f;
    private const float InAirDrag = 1.0f;
    private const float MaxYSpeed = 30f;

    private const float heightReducer = 3f;

    private const float boostRefreshCooldown = .1f;
    
    private const float inversionForce = 3f;
    private const float jumpForce = 11f;
    public static float BaseGravity = 20f;
    
    private const int rewindSpeed = 5;
    
    public Rigidbody2D projectilePrefab;
    public GameObject fadeSprite;
    
    


    
}
