// using UnityEngine;
//
// public class GrappleableFluidPlatform : FluidPlatform
// {
//     private Collider2D boxCollider;
//     private SpriteRenderer spriteRenderer;
//
//     // Start is called before the first frame update
//     private void Awake() {
//         boxCollider = GetComponent<Collider2D>();
//         spriteRenderer = GetComponent<SpriteRenderer>();
//         CheckPlatform();
//     }
//
//     protected override void TurnShifted() {
//         isGrappleable = true;
//     }
//     protected override void TurnUnshifted() {
//         isGrappleable = false;
//     }
// }
//
//
