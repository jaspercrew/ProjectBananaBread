using System;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance;

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

        // foreach (Sound s in sounds)
        // {
        //     s.source = gameObject.AddComponent<AudioSource>();
        //     s.source.clip = s.clip;
        //     // s.source.loop = s.loop;
        //
        //     // s.source.outputAudioMixerGroup = mixerGroup;
        // }
    }

    public void Play(SoundList sound, float volume, float pitch)
    {
        // we should probably use a hash map here but there's only three sounds so it's fine
        Sound s = Array.Find(sounds, item => item.name == sound);
        // if (s == null)
        // {
        //     Debug.LogWarning("Sound: " + name + " not found!");
        //     return;
        // }

        s.source.
        s.source.volume = volume;
        s.source.pitch = pitch;

        // these are 0 anyway
        // s.source.volume = 
        //     s.volume * (1f + UnityEngine.Random.Range(-s.volumeVariance / 2f, s.volumeVariance / 2f));
        // s.source.pitch = 
        //     s.pitch * (1f + UnityEngine.Random.Range(-s.pitchVariance / 2f, s.pitchVariance / 2f));

        s.source.Play();
    }

}