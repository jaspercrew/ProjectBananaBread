using Cinemachine;
using UnityEngine;
using Random = UnityEngine.Random;
public class ScreenShakeController : MonoBehaviour {
    public static ScreenShakeController Instance;
    private CinemachineVirtualCamera virtualCamera;
    private float shakeTimer;
    


    //public float rotationMultiplier = 10f;
    // Start is called before the first frame update
    private void Start() {
        virtualCamera = GetComponent<CinemachineVirtualCamera>();
        Instance = this;
    }
    

    

    public void StartShake(float length, float power) {
        CinemachineBasicMultiChannelPerlin perlin =
            virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
        perlin.m_AmplitudeGain = power;
        shakeTimer = length;
        Debug.Log("shake start");
    }
    
    
    // Update is called once per frame
    void Update() {
        if (shakeTimer > 0) {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f) {
                virtualCamera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>().m_AmplitudeGain = 0f;
                
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