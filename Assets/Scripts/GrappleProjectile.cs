using UnityEngine;

public class GrappleProjectile : Projectile {
    //components
    //private LineRenderer lineRenderer;
    private CharController characterController;
    private RadialGrapple grappleController;
    
    // configs
    private const float GravityScale = 0f;

    // parameters
    private float speed;
    private Vector3 direction;

    public void SetStats(Vector3 dir, float newSpeed) {
        direction = dir.normalized;
        speed = newSpeed;
    }
    
    private void Start() {
        Collider2D = GetComponent<BoxCollider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        characterController = FindObjectOfType<CharController>();
        grappleController = FindObjectOfType<RadialGrapple>();
        Rigidbody2D.gravityScale = GravityScale;
        //lineRenderer = GetComponent<LineRenderer>();
        Rigidbody2D.velocity = direction * speed;
    }

    private void Update() {
        // lineRenderer.SetPosition(0, transform.position);
        // lineRenderer.SetPosition(1, characterController.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Platform p = other.gameObject.GetComponent<Platform>();
        
        
        if (p == null || !p.isGrappleable) return;
        Rigidbody2D.velocity = Vector2.zero;
        //Debug.Log(other.gameObject.name);
        Vector3 pos = transform.position;
        grappleController.StartGrapple(pos);
        
        //lineRenderer.
        //Destroy(gameObject);
    }
}
