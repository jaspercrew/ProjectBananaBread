using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startpos, lengthY, startposY;
    [SerializeField]
    public GameObject cam;

    public float parallaxEffect = .3f;

    void Start()
    {
        startpos = transform.position.x;
        length = GetComponent<SpriteRenderer>().bounds.size.x;
        StartCoroutine(LateStart());
        
    }

    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        if (cam == null)
        {
            cam = GameObject.FindWithTag("MainCamera");
        }
    }
    void Update()
    {
        if (cam == null)
            cam = GameObject.FindWithTag("MainCamera");
        
        //transform.position = new Vector3(transform.position.x, cam.transform.position.y, transform.position.z);
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
        
        // float tempY = (cam.transform.position.y * (1 - parallaxEffect));
        // float distY = (cam.transform.position.y * parallaxEffect);
        // transform.position = new Vector3(transform.position.x, startposY + distY, transform.position.z);
        // if (tempY > startposY + lengthY) startposY += lengthY;
        // else if (tempY < startposY - lengthY) startposY -= lengthY;
    }
}