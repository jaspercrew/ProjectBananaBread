using UnityEngine;

public class LastExitInfo
{
    public bool HasBeenSet = false;
    public string exitName;
    // public Object DestinationScene;
    public SpawnPickDirection SpawnPickDirection;

    public LastExitInfo()
    {
        
    }

    public LastExitInfo(ExitToNextSpawn e)
    {
        HasBeenSet = true;
        exitName = e.exitTrigger.gameObject.name;
        // DestinationScene = e.destinationScene;
        SpawnPickDirection = e.spawnToPick;
    }
}

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager Instance;
    public LastExitInfo LastExitInfo = new LastExitInfo();

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
