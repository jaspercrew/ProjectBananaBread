using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraSwitchTrigger : MonoBehaviour {
    public CameraName enterCam;
    public CameraName exitCam = CameraName.CharacterCam;

    private void OnTriggerEnter2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            CameraManager.Instance.SwitchCam(enterCam);
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
        if (other.CompareTag("Player")) {
            CameraManager.Instance.SwitchCam(exitCam);
        }
    }
}
