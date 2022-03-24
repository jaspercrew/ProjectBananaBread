using System;
using System.Collections;
using Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;
public class ScreenShakeController : MonoBehaviour {
    public static ScreenShakeController Instance;
    public CinemachineVirtualCamera virtualCamera;
    private float shakeTimer;
    private float startingIntensity;
    private float shakeTimerTotal;
    
    //public float rotationMultiplier = 10f;
    // Start is called before the first frame update
    private void Start() {
        Instance = this;
        StartCoroutine(LateStart());
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        virtualCamera = GetComponent<CinemachineBrain>().ActiveVirtualCamera.
            VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
    }
    
    public void StartShake(float length, float power) {
        CinemachineBasicMultiChannelPerlin perlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = power;
        startingIntensity = power;
        shakeTimer = length;
        shakeTimerTotal = length;
        //Debug.Log("shake start");
    }
    
    // Update is called once per frame
    void Update() {

        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f) {
                virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 
                Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));

            }
        }
    }

    public void MediumShake() {
        StartShake(.1f, 4f);
    }
    
    public void LightShake() {
        StartShake(.1f, 1.6f);
    }
}