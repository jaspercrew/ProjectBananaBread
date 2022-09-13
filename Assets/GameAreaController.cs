using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class GameAreaController : MonoBehaviour
{
    public Transform spawnLocation;
    public bool useCamera;

    // Start is called before the first frame update
    void Start()
    {
        GetComponentInChildren<CinemachineVirtualCamera>().enabled = useCamera;

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharController.Instance.currentArea = this;
        }
    }
}
