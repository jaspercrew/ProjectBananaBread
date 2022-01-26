using System;
using System.Collections;
using System.Collections.Generic;
using System.Transactions;
using UnityEngine;
using UnityEngine.Experimental.Rendering.Universal;

public class FluidSpotlight : Interactor {
    
    

    // Start is called before the first frame update
    void Start() {
        InitializeInteractor();
        bonusInteractRange = 0f;

    }

    // Update is called once per frame
    void Update()
    {
        InteractorUpdate();
    }

    public override void Interact() {
        transform.GetChild(0).Rotate(Vector3.forward, 30);
    }
}
