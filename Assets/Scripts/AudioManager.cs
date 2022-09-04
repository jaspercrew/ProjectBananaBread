using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager Instance;

    // private const float SongVolume = 5f;

    private AudioSource effectSource;
    private AudioSource songSourceA;
    private AudioSource songSourceB;
    private AudioSource songSourceC;
    private readonly Dictionary<SoundName, AudioClip> soundToClip = new Dictionary<SoundName, AudioClip>();

    private SoundName savedSong;


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
    }

    private void Start()
    {
        float volume = .1f;
        songSourceA.volume = volume;
        songSourceB.volume = volume;
        songSourceC.volume = volume;
    }

    public void Play(SoundName sound, float volume)
    {
        AudioClip clip = soundToClip[sound];
        effectSource.PlayOneShot(clip, volume);
    }

//    public void PlaySong(SoundName songA)
//    {
//        AudioClip clip = soundToClip[songA];
//        effectSource.PlayOneShot(clip, songVolume);
//    }

    public void PlaySong(SoundName song, int source = 0)
    {
        if (savedSong == song || song == SoundName.None) //if the songA is already playing, do nothing
        {
            return;
        }

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
        sourceToUse.clip = soundToClip[song];
        sourceToUse.Play();
        savedSong = song;
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
    
    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        audioSource.volume = 1f;
 
        while (audioSource.volume > 0)
        {
            audioSource.volume -= 1f * Time.deltaTime / fadeTime;
 
            yield return null;
        }
 
        //audioSource.Stop();
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