using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

//DEPRECATED
public class CameraFollow : MonoBehaviour {
    public Transform target;
    public float smoothTime = 0.3F;
    private Vector3 velocity = Vector3.zero;
    private Vector3 offset = new Vector3(0, 5, -10);
    public Vector3 targetPosition;
     
    void Update()
    {
        // Define a target position above and behind the target transform
        //Assert.IsTrue(target.transform.position.z == 0);
        //Vector3 targetPosition = target.TransformPoint(new Vector3(0, 5, -10));
        targetPosition = target.position + offset;
        //Debug.Log(targetPosition + " " + target.position);
     
        // Smoothly move the camera towards that target position
        transform.position = Vector3.SmoothDamp(transform.position, targetPosition, ref velocity, smoothTime);
    }
}
