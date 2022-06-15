using System;
using System.Collections.Generic;
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

    public const float SceneTransitionTime = 0.55f;

    [HideInInspector]
    public Animator sceneFadeAnim;

    public bool isDarkScene;
    public bool isGravityScene;
    public bool isWindScene;
    public WindInfo realStateWind;
    public WindInfo altStateWind;
    public SoundName songA;
    public SoundName songB;
    public bool playMusic;

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
        sceneFadeAnim = GetComponentInChildren<Animator>();
        
        // Debug.Log("loading exit info");
        foreach (ExitToSpawn exitMapping in exitMappings)
        {
            if (exitMapping != null)
            {
                //print(exitMapping.exitName + " maps to " + exitMapping.spawnObj.name);
                spawnPositions[exitMapping.exitName] = exitMapping.spawnObj.transform.position;
            }
            
        }
    }

    private void Start()
    {
        if (playMusic)
        {
            AudioManager.Instance.PlaySong(songA, songB);
        }
    }

    public Vector3 GetSpawnPos()
    {
        //print(spawnPositions);
        
        string e = SceneTransitionManager.Instance.lastExitTouched;
        //print(e);
        
        if (spawnPositions.ContainsKey(e))
        {
            Debug.Log("exit " + e + " maps to " + spawnPositions[e]);
            return spawnPositions[e];
        }

        if (e != "")  {
            Debug.LogError("exit " + e + " is not present in this scene's " +
                           "exit-to-spawn mapping -- using default!");
        }
        return defaultSpawn.position;
    }
}
