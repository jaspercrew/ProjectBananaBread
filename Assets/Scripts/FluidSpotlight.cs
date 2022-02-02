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
        transform.Find("ToRotate").Rotate(Vector3.forward, 30);
        //transform.getch
    }
    
    protected override void TrigEnterFunction(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Add(this);
            isInteractable = true;
            transform.Find("ToRotate").Find("ToEnable").GetComponentInChildren<SpriteRenderer>().enabled = true;
            //renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
        }
    }
    protected override void TrigExitFunction(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Remove(this);
            isInteractable = false;
            transform.Find("ToRotate").Find("ToEnable").GetComponentInChildren<SpriteRenderer>().enabled = false;
            //renderer.material.shader = Shader.Find("Shader Graphs/BorderGraph");
        }
    }
}
