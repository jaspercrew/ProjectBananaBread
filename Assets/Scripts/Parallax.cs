using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Parallax : MonoBehaviour
{
    private float length, startpos;
    [SerializeField]
    public GameObject cam;

    private const float parallaxEffect = .3f;

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
    
        float temp = (cam.transform.position.x * (1 - parallaxEffect));
        float dist = (cam.transform.position.x * parallaxEffect);
        transform.position = new Vector3(startpos + dist, transform.position.y, transform.position.z);
        if (temp > startpos + length) startpos += length;
        else if (temp < startpos - length) startpos -= length;
    }
}