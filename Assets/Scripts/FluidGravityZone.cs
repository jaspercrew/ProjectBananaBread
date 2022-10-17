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

     private const float particleDensity = 2f;
     private const float edgeGap = 1f;
     private float lowerYBound;
     
     // Start is called before the first frame update
     private BoxCollider2D boxCollider2D;
     private ParticleSystem particleSystem;
     private ParticleSystem.ShapeModule shapeModule;
     private SpriteRenderer spriteRenderer;
     private ParticleSystem.VelocityOverLifetimeModule velocityModule;
     private ParticleSystem.MainModule mainModule;
     
     public GravityDirection inactiveGravityDirection;
     public GravityDirection activeGravityDirection;

     protected override void Start()
     {
         
         boxCollider2D = GetComponent<BoxCollider2D>();
         spriteRenderer = GetComponent<SpriteRenderer>();
         particleSystem = GetComponent<ParticleSystem>();

         lowerYBound = -boxCollider2D.size.y / 2;
         shapeModule = particleSystem.shape;
         shapeModule.radius = boxCollider2D.size.x / 2 - edgeGap;
         shapeModule.position = new Vector3(0, lowerYBound, 0);

         var emission = particleSystem.emission;

         var burst = emission.GetBurst(0);
         burst.count = new ParticleSystem.MinMaxCurve(shapeModule.radius * particleDensity);
         emission.SetBurst(0, burst);

         velocityModule = particleSystem.velocityOverLifetime;
         mainModule = particleSystem.main;

         // Vector2 bottomLeft = bottomMiddle + halfWidth * Vector2.left;
         // Vector2 bottomRight = bottomMiddle + halfWidth * Vector2.right;
         base.Start();
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
         //var velocityModule = particleSystem.velocityOverLifetime;
         particleSystem.Play();
         velocityModule.speedModifier = 1;
         mainModule.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 0);
         shapeModule.position = new Vector3(0, lowerYBound, 0);
         
     }

     protected override void Deactivate() {
         base.Deactivate();
         //var velocityModule = particleSystem.velocityOverLifetime;
         particleSystem.Stop();
         particleSystem.Clear();
         // velocityModule.speedModifier = -1;
         // mainModule.startRotation = new ParticleSystem.MinMaxCurve(Mathf.Deg2Rad * 180);
         // shapeModule.position = new Vector3(0, -lowerYBound, 0);

     }
 }
