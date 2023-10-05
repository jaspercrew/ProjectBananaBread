using UnityEngine;

public class GrappleProjectile : Projectile
{
    private Vector3 direction;
    //components
    //private LineRenderer lineRenderer;
    //private CharController characterController;
    //private RadialGrapple grappleController;

    // configs
    //private const float GravityScale = 0f;

    // parameters
    private float speed;

    protected override void Start()
    {
        collider2D = GetComponent<BoxCollider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        //characterController = FindObjectOfType<CharController>();
        //grappleController = FindObjectOfType<RadialGrapple>();
        //Rigidbody2D.gravityScale = GravityScale;
        //lineRenderer = GetComponent<LineRenderer>();
        rigidbody2D.velocity = direction * speed;
        base.Start();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            //print(other.gameObject.name);
            rigidbody2D.velocity = Vector2.zero;
            var pos = transform.position;
            CharController.instance.StartGrapple(pos);
        }
    }

    public void Initialize(Vector3 dir, float newSpeed)
    {
        direction = dir.normalized;
        speed = newSpeed;
    }
}