using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCamController : MonoBehaviour
{
    private Camera mainCamera;
    private Camera spawnCamera;
    public int numSlices = 8;
    // Start is called before the first frame update
    void Start()
    {
        mainCamera = CameraManager.Instance.transform.GetComponent<Camera>();
        spawnCamera = transform.GetComponent<Camera>();
        if (CharController.Instance.currentArea == null || CharController.Instance.currentArea.spawnLocation == null)
        {
            transform.position = SceneInformation.Instance.GetSpawnPos();
        }
        else
        {
            transform.position = CharController.Instance.currentArea.spawnLocation.position;
        }

        StartCoroutine(TestTransition());

    }

    public void ProgressTransition()
    {
        float sliceWidth = (1f / (float) numSlices);
        mainCamera.rect = new Rect(mainCamera.rect.x + sliceWidth / 2, 0, mainCamera.rect.width - sliceWidth, 1f);
        spawnCamera.rect = new Rect(spawnCamera.rect.x - sliceWidth, 0, spawnCamera.rect.width + sliceWidth, 1f);

    }

    private IEnumerator TestTransition()
    {
        spawnCamera.rect = new Rect(1, 0, 0, 1);
        yield return new WaitForSeconds(5f);
        for (int i = 0; i < numSlices; i++)
        {
            ProgressTransition();
            yield return new WaitForSeconds(1f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
