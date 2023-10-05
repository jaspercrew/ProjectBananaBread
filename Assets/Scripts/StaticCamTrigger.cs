/*
using System;
using Cinemachine;
using UnityEngine;

public class StaticCamTrigger : MonoBehaviour
{
    private CinemachineVirtualCamera cam;
    private BoxCollider2D camCollider;

    private void Start()
    {
        cam = GetComponent<CinemachineVirtualCamera>();
        //camCollider = GetComponent<BoxCollider2D>();
        // Vector2 colliderRatio = new Vector2(26.675f, 20f);
        // camCollider.size = colliderRatio * (cam.m_Lens.OrthographicSize / 10);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cam.Priority = 15;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            cam.Priority = 1;
        }
    }
}
*/

