using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public class GameArea : MonoBehaviour
{
    public bool[] audioLayers;
    
    [HideInInspector]
    public PolygonCollider2D polygonCollider2D;
    private CinemachineVirtualCamera cam;


    void Awake()
    {
        cam = GetComponentInChildren<CinemachineVirtualCamera>();
        cam.Follow = FindObjectOfType<CharController>().transform;
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }
    // Start is called before the first frame update
    void Start()
    {
        
        if (polygonCollider2D.bounds.Contains(CharController.Instance.transform.localPosition))
        {
            AudioManager.Instance.UpdateCurrentSongs(audioLayers, false);
        }
    }

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
            cam.Priority = 15;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            cam.Priority = 1;
        }
    }
}


