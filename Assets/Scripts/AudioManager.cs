using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

    private AudioSource audioSource;
    private Dictionary<SoundName, AudioClip> soundToClip = new Dictionary<SoundName, AudioClip>();
    

    // public AudioMixerGroup mixerGroup;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
            // DontDestroyOnLoad(gameObject);
        }

        foreach (SoundName sound in Enum.GetValues(typeof(SoundName)))
        {
            // TODO: set a naming scheme for files
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sound.ToString().ToLower());
            soundToClip[sound] = clip;
        }
        
        
    }

    private void Start()
    {
        audioSource = GetComponent<AudioSource>();
    }

    public void Play(SoundName sound, float volume)
    {
        AudioClip clip = soundToClip[sound];
        audioSource.PlayOneShot(clip, volume);
    }

}