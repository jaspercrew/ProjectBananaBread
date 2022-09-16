using System.Collections;
using Cinemachine;
using UnityEngine;

public class ScreenShakeController : BeatEntity {
    public static ScreenShakeController Instance;
    public CinemachineVirtualCamera virtualCamera;
    private float shakeTimer;
    private float startingIntensity;
    private float shakeTimerTotal;
    
    //public float rotationMultiplier = 10f;
    void Awake()
    {

        Instance = this;
    }
    // Start is called before the first frame update


    public void StartShake(float length, float power) {
        CinemachineBasicMultiChannelPerlin perlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = power;
        startingIntensity = power;
        shakeTimer = length;
        shakeTimerTotal = length;
        //Debug.Log("shake start");
    }

    protected override void MicroBeatAction()
    {
        if (!GameManager.Instance.isMenu)
        {
            MediumShake();
        }
    }

    // Update is called once per frame
    void Update() {
        if (GetComponent<CinemachineBrain>().ActiveVirtualCamera is null)
        {
            return;
        }
        virtualCamera = GetComponent<CinemachineBrain>().ActiveVirtualCamera.
            VirtualCameraGameObject.GetComponent<CinemachineVirtualCamera>();
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f) {
                virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 
                Mathf.Lerp(startingIntensity, 0f, 1 - (shakeTimer / shakeTimerTotal));

            }
        }
    }

    public void MediumShake() {
        StartShake(.07f, 4f);
    }
    
    public void LargeShake() {
        StartShake(.13f, 6f);
    }
    
    public void LightShake() {
        StartShake(.1f, 1.6f);
    }
}