using UnityEngine;

public class TargetGrappleController : MonoBehaviour {
    // components
    private new Rigidbody2D rigidbody;
    
    // trackers
    public bool isGrappling;
    public Transform swingPoint;
    
    // configs
    private float velocityModifier = 1.5f;
    private float grappleBreakValue = 2f;
    private float maxGrappleVelocity = 10f;
    
    
    // Start is called before the first frame update
    private void Start() {
        rigidbody = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (!isGrappling) return;
        
        Debug.DrawLine (transform.position, swingPoint.position, Color.red);
        // Assert.IsTrue(swingPoint != null);
        Vector2 connect = swingPoint.position - transform.position;
        rigidbody.velocity += connect.normalized * velocityModifier;
        if (rigidbody.velocity.magnitude > maxGrappleVelocity) {
            rigidbody.velocity *= maxGrappleVelocity / rigidbody.velocity.magnitude;
        }

        if ((transform.position - swingPoint.position).magnitude < grappleBreakValue) {
            EndGrapple();
        }
    }

    public void StartGrapple() {
        rigidbody.gravityScale /= 10;
        isGrappling = true;
        // this.swingPoint = swingPoint;
    }

    public void EndGrapple() {
        rigidbody.gravityScale *= 10;
        isGrappling = false;
    }
}
