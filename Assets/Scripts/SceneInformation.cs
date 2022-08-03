using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
// public class WindInfo
// { 
//     public float forceStrength; 
//     public float speedOnPlayer; 
//     public bool isHorizontal;
// }

public enum SpawnPickDirection
{
    Leftmost, Rightmost, Highest, Lowest
}

[Serializable]
public class ExitToNextSpawn
{
    public Transform exitTrigger;
    public UnityEngine.Object destinationScene;
    public SpawnPickDirection spawnToPick;
}

[Serializable]
public class SpawnOverride
{
    public string previousExitName;
    public Transform spawn;
}

public class SceneInformation : MonoBehaviour
{
    public static SceneInformation Instance;

    public const float SceneTransitionTime = 0.55f;

    [HideInInspector]
    public Animator sceneFadeAnim;
    
    // public bool isGravityScene;
    // public bool isWindScene;
    public SoundName song;

    public bool playMusic;

    public Transform defaultSpawn;
    [Header("This configures exits from this scene")]
    public List<ExitToNextSpawn> exitMappings;

    private readonly Dictionary<Transform, ExitToNextSpawn> exitToScene = 
        new Dictionary<Transform, ExitToNextSpawn>();

    [Header("This configures any overrides for entrances to this scene")]
    public List<SpawnOverride> spawnOverrides;

    private readonly Dictionary<string, Transform> exitToSpawnOverride = new Dictionary<string, Transform>();

    private Vector3 leftmostSpawn = new Vector3(float.PositiveInfinity, 0, 0);
    private Vector3 rightmostSpawn = new Vector3(float.NegativeInfinity, 0, 0);
    private Vector3 topSpawn = new Vector3(0, float.NegativeInfinity, 0);
    private Vector3 bottomSpawn = new Vector3(0, float.PositiveInfinity, 0);

    // Start is called before the first frame update
    private void Awake()
    {
        Instance = this;
        sceneFadeAnim = GetComponentInChildren<Animator>();

        // get spawn bounds
        Transform spawnsParent = transform.Find("Spawns");
        foreach (Transform child in spawnsParent)
        {
            Vector3 pos = child.transform.position;
            if (pos.x < leftmostSpawn.x)
            {
                leftmostSpawn = pos;
            }
            if (pos.x > rightmostSpawn.x)
            {
                rightmostSpawn = pos;
            }
            if (pos.y > topSpawn.y)
            {
                topSpawn = pos;
            }
            if (pos.y < bottomSpawn.y)
            {
                bottomSpawn = pos;
            }
        }

        // convert exit
        foreach (ExitToNextSpawn exitMapping in exitMappings)
        {
            if (exitMapping != null)
            {
                exitToScene[exitMapping.exitTrigger] = exitMapping;
            }
        }

        foreach (SpawnOverride spawnOverride in spawnOverrides)
        {
            if (spawnOverride != null)
            {
                exitToSpawnOverride[spawnOverride.previousExitName] = spawnOverride.spawn;
            }
        }
    }

    public ExitToNextSpawn SceneInfoForExit(Transform exit)
    {
        if (exitToScene.ContainsKey(exit))
        {
            return exitToScene[exit];
        }

        Debug.LogError("exit " + exit.gameObject.name + " is not configured in scene info!");
        return new ExitToNextSpawn();
    }

    private void Start()
    {
        // if (playMusic)
        // {
        //     //Debug.Log("playing");
        //     AudioManager.Instance.PlaySong(song);
        // }
    }

    private void Update()
    {

    }

    public Vector3 GetSpawnPos()
    {
        if (!SceneTransitionManager.Instance.LastExitInfo.HasBeenSet)
        {
            Debug.LogWarning("no last exit info, assuming first spawn, using default spawn");
            return defaultSpawn.position;
        }
    
        string exitName = SceneTransitionManager.Instance.LastExitInfo.exitName;

        if (exitToSpawnOverride.ContainsKey(exitName))
        {
            Debug.Log("using override entrance spawn for previous exit " + exitName);
            return exitToSpawnOverride[exitName].position;
        }
        
        SpawnPickDirection dir = SceneTransitionManager.Instance.LastExitInfo.SpawnPickDirection;

        switch (dir)
        {
            case SpawnPickDirection.Leftmost:
                return leftmostSpawn;
            case SpawnPickDirection.Rightmost:
                return rightmostSpawn;
            case SpawnPickDirection.Highest:
                return topSpawn;
            case SpawnPickDirection.Lowest:
                return bottomSpawn;
            default:
                Debug.LogError("could not pick spawn direction from previous scene, " +
                               "using default spawn!");
                return defaultSpawn.position;
        }
    }
}
