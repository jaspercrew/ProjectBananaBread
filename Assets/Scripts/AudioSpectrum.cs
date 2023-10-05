using UnityEngine;
using UnityEngine.Audio;

/// <summary>
///     Mini "engine" for analyzing spectrum data
///     Feel free to get fancy in here for more accurate visualizations!
/// </summary>
public class AudioSpectrum : MonoBehaviour
{
    public static AudioSpectrum instance;

    [HideInInspector] public float[] bufferSpectrum;

    public int spectrumFactor = 100;
    public int compressedSpectrumSize = 64;
    private AudioMixer audioMixer;

    private float[] bufferDecrease;

    [HideInInspector] private float[] compressedSpectrum;

    private int indicesPerBar;

    // public int spectrumIndex = 0;
    // Unity fills this up for us
    private float[] largeSpectrum;
    private readonly int largeSpectrumSize = 512;

    private void Awake()
    {
        instance = this;
        largeSpectrum = new float[largeSpectrumSize];
        compressedSpectrum = new float[compressedSpectrumSize];
        bufferSpectrum = new float[compressedSpectrumSize];
        bufferDecrease = new float[compressedSpectrumSize];

        indicesPerBar = largeSpectrumSize / compressedSpectrumSize;
    }

    private void Start()
    {
        audioMixer = (AudioMixer) Resources.Load("MainMixer");
    }

    private void Update()
    {
        // get the data
        AudioListener.GetSpectrumData(largeSpectrum, 0, FFTWindow.Blackman);
        for (var i = 0; i < compressedSpectrumSize; i++)
        {
            float sum = 0;
            for (var j = i * indicesPerBar; j < (i + 1) * indicesPerBar; j++) sum += largeSpectrum[j] * spectrumFactor;
            var average = sum / indicesPerBar;
            var vol = AudioManager.instance.normalizedVolume;
            compressedSpectrum[i] = average / (vol + .001f); // + small number to prevent / 0
        }

        BandBuffer();
        //print(string.Join(" ", bufferSpectrum));
        //print(SceneManager.GetActiveScene().name);
    }

    private void BandBuffer()
    {
        for (var i = 0; i < compressedSpectrumSize; ++i)
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