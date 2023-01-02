using System;
using UnityEngine;

public class RadialGrapple : MonoBehaviour{
    //public Camera mainCamera;
    private LineRenderer grappleLineRenderer;
    private DistanceJoint2D grappleDistanceJoint;
    private Rigidbody2D rigidbody2d;
    public bool isGrappling;
    private Rigidbody2D instantiatedProjectile;
    public Rigidbody2D projectilePrefab;
    private Vector3 attachmentPoint;

    private void Start() {
        grappleLineRenderer = GetComponent<LineRenderer>();
        grappleDistanceJoint = GetComponent<DistanceJoint2D>();
        rigidbody2d = GetComponent<Rigidbody2D>();
        grappleLineRenderer.enabled = false;
        grappleDistanceJoint.enabled = false;
    }

    private void Update() {
        if (Input.GetKeyDown(KeyCode.Q)) {
            //Vector2 mousePos = (Vector2) mainCamera.ScreenToWorldPoint(Input.mousePosition);
            Vector2 Dir = new Vector2(1, 1.4f);
            if (transform.localScale.x > 0.5) {
                Dir.x = -Dir.x;
            }

            if (CharController.Instance.isInverted) {
                Dir.y = -Dir.y;
            }
            LaunchHook(Dir);
        }
        else if (Input.GetKeyUp(KeyCode.Q)) {
            //TODO kill projectilePrefab as well
            
            if (instantiatedProjectile != null) { 
                Destroy(instantiatedProjectile.gameObject);
            }

            EndGrapple();
        }
        grappleLineRenderer.SetPosition(1, transform.position);
        if (instantiatedProjectile != null) //isLaunched
        {
            grappleLineRenderer.SetPosition(0, instantiatedProjectile.transform.position);
        }
        
        if (isGrappling) {
            const float offsetMultiplier = 1f;
            //float offset = Mathf.Cos(Vector3.Angle(transform.position - attachmentPoint, Vector3.down)) * offsetMultiplier;
            //Debug.Log(Vector3.Angle(transform.position - attachmentPoint, Vector3.up));
            //Debug.Log(offset);
            float grappleLength = (attachmentPoint - transform.position).magnitude;
            grappleDistanceJoint.distance = grappleLength;
            //grappleLineRenderer.SetPosition(0, instantiatedProjectile.transform.position);
        }
    }

    private void LaunchHook(Vector2 direction)
    {
        print("hook launched");
        const float speed = 20f;
        Vector3 offset = CharController.Instance.isInverted ? new Vector3(0, -1, 0) : new Vector3(0, 1, 0);
        instantiatedProjectile = Instantiate(projectilePrefab, transform.position + offset, transform.rotation);
        instantiatedProjectile.gameObject.GetComponent<GrappleProjectile>().Initialize(direction.normalized, speed);
        grappleLineRenderer.enabled = true;
        
    }

    public void StartGrapple(Vector3 grapplePoint) {
        
        attachmentPoint = grapplePoint;
        const float verticalDisplacementOffset = .5f;
        //Vector3 diffNormalized = (grapplePoint - transform.position).normalized ;
        transform.position += new Vector3(0, verticalDisplacementOffset, 0);
        
        
        isGrappling = true;
        //charController.isRecentlyGrappled = true;
        //grappleLineRenderer.SetPosition(0, grapplePoint);
        //grappleLineRenderer.SetPosition(1, transform.position);
        grappleDistanceJoint.connectedAnchor = grapplePoint;
        
        grappleDistanceJoint.enabled = true;
        grappleLineRenderer.enabled = true;
        

        //const float boostForce = 5f;
        const float gravModifier = .8f;
        const float minVel = 10f;

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
        grappleDistanceJoint.enabled = false;
        grappleLineRenderer.enabled = false;
        isGrappling = false;
    }
    
    
}