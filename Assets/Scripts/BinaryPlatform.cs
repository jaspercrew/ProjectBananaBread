using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BinaryPlatform : FluidObject //is only active in one state
{
    private Collider2D _collider;
    public bool isActive;
    // Start is called before the first frame update
    void Start() {
        _collider = GetComponent<Collider2D>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    protected override void SwitchState(EnvironmentState state) {
        if (isActive) {
            _collider.enabled = false;
            
        }
        else {
            _collider.enabled = true;
        }
        isActive = !isActive;
    }
}
