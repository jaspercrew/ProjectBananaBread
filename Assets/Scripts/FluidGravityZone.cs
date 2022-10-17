using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

 public class FluidGravityZone : ActivatedEntity // active = inverted gravity
 {
     public enum GravityDirection
     {
         North, South, East, West, None
     }
     
     // Start is called before the first frame update
     private BoxCollider2D boxCollider2D;
     private SpriteRenderer spriteRenderer;
     public GravityDirection inactiveGravityDirection;
     public GravityDirection activeGravityDirection;

     protected override void Start()
     {
         base.Start();
         boxCollider2D = GetComponent<BoxCollider2D>();
         spriteRenderer = GetComponent<SpriteRenderer>();
         
         Vector3 bounds = boxCollider2D.bounds.extents;
         float halfWidth = Mathf.Abs(bounds.x);
         float halfHeight = Mathf.Abs(bounds.y);
         Vector2 center = (Vector2) transform.position + boxCollider2D.offset.y * Vector2.up;

         Vector2 bottomMiddle = center + halfHeight * Vector2.down;
         Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
         Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
     }

     // protected void Update()
     // {
     // }

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
