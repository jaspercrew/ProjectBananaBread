using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
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
    private AudioMixer audioMixer;
    

    public static AudioSpectrum Instance;

    private void Start()
    {
        audioMixer = (AudioMixer)Resources.Load("MainMixer");

    }
    
    private void Update()
    {
        
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
            float vol = AudioManager.Instance.normalizedVolume;
            compressedSpectrum[i] = average / (vol + .001f); // + small number to prevent / 0 
        }
        BandBuffer();
        //print(string.Join(" ", bufferSpectrum));
        //print(SceneManager.GetActiveScene().name);
    }

    private void Awake()
    {
        Instance = this;
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
                //bufferDecrease[i] = .008f;
                bufferDecrease[i] = .002f;
            }
            if (compressedSpectrum[i] < bufferSpectrum[i])
            {
                bufferSpectrum[i] -= bufferDecrease[i];
                //bufferDecrease[i] *= 1.2f;
                bufferDecrease[i] *= 1.1f;
            }
        }
    }



}