using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class WindEmitterChild : MonoBehaviour
{
    private WindEmitter parent;
    private BoxCollider2D boxCollider;
    private BoxCollider2D parentBoxCollider;

    [Header("Don't change this object's Transform and don't add a collider!!")]
    public float none;

    private void Start()
    {
        parent = transform.parent.GetComponent<WindEmitter>();
        boxCollider = gameObject.AddComponent<BoxCollider2D>();
        boxCollider.isTrigger = true;
        parentBoxCollider = parent.transform.GetComponent<BoxCollider2D>();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CharController.Instance.currentWindZone = parent;
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        CharController.Instance.currentWindZone = null;
    }
}