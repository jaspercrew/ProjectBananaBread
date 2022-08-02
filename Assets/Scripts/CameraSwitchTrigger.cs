using System;
using Cinemachine;
using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour
{
    private CinemachineVirtualCamera camera;
    private BoxCollider2D camCollider;
    

    private void Start()
    {
        camera = GetComponent<CinemachineVirtualCamera>();
        //camCollider = GetComponent<BoxCollider2D>();
        // Vector2 colliderRatio = new Vector2(26.675f, 20f);
        // camCollider.size = colliderRatio * (camera.m_Lens.OrthographicSize / 10);
    }


    private void OnTriggerStay2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            camera.Priority = 15;
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player"))
        {
            camera.Priority = 1;
        }
    }
}
