using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.UI;

public partial class CharController //COMPONENTS
{
    
    private BoxCollider2D charCollider;
    private ParticleSystem dust;
    private ScreenShakeController screenShakeController;
    private Light2D charLight;
    private RadialGrapple grappleController;
    private SpriteRenderer spriteRenderer;
    private Image boostUseIndicator;
    private TrailRenderer dashTrail;
    public Rigidbody2D Rigidbody;
    private Transform particleChild;
    private Transform spriteHandler;

    private ParticleSystem rightWallPS;
    private ParticleSystem leftWallPS;

    private SpriteRenderer leftWallIndicator;
    private SpriteRenderer rightWallIndicator;
    
    public FixedJoint2D fixedJoint2D;
    private LineRenderer grappleLineRenderer;
    private DistanceJoint2D grappleDistanceJoint;

    private void GrabComponents_Awake()
    {
        fixedJoint2D = GetComponent<FixedJoint2D>();
        charLight = transform.Find("Light").GetComponent<Light2D>();
        particleChild = transform.Find("Particles");
        leftWallPS = particleChild.Find("LeftWallDust").GetComponent<ParticleSystem>();
        rightWallPS = particleChild.Find("RightWallDust").GetComponent<ParticleSystem>();
        boostUseIndicator = GetComponentInChildren<Image>();
        charCollider = transform.GetComponent<BoxCollider2D>();
        dashTrail = transform.Find("Particles").Find("DashFX").GetComponent<TrailRenderer>();
        dust = particleChild.Find("FeetDust").GetComponent<ParticleSystem>();
        leftWallIndicator = transform.Find("Borders").Find("LeftIndicator").GetComponent<SpriteRenderer>();
        rightWallIndicator = transform.Find("Borders").Find("RightIndicator").GetComponent<SpriteRenderer>();

        obstacleLayerMask = LayerMask.GetMask("Obstacle");
        obstaclePlusLayerMask = LayerMask.GetMask("Obstacle", "Slide", "Platform");
        wallSlideLayerMask = LayerMask.GetMask("Slide");
        platformLayerMask = LayerMask.GetMask("Platform");
        
        spriteHandler = transform.Find("SpriteHandler");
        spriteRenderer = spriteHandler.GetComponent<SpriteRenderer>();
        
        grappleLineRenderer = GetComponent<LineRenderer>();
        grappleDistanceJoint = GetComponent<DistanceJoint2D>();
    }

    
}
