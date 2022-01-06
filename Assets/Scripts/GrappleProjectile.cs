using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrappleProjectile : Projectile {
    //components
    private LineRenderer lineRenderer;
    private CharController characterController;
    private RadialGrapple grappleController;
    
    //configs
    private float GravityScale = 0f;
    
    //parameters
    private float speed;
    private Vector3 direction;

    public void SetStats(Vector3 direction, float speed) {
        this.direction = direction.normalized;
        this.speed = speed;
    }
    
    // Start is called before the first frame update
    void Start() {
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        characterController = FindObjectOfType<CharController>();
        grappleController = FindObjectOfType<RadialGrapple>();
        rigidbody.gravityScale = GravityScale;
        lineRenderer = GetComponent<LineRenderer>();
        rigidbody.velocity = direction * speed;
        
    }

    // Update is called once per frame
    void Update() {
        lineRenderer.SetPosition(0, transform.position);
        lineRenderer.SetPosition(1, characterController.transform.position);
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (!other.gameObject.CompareTag("Player")) {
            Debug.Log(other.gameObject.name);
            float length = (transform.position - characterController.transform.position).magnitude;
            grappleController.StartGrapple(other.ClosestPoint(transform.position), length);
            Destroy(gameObject);
        }
    }
}
