using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;

public class AudioManager : MonoBehaviour
{
    
    public static AudioManager Instance;

    private const float songVolume = 5f;

    private AudioSource effectSource;
    private AudioSource primarySource;
    private AudioSource altSource;
    private Dictionary<SoundName, AudioClip> soundToClip = new Dictionary<SoundName, AudioClip>();

    private SoundName savedA;
    private SoundName savedB;
    

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
        primarySource = sources[1];
        altSource = sources[2];
    }

    private void Start()
    {
        if (GameManager.Instance.isGameShifted)
        {
            primarySource.volume = 0f;
            altSource.volume = 1f;
        }
        else
        {
            primarySource.volume = 1f;
            altSource.volume = 0f;
        }
        //PlaySong(SoundName.Song1A, SoundName.Song1B);
    }

    public void Play(SoundName sound, float volume)
    {
        AudioClip clip = soundToClip[sound];
        effectSource.PlayOneShot(clip, volume);
    }

//    public void PlaySong(SoundName song)
//    {
//        AudioClip clip = soundToClip[song];
//        effectSource.PlayOneShot(clip, songVolume);
//    }
    
    public void PlaySong(SoundName primary, SoundName alt)
    {
        if (savedA == primary && savedB == alt) //if the song is already playing, do nothing
        {
            return;
        }
        AudioClip primaryClip = soundToClip[primary];
        AudioClip altClip = soundToClip[alt];
        primarySource.clip = primaryClip;
        altSource.clip = altClip;
        primarySource.Play();
        altSource.Play();
        savedA = primary;
        savedB = alt;
    }



    public void OnShift(bool toAlt)
    {
        const float fadeTime = .5f;
        if (toAlt)
        {
            StartCoroutine(FadeOut(primarySource, fadeTime));   
            StartCoroutine(FadeIn(altSource, fadeTime));
        }
        else
        {
            StartCoroutine(FadeOut(altSource, fadeTime)); 
            StartCoroutine(FadeIn(primarySource, fadeTime));
        }
        
    }
    
    private IEnumerator FadeOut(AudioSource audioSource, float FadeTime)
    {
        audioSource.volume = 1f;
 
        while (audioSource.volume > 0)
        {
            audioSource.volume -= 1f * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        //audioSource.Stop();
        audioSource.volume = 0f;
    }
 
    private IEnumerator FadeIn(AudioSource audioSource, float FadeTime)
    {
        audioSource.volume = 0;

        while (audioSource.volume < 1.0f)
        {
            audioSource.volume += 1f * Time.deltaTime / FadeTime;
 
            yield return null;
        }
 
        audioSource.volume = 1f;
    }

}