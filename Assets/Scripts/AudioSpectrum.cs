using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

/// <summary>
/// Mini "engine" for analyzing spectrum data
/// Feel free to get fancy in here for more accurate visualizations!
/// </summary>
public class AudioSpectrum : MonoBehaviour
{
    // public int spectrumIndex = 0;
    // Unity fills this up for us
    private float[] largeSpectrum;
    
    [HideInInspector]
    private float[] compressedSpectrum;
    [HideInInspector]
    public float[] bufferSpectrum;

    private float[] bufferDecrease;

    public int spectrumFactor = 100;
    public int compressedSpectrumSize = 64;
    private int largeSpectrumSize = 512;

    private int indicesPerBar;
    

    public static AudioSpectrum Instance;
    
    private void Update()
    {
        // print(bufferSpectrum[4]);
        // get the data
        AudioListener.GetSpectrumData(largeSpectrum, 0, FFTWindow.Blackman);
        for (int i = 0; i < compressedSpectrumSize; i++)
        {
            float sum = 0;
            for (int j = i * indicesPerBar; j < (i + 1) * (indicesPerBar); j++)
            {
                sum += largeSpectrum[j] * spectrumFactor;
            }
            float average = sum / indicesPerBar;
            compressedSpectrum[i] = average;
        }
        BandBuffer();
    }

    private void Start()
    {
        if (Instance is null)
        {
            Instance = this;
        }
        largeSpectrum = new float[largeSpectrumSize];
        compressedSpectrum = new float[compressedSpectrumSize];
        bufferSpectrum = new float[compressedSpectrumSize];
        bufferDecrease = new float[compressedSpectrumSize];
        
        indicesPerBar = largeSpectrumSize / compressedSpectrumSize;
    }

    private void BandBuffer()
    {
        for (int i = 0; i < compressedSpectrumSize; ++i)
        {
            if (compressedSpectrum[i] > bufferSpectrum[i])
            {
                bufferSpectrum[i] = compressedSpectrum[i];
                //bufferDecrease[i] = .005f;
                bufferDecrease[i] = .008f;
            }
            if (compressedSpectrum[i] < bufferSpectrum[i])
            {
                bufferSpectrum[i] -= bufferDecrease[i];
                bufferDecrease[i] *= 1.2f;
            }
        }
    }



}