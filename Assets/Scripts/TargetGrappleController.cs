using UnityEngine;

public class TargetGrappleController : MonoBehaviour {
    // components
    private new Rigidbody2D rigidbody;
    
    // trackers
    public bool isGrappling;
    public Transform swingPoint;
    
    // configs
    private const float VelocityModifier = 1.5f;
    private const float GrappleBreakValue = 2f;
    private const float MAXGrappleVelocity = 10f;


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
        rigidbody.velocity += connect.normalized * VelocityModifier;
        if (rigidbody.velocity.magnitude > MAXGrappleVelocity) {
            rigidbody.velocity *= MAXGrappleVelocity / rigidbody.velocity.magnitude;
        }

        if ((transform.position - swingPoint.position).magnitude < GrappleBreakValue) {
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
