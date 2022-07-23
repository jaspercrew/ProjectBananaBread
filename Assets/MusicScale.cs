using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicScale : MonoBehaviour
{
    private const float maxheight = 8f;
    private const float heightMultiplier = 2f;
    private const float minLength = .2f;
    private float backdropHeight = 1.75f;
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

    public void Initialize(int index)
    {
        spectrumIndex = index;
    }
}
