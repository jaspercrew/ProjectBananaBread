using System;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;



public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;
    public CinemachineVirtualCamera currentCam;
    

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
    }

    private void Update()
    {
        currentCam = GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject
            .GetComponent<CinemachineVirtualCamera>();
    }

    // public void SwitchCam(CinemachineVirtualCamera cam) {
    //     currentCam.Priority = 5;
    //     cam.Priority = 15;
    //     currentCam = cam;
    // }
}
