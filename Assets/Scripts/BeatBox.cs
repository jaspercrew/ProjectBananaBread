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

    private bool wasZeroLastFrame;
    private bool doChanging;
    
    // Start is called before the first frame update
    private void Start()
    {
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (doChanging)
        {
            float height = Math.Min(heightMultiplier * AudioSpectrum.Instance.bufferSpectrum[spectrumIndex] + minLength,
                maxHeight);
            Vector3 sM = mainBox.localScale;
            Vector3 sA = backdropA.localScale; 
            Vector3 sB = backdropB.localScale;
            sM = new Vector3(sM.x, doChanging? height + 0 * backdropHeight : 0, sM.z);
            sA = new Vector3(sA.x, doChanging? height + 1 * backdropHeight : 0, sA.z);
            sB = new Vector3(sB.x, doChanging? height + 2 * backdropHeight : 0, sB.z);
            mainBox.localScale = sM;
            backdropA.localScale = sA;
            backdropB.localScale = sB;
        } 
        else if (!wasZeroLastFrame)
        {
            Vector3 sM = mainBox.localScale;
            Vector3 sA = backdropA.localScale; 
            Vector3 sB = backdropB.localScale;
            sM = new Vector3(sM.x, 0, sM.z);
            sA = new Vector3(sA.x, 0, sA.z);
            sB = new Vector3(sB.x, 0, sB.z);
            mainBox.localScale = sM;
            backdropA.localScale = sA;
            backdropB.localScale = sB;
            wasZeroLastFrame = true;
        }
        else
        {
            // already zero, no need to do anything
        }
    }

    public void Initialize(int index, float height, float heightMult, float minLen, float backHeight)
    {
        spectrumIndex = index;
        maxHeight = height;
        heightMultiplier = heightMult;
        minLength = minLen;
        backdropHeight = backHeight;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            doChanging = false;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            
            doChanging = true;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            doChanging = true;
        }
    }
}
