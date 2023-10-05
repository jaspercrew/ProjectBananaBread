using UnityEngine;

public class LastExitInfo
{
    public string exitName;
    public bool hasBeenSet;

    // public Object DestinationScene;
    public SpawnPickDirection spawnPickDirection;

    public LastExitInfo()
    {
    }

    public LastExitInfo(ExitToNextSpawn e)
    {
        hasBeenSet = true;
        exitName = e.exitTrigger.gameObject.name;
        // DestinationScene = e.destinationScene;
        spawnPickDirection = e.spawnToPick;
    }
}

public class SceneTransitionManager : MonoBehaviour
{
    public static SceneTransitionManager instance;
    public int checkPointToUse = -1;
    public LastExitInfo lastExitInfo = new LastExitInfo();

    private void Awake()
    {
        if (instance == null)
            instance = this;
        else
            Destroy(gameObject);
    }
}