using System;
using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;

public enum CameraName{
    CharacterCam, StableCam
}


public class CameraManager : MonoBehaviour {
    public static CameraManager Instance;
    private Dictionary<CameraName, CinemachineVirtualCamera> nameToCam = new Dictionary<CameraName, CinemachineVirtualCamera>();
    private CameraName currentCam;
    

    private void Awake() {
        if (Instance != null) {
            Destroy(gameObject);
        }
        else {
            Instance = this;
        }
        
        foreach (CameraName camName in Enum.GetValues(typeof(CameraName)))
        {
            // TODO: set a naming scheme for cams
            CinemachineVirtualCamera cam = transform.Find(camName.ToString()).GetComponent<CinemachineVirtualCamera>();
            nameToCam[camName] = cam;
        }
    }

    public void SwitchCam(CameraName camName) {
        nameToCam[currentCam].Priority = 5;
        nameToCam[camName].Priority = 15;
        currentCam = camName;
    }
}
