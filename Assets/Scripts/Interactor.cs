using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Interactor : Entity {
    //components
    protected Renderer Renderer;
    protected BoxCollider2D Collider2D;
    
    //configs
    protected float bonusInteractRange;
    
    //trackers
    protected bool isInteractable;

    public static HashSet<Interactor> interactors = new HashSet<Interactor>();
    
    // Start is called before the first frame update
    protected void InitializeInteractor() {
        Collider2D = GetComponent<BoxCollider2D>();
        Renderer = GetComponent<Renderer>();
        Collider2D.edgeRadius = bonusInteractRange;
        //renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
        isInteractable = false;
    }
    
    protected void InteractorUpdate() {
        //if (isInteractable) {
        //    Interact();
        //}
    }
    
    public virtual void Interact() {
        Debug.Log("beans");
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other) {
        TrigEnterFunction(other);
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other) {
        TrigExitFunction(other);
    }

    protected virtual void TrigEnterFunction(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Add(this);
            isInteractable = true;
            //renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
        }
    }
    protected virtual void TrigExitFunction(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Remove(this);
            isInteractable = false;
            //renderer.material.shader = Shader.Find("Shader Graphs/BorderGraph");
        }
    }
}
