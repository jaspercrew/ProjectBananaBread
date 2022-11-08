using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameArea : MonoBehaviour
{
    private const int numLayers = 4;
    public bool[] audioLayers = new bool[numLayers];
    private PolygonCollider2D polygonCollider2D;
    private CinemachineVirtualCamera cam;
    public int cameraOverridePriority = 0;


    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.UpdateCurrentSongs(audioLayers);
            SaveData.SaveToFile(1);
        }
    }
    
    

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            cam.Priority = 15 + cameraOverridePriority;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            cam.Priority = 1;
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
        cam.Follow = FindObjectOfType<CharController>().transform;
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        if (polygonCollider2D.bounds.Contains(CharController.Instance.transform.localPosition))
        {
            AudioManager.Instance.UpdateCurrentSongs(audioLayers, false);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
