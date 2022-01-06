using UnityEngine;
using System.Collections;
using Newtonsoft.Json.Serialization;

public class RadialGrapple : MonoBehaviour{
    public Camera mainCamera;
    private LineRenderer _lineRenderer;
    private DistanceJoint2D _distanceJoint;
    private Rigidbody2D rigidbody2D;
    public bool isGrappling = false;

    private Rigidbody2D p;
    public Rigidbody2D projectile;

// Start is called before the first frame update
    void Start() {
        _lineRenderer = GetComponent<LineRenderer>();
        _distanceJoint = GetComponent<DistanceJoint2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        _distanceJoint.enabled = false;
    }

// Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            //Vector2 mousePos = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 leftDir = new Vector2(-1, 1.4f);
            Vector2 rightDir = new Vector2(1, 1.4f);
            if (transform.localScale.x > 0.5) {
                LaunchHook(leftDir);
            }
            else if (transform.localScale.x < -0.5) {
                LaunchHook(rightDir);
            }
            
        }
        else if (Input.GetKeyUp(KeyCode.Q)) {
            //TODO kill projectile as well
            
            if (p != null) { 
                Destroy(p.gameObject);
            }

            EndGrapple();
        }

        if (_distanceJoint.enabled) {
            _lineRenderer.SetPosition(1, transform.position);
        }
    }

    private void LaunchHook(Vector2 direction) {
        const float speed = 20f;
        Vector3 offset = new Vector3(0, 1, 0);
        p = Instantiate(projectile, transform.position + offset, transform.rotation);
        p.gameObject.GetComponent<GrappleProjectile>().SetStats(direction.normalized, speed);
        
    }

    public void StartGrapple(Vector3 grapplePoint, float grappleLength) {
        _lineRenderer.SetPosition(0, grapplePoint);
        _lineRenderer.SetPosition(1, transform.position);
        _distanceJoint.connectedAnchor = grapplePoint;
        _distanceJoint.distance = grappleLength * .90f;
        _distanceJoint.enabled = true;
        _lineRenderer.enabled = true;
        isGrappling = true;

        //const float boostForce = 5f;
        const float gravModifier = .8f;
        const float minVel = 10f;

        if (transform.localScale.x > 0.5) {
            //facing left
            rigidbody2D.velocity += (new Vector2(rigidbody2D.velocity.y, 0) * gravModifier);
            if (rigidbody2D.velocity.x > -minVel) {
                rigidbody2D.velocity = (Vector2.left * minVel);
                Debug.Log("left boost");
            }
        }
        else if (transform.localScale.x < -0.5) {
            rigidbody2D.velocity -= (new Vector2(rigidbody2D.velocity.y, 0) * gravModifier);
            if (rigidbody2D.velocity.x < minVel) {
                rigidbody2D.velocity = (Vector2.right * minVel);
                Debug.Log("right boost");
            }
        }
    }

    public void EndGrapple() {
        _distanceJoint.enabled = false;
        _lineRenderer.enabled = false;
        isGrappling = false;
    }
    
    
}