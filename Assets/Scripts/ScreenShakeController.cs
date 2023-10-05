using Cinemachine;
using UnityEngine;

public class ScreenShakeController : BeatEntity
{
    public static ScreenShakeController instance;
    public CinemachineVirtualCamera virtualCamera;
    private float shakeTimer;
    private float shakeTimerTotal;
    private float startingIntensity;

    //public float rotationMultiplier = 10f;
    private void Awake()
    {
        instance = this;
    }

    // Update is called once per frame
    private void Update()
    {
        if (GetComponent<CinemachineBrain>().ActiveVirtualCamera is null) return;
        virtualCamera =
            GetComponent<CinemachineBrain>().ActiveVirtualCamera.VirtualCameraGameObject
                .GetComponent<CinemachineVirtualCamera>();
        if (shakeTimer > 0)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
                virtualCamera
                    .GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>()
                    .m_AmplitudeGain = Mathf.Lerp(
                    startingIntensity,
                    0f,
                    1 - shakeTimer / shakeTimerTotal
                );
        }

        virtualCamera.transform.localRotation = Quaternion.Euler(
            0,
            0,
            virtualCamera.transform.localRotation.z
        );
    }

    // Start is called before the first frame update


    public void StartShake(float length, float power)
    {
        var perlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        if (perlin is null) return;
        perlin.m_AmplitudeGain = power;
        startingIntensity = power;
        shakeTimer = length;
        shakeTimerTotal = length;
        //Debug.Log("shake start");
    }

    protected override void MicroBeatAction()
    {
        if (!GameManager.instance.isMenu) MediumShake();
    }

    public void MediumShake()
    {
        StartShake(.07f, 4f);
    }

    public void LargeShake()
    {
        StartShake(.13f, 6f);
    }

    public void LightShake()
    {
        StartShake(.1f, 1.6f);
    }
}