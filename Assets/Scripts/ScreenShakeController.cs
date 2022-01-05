using UnityEngine;
using Random = UnityEngine.Random;

public class ScreenShakeController : MonoBehaviour {
    public static ScreenShakeController Instance;
    
    private float shakeTimeRemaining;
    private float shakePower;
    private float shakeFadeTime;
    private float shakeRotation;
    private Vector3 origin;

    public float rotationMultiplier = 10f;

    // Start is called before the first frame update
    private void Start() {
        origin = transform.position;
        Instance = this;
    }

    // Update is called once per frame
    void Update() {
        if (Input.GetKeyDown(KeyCode.K)) {
            StartShake(.1f, .2f);
        }

        transform.position = origin;
    }

    private void LateUpdate() {
        if (shakeTimeRemaining > 0) {
            shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * shakePower;
            float yAmount = Random.Range(-1f, 1f) * shakePower;

            transform.position += new Vector3(xAmount, yAmount, 0);

            shakePower = Mathf.MoveTowards
                (shakePower, 0f, shakeFadeTime * Time.deltaTime);

            shakeRotation = Mathf.MoveTowards(shakeRotation, 0f, 
                shakeFadeTime * rotationMultiplier * Time.deltaTime);
            
        }
        transform.rotation = Quaternion.Euler(0f, 0f, shakeRotation * Random.Range(-1f, 1f));
    }

    public void StartShake(float length, float power) {
        shakeTimeRemaining = length;
        shakePower = power;

        shakeFadeTime = power / length;

        shakeRotation = power * rotationMultiplier;
    }

    public void MediumShake() {
        StartShake(.1f, .2f);
    }
    
    public void LightShake() {
        StartShake(.1f, .08f);
    }
}