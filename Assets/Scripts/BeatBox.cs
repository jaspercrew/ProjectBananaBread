using System;
using UnityEngine;

public class BeatBox : MonoBehaviour
{
    public float maxHeight;
    public float heightMultiplier;
    public float minLength;
    public float backdropHeight;
    public int spectrumIndex;

    public Transform mainBox;
    public Transform backdropA;
    public Transform backdropB;

    // private bool wasChangingLastFrame;
    public bool doChanging;

    private bool wasZeroLastFrame;

    // Update is called once per frame
    private void Update()
    {
        if (doChanging) // changed by sub parent (group parent)
        {
            // if (!wasChangingLastFrame)
            // {
            //     Debug.Log("started changing");
            // }
            var height = Math.Min(
                heightMultiplier * AudioSpectrum.instance.bufferSpectrum[spectrumIndex] + minLength,
                maxHeight
            );
            var sM = mainBox.localScale;
            var sA = backdropA.localScale;
            var sB = backdropB.localScale;
            sM = new Vector3(sM.x, doChanging ? height + 0 * backdropHeight : 0, sM.z);
            sA = new Vector3(sA.x, doChanging ? height + 1 * backdropHeight : 0, sA.z);
            sB = new Vector3(sB.x, doChanging ? height + 2 * backdropHeight : 0, sB.z);
            mainBox.localScale = sM;
            backdropA.localScale = sA;
            backdropB.localScale = sB;
            // wasChangingLastFrame = true;
            wasZeroLastFrame = false;
        }
        else if (!wasZeroLastFrame)
        {
            var sM = mainBox.localScale;
            var sA = backdropA.localScale;
            var sB = backdropB.localScale;
            sM = new Vector3(sM.x, 0, sM.z);
            sA = new Vector3(sA.x, 0, sA.z);
            sB = new Vector3(sB.x, 0, sB.z);
            mainBox.localScale = sM;
            backdropA.localScale = sA;
            backdropB.localScale = sB;
            wasZeroLastFrame = true;
            // wasChangingLastFrame = false;
        }
        // already zero, no need to do anything
    }

    public void Initialize(
        int index,
        float height,
        float heightMult,
        float minLen,
        float backHeight
    )
    {
        spectrumIndex = index;
        maxHeight = height;
        heightMultiplier = heightMult;
        minLength = minLen;
        backdropHeight = backHeight;
    }
}