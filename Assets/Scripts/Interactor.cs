using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Interactor : Entity {
    //components
    protected Renderer renderer;
    protected BoxCollider2D collider2D;
    
    //configs
    protected float bonusInteractRange;
    
    //trackers
    protected bool isInteractable;

    public static HashSet<Interactor> interactors = new HashSet<Interactor>();
    
    // Start is called before the first frame update
    protected void InitializeInteractor() {
        collider2D = GetComponent<BoxCollider2D>();
        renderer = GetComponent<Renderer>();
        collider2D.edgeRadius = bonusInteractRange;
        renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
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
        if (other.gameObject.CompareTag("Player")) {
            interactors.Add(this);
            isInteractable = true;
            renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Remove(this);
            isInteractable = false;
            renderer.material.shader = Shader.Find("Shader Graphs/BorderGraph");
        }
    }
}
