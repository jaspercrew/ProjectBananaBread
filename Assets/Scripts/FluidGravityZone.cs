/*using UnityEngine;

public class FluidGravityZone : ActivatedEntity // active = inverted gravity
{
    public enum GravityDirection
    {
        North,
        South,
        East,
        West,
        None
    }

    private const float ParticleDensity = 2f;
    private const float EdgeGap = 1f;

    public GravityDirection inactiveGravityDirection;
    public GravityDirection activeGravityDirection;

    // Start is called before the first frame update
    private BoxCollider2D boxCollider2D;
    private ParticleSystem gravParticleSystem;
    private float lowerYBound;
    private ParticleSystem.MainModule mainModule;
    private ParticleSystem.ShapeModule shapeModule;
    private SpriteRenderer spriteRenderer;
    private ParticleSystem.VelocityOverLifetimeModule velocityModule;

    protected override void Start()
    {
        boxCollider2D = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        gravParticleSystem = GetComponent<ParticleSystem>();

        lowerYBound = -boxCollider2D.size.y / 2;
        shapeModule = gravParticleSystem.shape;
        shapeModule.radius = boxCollider2D.size.x / 2 - EdgeGap;
        shapeModule.position = new Vector3(0, lowerYBound, 0);

        var emission = gravParticleSystem.emission;

        var burst = emission.GetBurst(0);
        burst.count = new ParticleSystem.MinMaxCurve(shapeModule.radius * ParticleDensity);
        emission.SetBurst(0, burst);

        velocityModule = gravParticleSystem.velocityOverLifetime;
        mainModule = gravParticleSystem.main;

        // Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
        // Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
        base.Start();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) other.GetComponent<CharController>().DeInvert();
    }

    // protected void Update()
    // {
    // }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            // TODO: change this to work with activeGravityDirection and inactiveGravityDirection

            if (IsActive)
                other.GetComponent<CharController>().Invert();
            else
                other.GetComponent<CharController>().DeInvert();
        }
    }

    protected override void Activate()
    {
        base.Activate();
        //var velocityModule = gravParticleSystem.velocityOverLifetime;
        gravParticleSystem.Play();
        velocityModule.speedModifier = 1;
        mainModule.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 0);
        shapeModule.position = new Vector3(0, lowerYBound, 0);
    }

    protected override void Deactivate()
    {
        base.Deactivate();
        //var velocityModule = gravParticleSystem.velocityOverLifetime;
        gravParticleSystem.Stop();
        gravParticleSystem.Clear();
        // velocityModule.speedModifier = -1;
        // mainModule.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 180);
        // shapeModule.position = new Vector3(0, -lowerYBound, 0);
    }
}*/

