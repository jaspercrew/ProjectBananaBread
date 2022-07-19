using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class FluidGravityZone : ActivatedEntity //active = inverted gravity
 {
     // Start is called before the first frame update
     private BoxCollider2D boxCollider2D;
     private SpriteRenderer spriteRenderer;

     void Start()
     {
         boxCollider2D = GetComponent<BoxCollider2D>();
         spriteRenderer = GetComponent<SpriteRenderer>();
     }


     private void OnTriggerStay2D(Collider2D other) {
         if (other.gameObject.CompareTag("Player")) {
             if (isActive)
             {
                 other.GetComponent<CharController>().Invert();
             }
             else
             {
                 other.GetComponent<CharController>().DeInvert();
             }
         }
     }
     
     private void OnTriggerExit2D(Collider2D other) {
         if (other.gameObject.CompareTag("Player")) {
             other.GetComponent<CharController>().DeInvert();
         }
     }

     private void EnableFGZ() {
         isActive = true;
         boxCollider2D.size = Vector2.one;
         spriteRenderer.enabled = true;
     }

     private void DisableFGZ() {
         isActive = false;
         boxCollider2D.size = Vector2.zero;
         spriteRenderer.enabled = false;
     }
 }
