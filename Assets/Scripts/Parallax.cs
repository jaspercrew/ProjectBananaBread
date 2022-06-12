using UnityEngine;

public class Parallax : MonoBehaviour
{
    public Camera cam;
    [Header("0 = follows camera exactly, 1 = does not move at all")]
    [SerializeField]
    public float parallaxEffectX;

    private Vector3 lastCameraPos;

    private void Start()
    {
        cam = Camera.main;
        lastCameraPos = cam.transform.position;
    }
    
    private void FixedUpdate()
    {
        Vector3 pos = cam.transform.position;
        float dx = pos.x - lastCameraPos.x;
        transform.position += dx * (1 - parallaxEffectX) * Vector3.right;
        lastCameraPos = pos;
    }
}