using UnityEngine;
using Random = System.Random;

public class BeatBoxParent : MonoBehaviour
{
    public int numBarsPerGroup = 16;
    public float gapBetweenBars;
    public int numGroups = 8;
    public int audioIndexOffset = 2;
    public int verticalLayerCount = 1;
    public float maxHeightPerLayer = 8f;
    public float heightBoostMultiplier = 3f;
    public float distanceBetweenVerticalLayers = 45f;
    public float boxRotation;
    public float minHeightBooster = 1f;
    public float backdropHeightMultiplier = 2.5f;
    public bool useWavePattern;
    public Vector3 overallRotation;
    
    public GameObject beatBoxPrefab;
    public GameObject beatBoxSubParentPrefab;
    public Vector2 scaleFactor = Vector2.one;

    private float barWidth;
    // Start is called before the first frame update
    private void Start()
    {
        gapBetweenBars += 1 / (Mathf.Cos(boxRotation * Mathf.Deg2Rad)) - 1;

        int[] indexArray = new int[numBarsPerGroup];
        Random rand = new Random();
        for (int bar = 0; bar < numBarsPerGroup; bar++)
        {
            indexArray[bar] = bar + audioIndexOffset;
        }
        
        barWidth = beatBoxPrefab.transform.localScale.x;
        Vector3 spawnLocation = Vector3.zero;

        float groupWidth = numBarsPerGroup * (barWidth + gapBetweenBars);
        
        int count = 0;
        for (int g = 0; g < numGroups; g++)
        {
            Shuffle(rand, indexArray);
            GameObject subParent = Instantiate(beatBoxSubParentPrefab, transform, false);
            subParent.transform.position = transform.TransformPoint(spawnLocation + groupWidth / 2 * Vector3.right);
            
            for (int i = 0; i < numBarsPerGroup; i++)
            {
                count++; // TODO: can't we just use i and q?
                float verticalAdd = 0f; // TODO: can't we just use h?
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
                    
                    GameObject instantiatedBeatBox = Instantiate(beatBoxPrefab, subParent.transform, true);
                    instantiatedBeatBox.transform.position = subParent.transform.TransformPoint(spawnLocation);
                    instantiatedBeatBox.transform.eulerAngles = new Vector3(0, 0, boxRotation);
                    instantiatedBeatBox.GetComponent<BeatBox>().Initialize(indexArray[i], maxHeightPerLayer, 
                        heightBoostMultiplier, minHeightBooster, backdropHeightMultiplier);
                    verticalAdd += distanceBetweenVerticalLayers;
                }
                
                spawnLocation = new Vector3(spawnLocation.x + barWidth + gapBetweenBars, spawnLocation.y, spawnLocation.z);
            }
        }
        transform.localScale = scaleFactor;
        transform.localRotation = Quaternion.Euler(overallRotation.x, overallRotation.y, overallRotation.z);
    }
    
    
    // https://stackoverflow.com/questions/108819/best-way-to-randomize-an-array-with-net
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
