using System;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
[Serializable]
public class WindInfo
{ 
    public float forceStrength; 
    public float speedOnPlayer; 
    public bool isHorizontal;
}

public class SceneInformation : MonoBehaviour
{
    public static SceneInformation Instance;

    public bool isDarkScene;
    public bool isGravityScene;
    public bool isWindScene;
    public WindInfo realStateWind;
    public WindInfo altStateWind;

    [Serializable]
    public class ExitToSpawn
    {
        public string exitName;
        public GameObject spawnObj;
    }

    public Transform defaultSpawn;
    public List<ExitToSpawn> exitMappings;
    private readonly Dictionary<string, Vector3> spawnPositions = new Dictionary<string, Vector3>();

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        
        // Debug.Log("loading exit info");
        foreach (ExitToSpawn exitMapping in exitMappings)
        {
            if (exitMapping != null)
            {
                print(exitMapping.exitName + " maps to " + exitMapping.spawnObj.name);
                spawnPositions[exitMapping.exitName] = exitMapping.spawnObj.transform.position;
            }
            
        }
    }

    public Vector3 GetSpawnPos()
    {
        string e = GameManager.Instance.lastExitTouched;
        
        if (spawnPositions.ContainsKey(e))
        {
            // Debug.LogWarning("exit " + e + " maps to " + spawnPositions[e]);
            return spawnPositions[e];
        }

        if (e != "")  {
            Debug.LogError("exit " + e + " is not present in this scene's " +
                           "exit-to-spawn mapping -- using default!");
        }
        return defaultSpawn.position;
    }
}
