using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameArea : MonoBehaviour
{
    private const int numLayers = 4;
    public bool[] audioLayers = new bool[numLayers];
    private PolygonCollider2D polygonCollider2D;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            AudioManager.Instance.UpdateCurrentSongs(audioLayers);
            SaveData.SaveToFile(1);
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
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
