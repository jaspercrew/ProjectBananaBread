using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;

public class BeatBoxParent : MonoBehaviour
{
    public int numBarsPerGroup = 16;
    public float gapBetweenBars;
    [FormerlySerializedAs("numGroups")] public int numGroupsPerLayer = 8;
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
        Vector3 originalSpawnLocation = spawnLocation;

        // float groupWidth = numBarsPerGroup * (barWidth + gapBetweenBars);
        // float groupWidth = 0;

        // draw one row at a time
        for (int verticalLayer = 0; verticalLayer < verticalLayerCount; verticalLayer++)
        {
            int rowCount = 0; // change this to overallCount outside this for loop if we don't want the waves to line up
            
            // draw one group at a time in the row
            for (int group = 0; group < numGroupsPerLayer; group++)
            {
                // create sub-parent for the group
                GameObject subParent = Instantiate(beatBoxSubParentPrefab, transform, false);
                subParent.transform.position = transform.TransformPoint(spawnLocation);
                
                // randomize audio frequencies for this group
                Shuffle(rand, indexArray);
                
                // create the boxes in the group
                for (int bar = 0; bar < numBarsPerGroup; bar++)
                {
                    Vector3 waveSpawnLocation = spawnLocation + Mathf.Sin(rowCount / 2f) * 3f * Vector3.up;
                    rowCount++;
                    
                    // create box
                    GameObject instantiatedBeatBox = Instantiate(beatBoxPrefab, subParent.transform, true);
                    instantiatedBeatBox.transform.position =
                        transform.transform.TransformPoint(useWavePattern? waveSpawnLocation : spawnLocation);
                    instantiatedBeatBox.transform.eulerAngles = new Vector3(0, 0, boxRotation);
                    
                    instantiatedBeatBox.GetComponent<BeatBox>().Initialize(indexArray[bar], maxHeightPerLayer, 
                        heightBoostMultiplier, minHeightBooster, backdropHeightMultiplier);

                    // move right for next box
                    spawnLocation += (barWidth + gapBetweenBars) * Vector3.right;
                }
            }

            // move all the way back and up for next row
            spawnLocation = originalSpawnLocation + verticalLayer * distanceBetweenVerticalLayers * Vector3.up;
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
