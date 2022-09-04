using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

public class BeatBoxParent : MonoBehaviour
{
    public int numBars = 16;
    public float barGap = 0;
    public int numGroups = 8;
    public int indexOffset = 2;
    public int verticalSize = 1;
    public float maxheight = 8f;
    public float heightMultiplier = 3f;
    public float verticalLayerOffset = 45f;
    public float boxRotation = 0f;
    public float minLength = 1f;
    public float backdropHeight = 2.5f;
    
    public GameObject beatBoxPrefab;
    public Vector2 scaleFactor = Vector2.one;

    private float barWidth;
    
    // Start is called before the first frame update
    private void Start()
    {
        int[] indexArray = new int[numBars];
        Random rand = new Random();
        for (int bar = 0; bar < numBars; bar++)
        {
            indexArray[bar] = bar + indexOffset;
        }
        
        barWidth = beatBoxPrefab.transform.localScale.x;
        Vector3 spawnLocation = Vector3.zero;
        int count = 0;
        for (int g = 0; g < numGroups; g++)
        {
            Shuffle(rand, indexArray);
            for (int i = 0; i < numBars; i++)
            {
                count++;
                float verticalAdd = 0f;
                for (int h = 0; h < verticalSize; h++)
                {
                    //print("inst");
                    spawnLocation = new Vector3(spawnLocation.x, (Mathf.Sin(count / 2f) * 3f) + verticalAdd, spawnLocation.z);
                    GameObject instantiatedBeatBox = Instantiate(beatBoxPrefab, transform, true);
                    instantiatedBeatBox.transform.position = transform.TransformPoint(spawnLocation);
                    instantiatedBeatBox.transform.eulerAngles = new Vector3(0, 0, boxRotation);
                    instantiatedBeatBox.GetComponent<BeatBox>()
                        .Initialize(indexArray[i], maxheight, heightMultiplier, minLength, backdropHeight);
                    verticalAdd += verticalLayerOffset;
                }
                spawnLocation = new Vector3(spawnLocation.x + barWidth + barGap, spawnLocation.y, spawnLocation.z);
            }
        }
        transform.localScale = scaleFactor;
    }
    
    
    // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
    private static void Shuffle<T>(Random rng, IList<T> array)
    {
        int n = array.Count;
        while (n > 1) 
        {
            int k = rng.Next(n--);
            (array[n], array[k]) = (array[k], array[n]);
        }
    }

}
