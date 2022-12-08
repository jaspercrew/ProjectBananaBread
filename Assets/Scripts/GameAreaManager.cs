using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameAreaManager : MonoBehaviour
{
    public GameObject gameAreaPrefab;
    private GameObject instantiatedGameArea;
    public float generalY;
    public float initialX;
    private const float maxHeight = 150f;
    public List<float> xVals = new List<float>();
    public List<int> songLayers = new List<int>();
    public int totalLayers;
    
    // Start is called before the first frame update
    void Start()
    {
        // if (xVals.Count != songLayers.Count + 1)
        // {
        //     
        // }
        int numAreas = xVals.Count;
        for (int i = 0; i < numAreas; i++)
        {
            instantiatedGameArea =
                Instantiate(gameAreaPrefab, Vector3.zero, Quaternion.Euler(Vector3.zero), transform);
            GameArea gameArea = instantiatedGameArea.GetComponent<GameArea>();
            float xToUseA = i == 0 ? initialX : xVals[i - 1];
            float xToUseB = xVals[i];
            Vector2[] pointsToSet = new Vector2[4];
            pointsToSet[0] = new Vector2(xToUseA, generalY + maxHeight);
            pointsToSet[1] = new Vector2(xToUseB, generalY + maxHeight);
            pointsToSet[2] = new Vector2(xToUseB, generalY - maxHeight);
            pointsToSet[3] = new Vector2(xToUseA, generalY - maxHeight);
            gameArea.polygonCollider2D.points = pointsToSet;
            // gameArea.polygonCollider2D.size =
            //     new Vector2(Mathf.Abs(i == 0 ? initialX - xVals[0] : xVals[i] - xVals[i + 1]), maxHeight);

            for (int j = 0; j < totalLayers; j++)
            {
                gameArea.audioLayers[j] = j < songLayers[i];
            }
            
        }
    }

    void OnDrawGizmos()
    {
        Gizmos.DrawLine(new Vector3(initialX, generalY - maxHeight, 0), new Vector3(initialX, generalY + maxHeight, 0));
        foreach (float x in xVals)
        {
            Gizmos.DrawLine(new Vector3(x, generalY - maxHeight, 0), new Vector3(x, generalY + maxHeight, 0));
        }
    }
    
    
    
    
    
}
