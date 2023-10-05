using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    public float normalizedVolume;

    //private readonly Dictionary<SoundName, AudioClip> soundToClipf = new Dictionary<SoundName, AudioClip>();
    public AudioSource[] mainSources;

    public readonly Dictionary<SoundName, AudioClip> soundToClip =
        new Dictionary<SoundName, AudioClip>();

    private AudioMixer audioMixer;

    private AudioSource effectSource;

    private AudioSource isolatedSource;

    private SoundName savedSong;

    // private const float SongVolume = 5f;
    private AudioSource[] songsA;
    private AudioSource[] songsB;
    private AudioSource songSourceA;
    private AudioSource songSourceAf;
    private AudioSource songSourceB;
    private AudioSource songSourceBf;
    private AudioSource songSourceC;
    private AudioSource songSourceCf;
    private AudioSource songSourceD;
    private AudioSource songSourceDf;

    // public AudioMixerGroup mixerGroup;

    public void Awake()
    {
        if (instance != null)
            Destroy(gameObject);
        else
            instance = this;

        foreach (SoundName sound in Enum.GetValues(typeof(SoundName)))
        {
            // TODO: set a naming scheme for files
            var clip = Resources.Load<AudioClip>("Sounds/" + sound.ToString().ToLower());
            soundToClip[sound] = clip;
        }

        // foreach (SoundName sound in Enum.GetValues(typeof(SoundName)))
        // {
        //     // TODO: set a naming scheme for files
        //     AudioClip clip = Resources.Load<AudioClip>("Sounds/" + sound.ToString().ToLower());
        //     soundToClipf[sound] = clip;
        // }

        songsA = transform.Find("SongSourcesA").GetComponents<AudioSource>();
        songsB = transform.Find("SongSourcesB").GetComponents<AudioSource>();

        effectSource = transform.Find("EffectSource").GetComponent<AudioSource>();
        songSourceA = songsA[0];
        songSourceB = songsA[1];
        songSourceC = songsA[2];
        songSourceD = songsA[3];
        isolatedSource = transform.Find("NonFadeSource").GetComponent<AudioSource>();
        songSourceAf = songsB[0];
        songSourceBf = songsB[1];
        songSourceCf = songsB[2];
        songSourceDf = songsB[3];

        mainSources = new AudioSource[9];
        mainSources[0] = songSourceA;
        mainSources[1] = songSourceB;
        mainSources[2] = songSourceC;
        mainSources[3] = songSourceD;

        mainSources[4] = effectSource;

        mainSources[5] = songSourceAf;
        mainSources[6] = songSourceBf;
        mainSources[7] = songSourceCf;
        mainSources[8] = songSourceDf;

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
        foreach (var source in mainSources) source.Pause();
    }

    public void UnpauseAudio()
    {
        foreach (var source in mainSources) source.UnPause();
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
        var clip = soundToClip[sound];
        effectSource.PlayOneShot(clip, volume);
    }

    public void IsolatedPlay(SoundName sound, float volume)
    {
        var clip = soundToClip[sound];
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
            sourceToUse = songSourceA;
        else if (source == 1)
            sourceToUse = songSourceB;
        else
            sourceToUse = songSourceC;
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
            sourceToUse = flipped ? songSourceAf : songSourceA;
        //print(flipped ? "Af" : "A");
        else if (source == 1)
            sourceToUse = flipped ? songSourceBf : songSourceB;
        //print(flipped ? "Bf" : "B");
        else if (source == 2)
            sourceToUse = flipped ? songSourceCf : songSourceC;
        //print(flipped ? "Cf" : "C");
        else
            sourceToUse = flipped ? songSourceDf : songSourceD;
        //print(flipped ? "Cf" : "C");
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
    //     const float lifetime = .25f;
    //     if (isAlt)
    //     {
    //         StartCoroutine(FadeOut(songSourceA, lifetime));
    //         StartCoroutine(FadeIn(songSourceB, lifetime));
    //     }
    //     else
    //     {
    //         StartCoroutine(FadeOut(songSourceB, lifetime));
    //         StartCoroutine(FadeIn(songSourceA, lifetime));
    //     }
    //
    // }

    public void AllFadeOut()
    {
        foreach (var source in mainSources) StartCoroutine(FadeOut(source, 1.5f));
    }

    public void UpdateCurrentSongs(bool[] songsToPlay, bool fade = true)
    {
        var currentSongStates = new bool[songsToPlay.Length];
        for (var i = 0; i < songsToPlay.Length; i++) currentSongStates[i] = songsA[i].volume > double.Epsilon;

        for (var i = 0; i < songsToPlay.Length; i++)
            if (songsToPlay[i] != currentSongStates[i])
            {
                if (fade)
                    //print("start f");
                    StartCoroutine(SongFade(i, !songsToPlay[i]));
                else
                    songsA[i].volume = songsB[i].volume = songsToPlay[i] ? .3f : 0f;
            }
    }

    private IEnumerator SongFade(int source, bool fadeOut)
    {
        //print("SongFade()" + source + "  " + fadeOut);
        AudioSource[] song = {songsA[source], songsB[source]};
        var fadeTime = .25f;
        var startingVol = songsA[source].volume;
        var fullVol = .3f;
        var destinationVolume = fadeOut ? 0f : fullVol;

        var currentVolume = startingVol;

        if (fadeOut)
            while (currentVolume > destinationVolume)
            {
                currentVolume -= startingVol * Time.deltaTime / fadeTime;
                song[0].volume = song[1].volume = currentVolume;
                yield return null;
            }
        else
            while (currentVolume < destinationVolume)
            {
                currentVolume += fullVol * Time.deltaTime / fadeTime;
                song[0].volume = song[1].volume = currentVolume;
                yield return null;
            }

        song[0].volume = song[1].volume = destinationVolume;
    }

    private IEnumerator FadeOut(AudioSource audioSource, float fadeTime)
    {
        var startingVol = audioSource.volume;

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