using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatBox : MonoBehaviour
{
    public float maxheight = 0;
    public float heightMultiplier = 0;
    public float minLength;
    public float backdropHeight;
    public int spectrumIndex;

    public Transform mainBox;
    public Transform backdropA;
    public Transform backdropB;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {

        Vector3 scale = mainBox.transform.localScale;
        float height = Math.Min(heightMultiplier * AudioSpectrum.Instance.bufferSpectrum[spectrumIndex] + minLength, maxheight);
        mainBox.transform.localScale = new Vector3(scale.x, height,  scale.z);
        backdropA.localScale = new Vector3(backdropA.localScale.x, height + backdropHeight, backdropA.localScale.z);
        backdropB.localScale = new Vector3(backdropB.localScale.x, height + (backdropHeight * 2), backdropB.localScale.z);
        //print(string.Join (" , ", AudioSpectrum.Instance.bufferSpectrum));
    }

    public void Initialize(int index, float maxHeight, float heightMultiplier, float minLength, float backdropHeight)
    {
        spectrumIndex = index;
        this.maxheight = maxHeight;
        this.heightMultiplier = heightMultiplier;
        this.minLength = minLength;
        this.backdropHeight = backdropHeight;
    }
}
