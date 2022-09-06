using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BeatBoxParent : MonoBehaviour
{
    public int numBarsPerGroup = 16;
    public float gapBetweenBars = 0;
    public int numGroups = 8;
    public int audioIndexOffset = 2;
    public int verticalLayerCount = 1;
    public float maxHeightPerLayer = 8f;
    public float heightBoostMultiplier = 3f;
    public float distanceBetweenVerticalLayers = 45f;
    public float boxRotation = 0f;
    public float minHeightBooster = 1f;
    public float backdropHeightMultiplier = 2.5f;
    public bool useWavePattern;
    
    public GameObject beatBoxPrefab;
    public Vector2 scaleFactor = Vector2.one;

    private float barWidth;
    // Start is called before the first frame update
    void Start()
    {
        int[] indexArray = new int[numBarsPerGroup];
        Random rand = new Random();
        for (int bar = 0; bar < numBarsPerGroup; bar++)
        {
            indexArray[bar] = bar + audioIndexOffset;
        }
        
        barWidth = beatBoxPrefab.transform.localScale.x;
        Vector3 spawnLocation = Vector3.zero;
        int count = 0;
        for (int g = 0; g < numGroups; g++)
        {
            Shuffle(rand, indexArray);
            for (int i = 0; i < numBarsPerGroup; i++)
            {
                count++;
                float verticalAdd = 0f;
                for (int h = 0; h < verticalLayerCount; h++)
                {
                    //print("inst");
                    if (useWavePattern)
                    {
                        spawnLocation = new Vector3(spawnLocation.x, (Mathf.Sin(count / 2f) * 3f) + verticalAdd, spawnLocation.z);
                    }
                    else
                    {
                        spawnLocation = new Vector3(spawnLocation.x, verticalAdd, spawnLocation.z); 
                    }
                    
                    GameObject instantiatedBeatBox = Instantiate(beatBoxPrefab, transform, true);
                    instantiatedBeatBox.transform.position = transform.TransformPoint(spawnLocation);
                    instantiatedBeatBox.transform.eulerAngles = new Vector3(0, 0, boxRotation);
                    instantiatedBeatBox.GetComponent<BeatBox>().Initialize(indexArray[i], maxHeightPerLayer, heightBoostMultiplier, minHeightBooster, backdropHeightMultiplier);
                    verticalAdd += distanceBetweenVerticalLayers;
                }
                spawnLocation = new Vector3(spawnLocation.x + barWidth + gapBetweenBars, spawnLocation.y, spawnLocation.z);
            }
        }
        transform.localScale = scaleFactor;
    }
    
    
    //https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
    public void Shuffle<T> (Random rng, T[] array)
    {
        int n = array.Length;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            T temp = array[n];
            array[n] = array[k];
            array[k] = temp;
        }
    }

}
