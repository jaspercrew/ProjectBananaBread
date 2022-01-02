using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

// TODO: do we need this
public class ScreenShakeController : MonoBehaviour {
    public static ScreenShakeController Instance;
    
    private float _shakeTimeRemaining;
    private float _shakePower;
    private float _shakeFadeTime;
    private float _shakeRotation;
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
        if (_shakeTimeRemaining > 0) {
            _shakeTimeRemaining -= Time.deltaTime;

            float xAmount = Random.Range(-1f, 1f) * _shakePower;
            float yAmount = Random.Range(-1f, 1f) * _shakePower;

            transform.position += new Vector3(xAmount, yAmount, 0);

            _shakePower = Mathf.MoveTowards
                (_shakePower, 0f, _shakeFadeTime * Time.deltaTime);

            _shakeRotation = Mathf.MoveTowards(_shakeRotation, 0f, 
                _shakeFadeTime * rotationMultiplier * Time.deltaTime);
            
        }
        transform.rotation = Quaternion.Euler(0f, 0f, _shakeRotation * Random.Range(-1f, 1f));
    }

    public void StartShake(float length, float power) {
        _shakeTimeRemaining = length;
        _shakePower = power;

        _shakeFadeTime = power / length;

        _shakeRotation = power * rotationMultiplier;
    }

    public void LightShake() {
        StartShake(.1f, .2f);
    }
}