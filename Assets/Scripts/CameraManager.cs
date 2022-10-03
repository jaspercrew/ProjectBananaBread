using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;


public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;
    public CinemachineVirtualCamera currentCam;
    public GameObject slicePrefab;
    private Camera unityCam;
    private TransitionSlice[] slices;
    private const int numSlices = 8;
    private const float initialDelay = .3f;
    private const float arppegioTime = .75f;
    private const float disappearDelay = .5f;
    public const float sliceFadeTime = .2f;
    public float totalDelayToSpawn;
    

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Start()
    {
        
        unityCam = GetComponent<Camera>();
        
        if (slicePrefab is null)
        {
            return;
        }
        
        totalDelayToSpawn = initialDelay + arppegioTime + disappearDelay / 2;
        slices = new TransitionSlice[numSlices];
        float distanceFromCamera = unityCam.nearClipPlane ; // Change this value if you want
        Vector3 midLeft = unityCam.ViewportToWorldPoint(new Vector3(0, .5f, distanceFromCamera));
        Vector3 midRight = unityCam.ViewportToWorldPoint(new Vector3(1, .5f, distanceFromCamera));
        Vector3 topMid = unityCam.ViewportToWorldPoint(new Vector3(.5f, 1f, distanceFromCamera));
        Vector3 botMid = unityCam.ViewportToWorldPoint(new Vector3(.5f, 0f, distanceFromCamera));
        
        Vector3 sliceGap = new Vector3((midRight.x - midLeft.x) / (numSlices + 1), 0, 0);
        float camHeight = topMid.y - botMid.y;
        for (int i = 0; i < numSlices; i++)
        {
            Vector3 spawnPoint = midLeft + sliceGap * (i + 1);
            GameObject clone = Instantiate(slicePrefab, spawnPoint, Quaternion.identity, transform);
            clone.transform.localScale = new Vector3(sliceGap.x * 2, camHeight, 1f);
            slices[i] = clone.GetComponent<TransitionSlice>();
        }
    }

    private void Update()
    {
        if (GetComponent<CinemachineBrain>().ActiveVirtualCamera is null)
        {
            return;
        }
        currentCam = GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject
            .GetComponent<CinemachineVirtualCamera>();
        
    }

    public void DoTransition(bool invertDirection = false)
    {
        StartCoroutine(TransitionCoroutine(invertDirection));
    }

    private IEnumerator TransitionCoroutine(bool invertDirection)
    {
        CharController.Instance.disabledMovement = true;
        yield return new WaitForSeconds(initialDelay);
        float sliceInterval = arppegioTime / numSlices;

        for (int i = 0; i < numSlices; i++)
        {
            yield return new WaitForSeconds(sliceInterval);
            slices[i].Appear();
        }

        yield return new WaitForSeconds(disappearDelay);

        if (!invertDirection)
        {
            for (int i = 0; i < numSlices; i++)
            {
                yield return new WaitForSeconds(sliceInterval);
                slices[i].Disappear();
            }
        }
        else
        {
            for (int i = numSlices - 1; i >= 0; i--)
            {
                yield return new WaitForSeconds(sliceInterval);
                slices[i].Disappear();
            }
        }

        CharController.Instance.disabledMovement = false;



    }

    // public void SwitchCam(CinemachineVirtualCamera cam) {
    //     currentCam.Priority = 5;
    //     cam.Priority = 15;
    //     currentCam = cam;
    // }
}
