using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class FluidGravityZone : ActivatedEntity // active = inverted gravity
 {
     // Start is called before the first frame update
     private BoxCollider2D boxCollider2D;
     private SpriteRenderer spriteRenderer;
     public Vector2 inactiveGravityDirection;
     public Vector2 activeGravityDirection;

     protected override void Start()
     {
         base.Start();
         boxCollider2D = GetComponent<BoxCollider2D>();
         spriteRenderer = GetComponent<SpriteRenderer>();
         activeGravityDirection.Normalize();
         inactiveGravityDirection.Normalize();
     }

     protected void Update()
     {
         activeGravityDirection.Normalize();
         inactiveGravityDirection.Normalize();
     }


     private void OnTriggerStay2D(Collider2D other) {
         if (other.gameObject.CompareTag("Player")) {
             // TODO: change this to work with activeGravityDirection and inactiveGravityDirection
             
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
