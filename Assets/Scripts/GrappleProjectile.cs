// using UnityEngine;
//
// public class GrappleProjectile : Projectile {
//     // parameters
//     private float speed;
//     private Vector3 direction;
//
//     public void SetStats(Vector3 dir, float newSpeed) {
//         direction = dir.normalized;
//         speed = newSpeed;
//     }
//     
//     protected override void Start() {
//         base.Start();
//         Collider2D = GetComponent<BoxCollider2D>();
//         Rigidbody2D = GetComponent<Rigidbody2D>();
//         Rigidbody2D.velocity = direction * speed;
//     }
//     
//
//     private void OnTriggerEnter2D(Collider2D other)
//     {
//         GrapplePoint point = other.gameObject.GetComponent<GrapplePoint>();
//         
//         
//         if (point == null) return;
//         Rigidbody2D.velocity = Vector2.zero;
//         Destroy(gameObject);
//         CharController.Instance.StartLineGrapple(point);
//         
//     }
// }
