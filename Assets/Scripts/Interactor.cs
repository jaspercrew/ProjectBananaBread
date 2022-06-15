using System.Collections.Generic;
using UnityEngine;

public abstract class Interactor : Entity {
    //components
    protected Renderer Renderer;
    protected BoxCollider2D Collider2D;
    protected SpriteRenderer HighlightFX;

    //trackers
    protected bool IsInteractable;

    public static readonly HashSet<Interactor> interactors = new HashSet<Interactor>();
    
    // Start is called before the first frame update
    protected void Start() {
        Collider2D = GetComponent<BoxCollider2D>();
        Renderer = GetComponent<Renderer>();
        IsInteractable = false;
        HighlightFX = transform.Find("highlightFX").GetComponent<SpriteRenderer>();
        HighlightFX.enabled = false;
    }

    public virtual void Interact() {
        Debug.Log("beans");
    }
    
    protected virtual void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Add(this);
            IsInteractable = true;
            HighlightFX.enabled = true;
            //renderer.material.shader = Shader.Find("Shader Graphs/GenericShader");
        }
    }
    
    protected virtual void OnTriggerExit2D(Collider2D other) {
        if (other.gameObject.CompareTag("Player")) {
            interactors.Remove(this);
            IsInteractable = false;
            HighlightFX.enabled = false;
            //renderer.material.shader = Shader.Find("Shader Graphs/BorderGraph");
        }
    }

    protected virtual void Update()
    {
        // if (!Collider2D.bounds.Contains(CharController.Instance.transform.position))
        // {
        //     interactors.Remove(this);
        // }
    }
}
