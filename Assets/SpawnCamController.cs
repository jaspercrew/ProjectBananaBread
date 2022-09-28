using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnCamController : BeatEntity
{
    private Camera mainCamera;
    private Camera spawnCamera;
    public int numSlices = 8;
    private bool doBeats;

    private int transitionProgression;
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        mainCamera = CameraManager.Instance.transform.GetComponent<Camera>();
        spawnCamera = transform.GetComponent<Camera>();
        ResetTransition();
        StartCoroutine(TestTransition());

    }

    private void ResetTransition()
    {
        transitionProgression = 0;
        spawnCamera.rect = new Rect(1, 0, 0, 1);
        if (CharController.Instance.currentArea == null || CharController.Instance.currentArea.spawnLocation == null)
        {
            transform.position = SceneInformation.Instance.GetSpawnPos();
        }
        else
        {
            transform.position = CharController.Instance.currentArea.spawnLocation.position;
        }

        transform.position = new Vector3(transform.position.x, transform.position.y, -10);
    }

    public void ProgressTransition()
    {
        transitionProgression++;
        float sliceWidth = (1f / (float) numSlices);
        mainCamera.rect = new Rect(mainCamera.rect.x + sliceWidth / 2, 0, mainCamera.rect.width - sliceWidth, 1f);
        spawnCamera.rect = new Rect(spawnCamera.rect.x - sliceWidth, 0, spawnCamera.rect.width + sliceWidth, 1f);
        if (transitionProgression == 8) //reset transition assets
        {
            ResetTransition();
            doBeats = false;
        }

    }

    protected override void MicroBeatAction()
    {
        base.MicroBeatAction();
        if (!doBeats)
        {
            return;
        }
        ProgressTransition();
        
    }

    private IEnumerator TestTransition()
    {
        
        yield return new WaitForSeconds(5f);
        doBeats = true;

    }

    // Update is called once per frame
    void Update()
    {
        
    }
    
}
