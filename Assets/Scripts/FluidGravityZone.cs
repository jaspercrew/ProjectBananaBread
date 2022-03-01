//DEPRECATED



// using System;
// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;
//
// public class FluidGravityZone : Entity
// {
//     // Start is called before the first frame update
//     private BoxCollider2D boxCollider2D;
//     private SpriteRenderer spriteRenderer;
//     private CharController charController;
//     [SerializeField] public EnvironmentState invertState;
//     private bool isActive = false;
//     
//     void Start() {
//         boxCollider2D = GetComponent<BoxCollider2D>();
//         spriteRenderer = GetComponent<SpriteRenderer>();
//         charController = FindObjectOfType<CharController>();
//         if (invertState == GameManager.Instance.currentState) {
//             EnableFGZ();
//         }
//         else {
//             DisableFGZ();
//         }
//     }
//
//     // Update is called once per frame
//     void Update()
//     {
//         
//     }
//
//     public override void SwitchToState(EnvironmentState state) {
//         if (invertState == state) {
//             EnableFGZ();
//         }
//         else {
//             if (boxCollider2D.bounds.Contains(charController.transform.position)) {
//                 charController.DeInvert();
//             }
//             DisableFGZ();
//             //TODO: disable player inversion
//         }
//     }
//
//     private void OnTriggerEnter2D(Collider2D other) {
//         if (other.gameObject.CompareTag("Player") && isActive) {
//             other.GetComponent<CharController>().Invert();
//         }
//     }
//     
//     private void OnTriggerExit2D(Collider2D other) {
//         if (other.gameObject.CompareTag("Player") && isActive) {
//             other.GetComponent<CharController>().DeInvert();
//         }
//     }
//
//     private void EnableFGZ() {
//         isActive = true;
//         boxCollider2D.size = Vector2.one;
//         spriteRenderer.enabled = true;
//     }
//
//     private void DisableFGZ() {
//         isActive = false;
//         boxCollider2D.size = Vector2.zero;
//         spriteRenderer.enabled = false;
//     }
// }
