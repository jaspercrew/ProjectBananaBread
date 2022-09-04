using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAreaController : MonoBehaviour
{
    public Transform spawnLocation;
    public bool isRotationArea = false;
    public bool isRewindArea = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharController.Instance.currentArea = this;
        }
    }
}
