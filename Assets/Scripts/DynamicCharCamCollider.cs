// using System;
// using Cinemachine;
// using UnityEngine;
//
// public class DynamicCharCamCollider : MonoBehaviour
// {
//     private CinemachineVirtualCamera cam;
//     private PolygonCollider2D camCollider;
//     public int cameraOverridePriority = 0;
//     
//
//     private void Start()
//     {
//         cam = GetComponent<CinemachineVirtualCamera>();
//         camCollider = GetComponentInParent<PolygonCollider2D>();
//         transform.GetComponent<CinemachineVirtualCamera>().Follow = FindObjectOfType<CharController>().transform;
//     }
//
//     private void Update()
//     {
//         cam.Priority = 1;
//         if (camCollider.bounds.Contains(CharController.Instance.transform.position))
//         {
//             cam.Priority = 15 + cameraOverridePriority;
//         }
//     }
//     
//     
//
//
//     // private void OnTriggerStay2D(Collider2D other) {
//     //     if (other.CompareTag("Player"))
//     //     {
//     //         cam.Priority = 15;
//     //     }
//     // }
//     //
//     // private void OnTriggerExit2D(Collider2D other) {
//     //     if (other.CompareTag("Player"))
//     //     {
//     //         cam.Priority = 1;
//     //     }
//     // }
// }