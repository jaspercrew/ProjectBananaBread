using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
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
    private AudioSource songSourceAf;
    private AudioSource songSourceBf;
    private AudioSource songSourceCf;
    public readonly Dictionary<SoundName, AudioClip> soundToClip = new Dictionary<SoundName, AudioClip>();

    public float normalizedVolume;

    private AudioMixer audioMixer;
    //private readonly Dictionary<SoundName, AudioClip> soundToClipf = new Dictionary<SoundName, AudioClip>();
    public AudioSource[] mainSources;

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
        
        // foreach (SoundName sound in Enum.GetValues(typeof(SoundName)))
        // {
        //     // TODO: set a naming scheme for files
        //     AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sound.ToString().ToLower());
        //     soundToClipf[sound] = clip;
        // }
        
        AudioSource[] allSources = GetComponents<AudioSource>();
        effectSource = allSources[0];
        songSourceA = allSources[1];
        songSourceB = allSources[2];
        songSourceC = allSources[3];
        isolatedSource = allSources[4];
        songSourceAf = allSources[5];
        songSourceBf = allSources[6];
        songSourceCf = allSources[7];
        
        mainSources = new AudioSource[7];
        mainSources[0] = songSourceA;
        mainSources[1] = songSourceB;
        mainSources[2] = songSourceC;
        mainSources[3] = effectSource;
        mainSources[4] = songSourceAf;
        mainSources[5] = songSourceBf;
        mainSources[6] = songSourceCf;

        audioMixer = (AudioMixer) Resources.Load("MainMixer");

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
        foreach (AudioSource source in mainSources)
        {
            source.Pause();
        }
    }
    
    public void UnpauseAudio()
    {
        foreach (AudioSource source in mainSources)
        {
            source.UnPause();
        }
    }
    
    public void UpdateVolume(float sliderValue)
    {
        // const float volumeMultiplier = 1f;
        // foreach (AudioSource source in mainSources)
        // {
        //     
        //     //print("volume updated for a source");
        //     source.volume = sliderValue * volumeMultiplier;
        // }
        normalizedVolume = sliderValue;

        audioMixer.SetFloat("MainVolume", Mathf.Log10(sliderValue + .001f) * 20);

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
        //print("playsong()");
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
            //sourceToUse.PlayOneShot();
        }

    }
    
    public void PlaySongScheduled(SoundName song, int source, double time, bool flipped)
    {
        //print("scheduled play");
        AudioSource sourceToUse;
        if (source == 0)
        {
            sourceToUse = flipped ? songSourceAf : songSourceA;
            //print(flipped ? "Af" : "A");
        }
        else if (source == 1)
        {
            sourceToUse = flipped ? songSourceBf : songSourceB;
            //print(flipped ? "Bf" : "B");
        }
        else
        {
            sourceToUse = flipped ? songSourceCf : songSourceC;
            //print(flipped ? "Cf" : "C");
        }
        if (song == SoundName.None) 
        {
            sourceToUse.Stop();
        }
        else
        {
            sourceToUse.clip = soundToClip[song];
            sourceToUse.PlayScheduled(time);
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
        foreach (AudioSource source in mainSources)
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