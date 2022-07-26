using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BeatBoxParent : MonoBehaviour
{
    public int numBars = 16;
    public float barGap = 0;
    public int numGroups = 8;
    public int indexOffset = 2;
    public GameObject beatBoxPrefab;

    private float barWidth;
    // Start is called before the first frame update
    void Start()
    {
        int[] indexArray = new int[numBars];
        Random rand = new Random();
        for (int bar = 0; bar < numBars; bar++)
        {
            indexArray[bar] = bar + indexOffset;
        }
        
        barWidth = beatBoxPrefab.transform.localScale.x;
        Vector3 spawnLocation = Vector3.zero;
        for (int g = 0; g < numGroups; g++)
        {
            Shuffle(rand, indexArray);
            for (int i = 0; i < numBars; i++)
            {
                print("inst");
                GameObject instantiatedBeatBox = Instantiate(beatBoxPrefab, transform, true);
                instantiatedBeatBox.transform.position = transform.TransformPoint(spawnLocation);
                instantiatedBeatBox.transform.eulerAngles = new Vector3(0, 0, 0);
                instantiatedBeatBox.GetComponent<MusicScale>().Initialize(indexArray[i]);
                spawnLocation = new Vector3(spawnLocation.x + barWidth + barGap, spawnLocation.y, spawnLocation.z);
            }
        }
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
