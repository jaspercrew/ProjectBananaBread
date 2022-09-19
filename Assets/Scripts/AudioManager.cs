using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager Instance;

    // private const float SongVolume = 5f;

    private AudioSource effectSource;
    private AudioSource songSourceA;
    private AudioSource songSourceB;
    private AudioSource songSourceC;
    private AudioSource isolatedSource;
    private readonly Dictionary<SoundName, AudioClip> soundToClip = new Dictionary<SoundName, AudioClip>();
    public AudioSource[] audioSources;

    private SoundName savedSong;


    // public AudioMixerGroup mixerGroup;

    public void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        foreach (SoundName sound in Enum.GetValues(typeof(SoundName)))
        {
            // TODO: set a naming scheme for files
            AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sound.ToString().ToLower());
            soundToClip[sound] = clip;
        }
        
        AudioSource[] sources = GetComponents<AudioSource>();
        effectSource = sources[0];
        songSourceA = sources[1];
        songSourceB = sources[2];
        songSourceC = sources[3];
        isolatedSource = sources[4];
        
        audioSources = new AudioSource[4];
        audioSources[0] = songSourceA;
        audioSources[1] = songSourceB;
        audioSources[2] = songSourceC;
        audioSources[3] = effectSource;

    }
    

    public void Start()
    {
       // print(songSourceA.volume);
        // if (songSourceA.volume == 0)
        // {
        //     print("check for 0 volume");
        //     UpdateVolume(.5f);
        // }

    }
    
    

    public void PauseAudio()
    {
        foreach (AudioSource source in audioSources)
        {
            source.Pause();
        }
    }
    
    public void UnpauseAudio()
    {
        foreach (AudioSource source in audioSources)
        {
            source.UnPause();
        }
    }
    
    public void UpdateVolume(float sliderValue)
    {
        const float volumeMultiplier = 1f;
        foreach (AudioSource source in audioSources)
        {
            source.volume = sliderValue * volumeMultiplier;
        }
    }

    public void Play(SoundName sound, float volume)
    {
        AudioClip clip = soundToClip[sound];
        effectSource.PlayOneShot(clip, volume);
    }
    
    public void IsolatedPlay(SoundName sound, float volume)
    {
        AudioClip clip = soundToClip[sound];
        isolatedSource.PlayOneShot(clip, volume);
    }

//    public void PlaySong(SoundName songA)
//    {
//        AudioClip clip = soundToClip[songA];
//        effectSource.PlayOneShot(clip, songVolume);
//    }

    public void PlaySong(SoundName song, int source = 0)
    {
        AudioSource sourceToUse;
        if (source == 0)
        {
            sourceToUse = songSourceA;
        }
        else if (source == 1)
        {
            sourceToUse = songSourceB;
        }
        else
        {
            sourceToUse = songSourceC;
        }
        if (song == SoundName.None) 
        {
            sourceToUse.Stop();
        }
        else
        {
            sourceToUse.clip = soundToClip[song];
            sourceToUse.Play();
        }

    }



    // TODO: remove and make this an ActivatedEntity?
    // private int beatCounter = 0;
    // private bool isAlt = false;
    // public void OnShift(/*bool toAlt*/)
    // {
    //     beatCounter++;
    //     if (beatCounter == 4)
    //     {
    //         beatCounter = 0;
    //     }
    //     else
    //     {
    //         return;
    //     }
    //
    //     isAlt = !isAlt;
    //     
    //     const float fadeTime = .25f;
    //     if (isAlt)
    //     {
    //         StartCoroutine(FadeOut(songSourceA, fadeTime));   
    //         StartCoroutine(FadeIn(songSourceB, fadeTime));
    //     }
    //     else
    //     {
    //         StartCoroutine(FadeOut(songSourceB, fadeTime)); 
    //         StartCoroutine(FadeIn(songSourceA, fadeTime));
    //     }
    //     
    // }

    public void AllFadeOut()
    {
        foreach (AudioSource source in audioSources)
        {
            StartCoroutine(FadeOut(source, 1.5f));
        }
    }
    
    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        float startingVol = audioSource.volume;
 
        while (audioSource.volume > 0)
        {
            audioSource.volume -= startingVol * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        audioSource.Stop();
        audioSource.volume = 0f;
    }

    private IEnumerator FadeIn(AudioSource audioSource, float fadeTime)
    {
        audioSource.volume = 0;

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += 1f * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        audioSource.volume = 1f;
    }

}