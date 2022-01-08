using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidGravityZone : Entity
{
    // Start is called before the first frame update
    private BoxCollider2D boxCollider2D;
    private AreaEffector2D areaEffector2D;
    [SerializeField] public EnvironmentState invertState;
    //private bool isActive = false;
    
    void Start() {
        boxCollider2D = GetComponent<BoxCollider2D>();
        areaEffector2D = GetComponent<AreaEffector2D>();
        //areaEffector2D.enabled = false;
        areaEffector2D.forceAngle = 270f;
        areaEffector2D.forceMagnitude = -Physics.gravity.magnitude * 2;
        // if (invertState == GameManager.Instance.currentState) {
        //     areaEffector2D.enabled = true;
        // }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SwitchToState(EnvironmentState state) {
        // if (invertState == state) {
        //     areaEffector2D.enabled = true;
        // }
        // else {
        //     areaEffector2D.enabled = false;
        // }
    }

    // private void OnTriggerEnter2D(Collider2D other) {
    //     if (other.gameObject.CompareTag("Player") && isActive) {
    //         other.GetComponent<Rigidbody2D>().gravityScale *= -1;
    //     }
    // }
    //
    // private void OnTriggerExit2D(Collider2D other) {
    //     if (other.gameObject.CompareTag("Player") && isActive) {
    //         other.GetComponent<Rigidbody2D>().gravityScale *= -1;
    //     }
    // }
}
