using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class FluidGravityZone : ActivatedEntity //active = inverted gravity
 {
     // Start is called before the first frame update
     private BoxCollider2D boxCollider2D;
     private SpriteRenderer spriteRenderer;

     protected override void Start()
     {
         base.Start();
         boxCollider2D = GetComponent<BoxCollider2D>();
         spriteRenderer = GetComponent<SpriteRenderer>();
     }


     private void OnTriggerStay2D(Collider2D other) {
         if (other.gameObject.CompareTag("Player")) {
             if (IsActive)
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

     protected override void Activate() {
         base.Activate();
     }

     protected override void Deactivate() {
         base.Deactivate();
     }
 }
