using System;
using UnityEngine;

public partial class RadialGrapple : MonoBehaviour{
    public Camera mainCamera;
    private LineRenderer lineRenderer;
    private DistanceJoint2D distanceJoint;
    private Rigidbody2D rigidbody2d;
    private CharController charController;
    public bool isGrappling;

    private Rigidbody2D p;
    public Rigidbody2D projectile;

    private Vector3 attachmentPoint;

    private void Start() {
        lineRenderer = GetComponent<LineRenderer>();
        distanceJoint = GetComponent<DistanceJoint2D>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        charController = FindObjectOfType<CharController>();
        
        distanceJoint.enabled = false;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            //Vector2 mousePos = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 Dir = new Vector2(1, 1.4f);
            if (transform.localScale.x > 0.5) {
                Dir.x = -Dir.x;
            }

            if (charController.isInverted) {
                Dir.y = -Dir.y;
            }
            LaunchHook(Dir);
        }
        else if (Input.GetKeyUp(KeyCode.Q)) {
            //TODO kill projectile as well
            
            if (p != null) { 
                Destroy(p.gameObject);
            }

            EndGrapple();
        }

        

        if (distanceJoint.enabled) {
            lineRenderer.SetPosition(1, transform.position);
        }
    }

    private void FixedUpdate() {
        if (isGrappling) {
            const float offsetMultiplier = 1f;
            float offset = Mathf.Cos(Vector3.Angle(transform.position - attachmentPoint, Vector3.down)) * offsetMultiplier;
            //Debug.Log(Vector3.Angle(transform.position - attachmentPoint, Vector3.up));
            //Debug.Log(offset);
            float grappleLength = (attachmentPoint - transform.position).magnitude;
            distanceJoint.distance = grappleLength;
        }
    }

    private void LaunchHook(Vector2 direction) {
        const float speed = 20f;
        Vector3 offset = charController.isInverted ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        p = Instantiate(projectile, transform.position + offset, transform.rotation);
        p.gameObject.GetComponent<GrappleProjectile>().SetStats(direction.normalized, speed);
        
    }

    public void StartGrapple(Vector3 grapplePoint) {
        
        attachmentPoint = grapplePoint;
        const float offsetMultiplier = .5f;
        //Vector3 diffNormalized = (grapplePoint - transform.position).normalized ;
        transform.position += new Vector3(0, 1 * offsetMultiplier, 0);
        
        
        isGrappling = true;
        charController.isRecentlyGrappled = true;
        lineRenderer.SetPosition(0, grapplePoint);
        lineRenderer.SetPosition(1, transform.position);
        distanceJoint.connectedAnchor = grapplePoint;
        
        distanceJoint.enabled = true;
        lineRenderer.enabled = true;
        

        //const float boostForce = 5f;
        const float gravModifier = .8f;
        const float minVel = 7f;

        if (transform.localScale.x > 0.5) {
            //facing left
            rigidbody2d.velocity += (new Vector2(rigidbody2d.velocity.y, 0) * gravModifier);
            if (rigidbody2d.velocity.x > -minVel) {
                rigidbody2d.velocity = (Vector2.left * minVel);
                //Debug.Log("left boost");
            }
        }
        else if (transform.localScale.x < -0.5) {
            rigidbody2d.velocity -= (new Vector2(rigidbody2d.velocity.y, 0) * gravModifier);
            if (rigidbody2d.velocity.x < minVel) {
                rigidbody2d.velocity = (Vector2.right * minVel);
                //Debug.Log("right boost");
            }
        }
    }

    public void EndGrapple() {
        distanceJoint.enabled = false;
        lineRenderer.enabled = false;
        isGrappling = false;
    }
    
    
}