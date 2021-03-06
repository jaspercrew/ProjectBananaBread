using UnityEngine;

//DEPRECATED
public class CameraFollow : MonoBehaviour {
    public Transform target;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset = new Vector3(0, 5, -10);
    public Vector3 targetPosition;
     
    void Update()
    {
        targetPosition = target.position + offset;

        // Smoothly move the camera towards that target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
