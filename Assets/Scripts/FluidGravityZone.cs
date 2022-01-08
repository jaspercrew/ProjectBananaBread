using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FluidGravityZone : Entity
{
    // Start is called before the first frame update
    private BoxCollider2D boxCollider2D;
    [SerializeField] public EnvironmentState invertState;
    private bool isActive = false;
    
    void Start() {
        boxCollider2D = GetComponent<BoxCollider2D>();
        if (invertState == GameManager.Instance.currentState) {
            isActive = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SwitchToState(EnvironmentState state) {
        if (invertState == state) {
            isActive = true;
        }
        else {
            isActive = false;
            //disable player inversion
        }
    }

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && isActive) {
            other.GetComponent<CharController>().isInverted = true;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player") && isActive) {
            other.GetComponent<CharController>().isInverted = false;
        }
    }
}
