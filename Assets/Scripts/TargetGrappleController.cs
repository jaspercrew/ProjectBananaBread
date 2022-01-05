using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TargetGrappleController : MonoBehaviour {
    //componentss
    private Rigidbody2D _rigidbody;
    
    //trackers
    public bool isGrappling = false;
    public Transform swingPoint;
    
    //configs
    private float velocityModifier = 1.5f;
    private float grappleBreakValue = 2f;
    private float maxGrappleVelocity = 10f;
    
    
    // Start is called before the first frame update
    void Start() {
        _rigidbody = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    private void FixedUpdate() {
        if (isGrappling) {
            Debug.DrawLine (transform.position, swingPoint.position, Color.red);
            //Assert.IsTrue(swingPoint != null);
            Vector2 connect = swingPoint.position - transform.position;
            _rigidbody.velocity += connect.normalized * velocityModifier;
            if (_rigidbody.velocity.magnitude > maxGrappleVelocity) {
                _rigidbody.velocity *= maxGrappleVelocity / _rigidbody.velocity.magnitude;
            }

            if ((transform.position - swingPoint.position).magnitude < grappleBreakValue) {
                EndGrapple();
            }

        }
    }

    public void StartGrapple() {
        _rigidbody.gravityScale /= 10;
        isGrappling = true;
        //this.swingPoint = swingPoint;
    }

    public void EndGrapple() {
        _rigidbody.gravityScale *= 10;
        isGrappling = false;
    }
}
