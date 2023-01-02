using UnityEngine;

public class GrappleProjectile : Projectile {
    //components
    //private LineRenderer lineRenderer;
    //private CharController characterController;
    //private RadialGrapple grappleController;
    
    // configs
    //private const float GravityScale = 0f;

    // parameters
    private float speed;
    private Vector3 direction;

    public void Initialize(Vector3 dir, float newSpeed) {
        direction = dir.normalized;
        speed = newSpeed;
    }
    
    protected override void Start() {
        Collider2D = GetComponent<BoxCollider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        //characterController = FindObjectOfType<CharController>();
        //grappleController = FindObjectOfType<RadialGrapple>();
        //Rigidbody2D.gravityScale = GravityScale;
        //lineRenderer = GetComponent<LineRenderer>();
        Rigidbody2D.velocity = direction * speed;
        base.Start();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        
        if (other.gameObject.layer == LayerMask.NameToLayer("Obstacle"))
        {
            print(other.gameObject.name);
            Rigidbody2D.velocity = Vector2.zero;
            Vector3 pos = transform.position;
            CharController.Instance.StartGrapple(pos);
        }
    }
}