using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;
using UnityEngine.Serialization;
using UnityEngine.UI;

public partial class CharController //COMPONENTS
{
    [FormerlySerializedAs("Rigidbody")] public Rigidbody2D rigidbody;

    public FixedJoint2D fixedJoint2D;
    private Transform backgroundSpeedSprite;
    private Image boostUseIndicator;
    private BoxCollider2D charCollider;
    private Text charDebugText;
    private Light2D charLight;
    private TrailRenderer dashTrail;
    private ParticleSystem dust;
    private DistanceJoint2D grappleDistanceJoint;
    private LineRenderer grappleLineRenderer;

    private SpriteRenderer leftWallIndicator;
    private ParticleSystem leftWallPS;
    private Transform mainSpeedSprite;
    private Transform particleChild;
    private SpriteRenderer rightWallIndicator;

    private ParticleSystem rightWallPS;
    private ScreenShakeController screenShakeController;
    private Transform spriteHandler;

    // private RadialGrapple grappleController;
    private SpriteRenderer spriteRenderer;

    private void GrabComponents_Awake()
    {
        charDebugText = transform.Find("DebugText").GetComponentInChildren<Text>();
        fixedJoint2D = GetComponent<FixedJoint2D>();
        charLight = transform.Find("Light").GetComponent<Light2D>();
        particleChild = transform.Find("Particles");
        leftWallPS = particleChild.Find("LeftWallDust").GetComponent<ParticleSystem>();
        rightWallPS = particleChild.Find("RightWallDust").GetComponent<ParticleSystem>();
        boostUseIndicator = GetComponentInChildren<Image>();
        charCollider = transform.GetComponent<BoxCollider2D>();
        dashTrail = transform.Find("Particles").Find("DashFX").GetComponent<TrailRenderer>();
        dust = particleChild.Find("FeetDust").GetComponent<ParticleSystem>();
        leftWallIndicator = transform
            .Find("Borders")
            .Find("LeftIndicator")
            .GetComponent<SpriteRenderer>();
        rightWallIndicator = transform
            .Find("Borders")
            .Find("RightIndicator")
            .GetComponent<SpriteRenderer>();

        backgroundSpeedSprite = transform.Find("SpeedIndicator").Find("BackgroundSpeedSprite");
        mainSpeedSprite = transform.Find("SpeedIndicator").Find("MainSpeedSprite");

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