using System;
using Cinemachine;
using UnityEngine;

public class DynamicCharCamCollider : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    private PolygonCollider2D camCollider;
    

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        camCollider = GetComponentInParent<PolygonCollider2D>();
        transform.GetComponent<CinemachineVirtualCamera>().Follow = FindObjectOfType<CharController>().transform;
    }

    private void Update()
    {
        cam.Priority = 1;
        if (camCollider.bounds.Contains(CharController.Instance.transform.localPosition) && CharController.Instance.transform.parent == null)
        {
            cam.Priority = 15;
        }
    }


    // private void OnTriggerStay2D(Collider2D other) {
    //     if (other.CompareTag("Player"))
    //     {
    //         cam.Priority = 15;
    //     }
    // }
    //
    // private void OnTriggerExit2D(Collider2D other) {
    //     if (other.CompareTag("Player"))
    //     {
    //         cam.Priority = 1;
    //     }
    // }
}