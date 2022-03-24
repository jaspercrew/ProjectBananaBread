using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public abstract class Interactor : Entity {
    //components
    protected Renderer Renderer;
    protected BoxCollider2D Collider2D;
    protected SpriteRenderer highlightFX;

    //trackers
    protected bool isInteractable;

    public static HashSet<Interactor> interactors = new HashSet<Interactor>();
    
    // Start is called before the first frame update
    protected void Start() {
        Collider2D = GetComponent<BoxCollider2D>();
        Renderer = GetComponent<Renderer>();
        isInteractable = false;
        highlightFX = transform.Find("highlightFX").GetComponent<SpriteRenderer>();
        highlightFX.enabled = false;
    }

    public virtual void Interact() {
        Debug.Log("beans");
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Add(this);
            isInteractable = true;
            highlightFX.enabled = true;
            //renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Remove(this);
            isInteractable = false;
            highlightFX.enabled = false;
            //renderer.material.shader = Shader.Find("Shader Graphs/BorderGraph");
        }
    }

}
