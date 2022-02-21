using System.Collections;
using System.Collections.Generic;
using BitStrap;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

public class PPManager : MonoBehaviour
{
    private Volume volume;
    public static PPManager Instance;
    private bool shiftEffecting;
    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;
    private ShadowsMidtonesHighlights smh;

    private float shiftEffectDuration = .3f;
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

        
    }

    // Start is called before the first frame update
    void Start()
    {
        volume = GetComponent<Volume>();
        volume.profile.TryGet<Bloom>(out bloom);
        volume.profile.TryGet<ChromaticAberration>(out chromaticAberration);
        volume.profile.TryGet<FilmGrain>(out filmGrain);
        volume.profile.TryGet<ShadowsMidtonesHighlights>(out smh);
        
        //SpecificShiftEffect(true);
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShiftEffect(bool toAlt)
    {
        StartCoroutine(SpecificShiftEffectCoroutine(toAlt));
        StartCoroutine(GeneralShiftEffectCoroutine());
    }
    
    public IEnumerator SpecificShiftEffectCoroutine(bool toAlt)
    {
        float shiftEffectDuration = .33f;
        
        Debug.Log(smh.highlights.value);
        Debug.Log(smh.shadows.value);
        float timeElapsed = 0f;
        
        Vector4 highlightValue = new Vector4(1f, .3f, .8f, 0); 
        Vector4 shadowValue = new Vector4(.4f, .4f, 1f, 0);
        while (timeElapsed < shiftEffectDuration)
        {
            timeElapsed += Time.deltaTime;
            if (toAlt)
            {
                smh.highlights.Interp(Vector4.one / 2, highlightValue, timeElapsed / shiftEffectDuration);
                smh.shadows.Interp(Vector4.one / 2, shadowValue, timeElapsed / shiftEffectDuration);
                // Debug.Log();
            }
            else
            {
                smh.highlights.Interp(highlightValue, Vector4.one / 2, timeElapsed / shiftEffectDuration);
                smh.shadows.Interp(shadowValue, Vector4.one / 2, timeElapsed / shiftEffectDuration);
            }

            yield return null;
        }
    }

    private IEnumerator GeneralShiftEffectCoroutine()
    {
        const float increaseDuration = .13f;
        const float decreaseDuration = .2f;
        const float bloomIntensity = .1f;
        const float chromaticAberrationIntensity = .7f;
        const float filmGrainIntensity = .4f;


        float increaseTimeElapsed = 0f;
        while (increaseTimeElapsed < increaseDuration)  {
            bloom.intensity.value = Mathf.Lerp(0f, bloomIntensity, increaseTimeElapsed / increaseDuration);
            filmGrain.intensity.value = Mathf.Lerp(0f, filmGrainIntensity, increaseTimeElapsed / increaseDuration);
            chromaticAberration.intensity.value = Mathf.Lerp(0f, chromaticAberrationIntensity, increaseTimeElapsed / increaseDuration);
            increaseTimeElapsed += Time.deltaTime;
            yield return null;
        }
        yield return new WaitForSeconds(increaseDuration);

        float decreaseTimeElapsed = 0f;
        while (decreaseTimeElapsed < increaseDuration)  {
            bloom.intensity.value = Mathf.Lerp(bloomIntensity, 0f, decreaseTimeElapsed / decreaseDuration);
            filmGrain.intensity.value = Mathf.Lerp(filmGrainIntensity, 0f, decreaseTimeElapsed / decreaseDuration);
            chromaticAberration.intensity.value = Mathf.Lerp(chromaticAberrationIntensity, 0f, decreaseTimeElapsed / decreaseDuration);
            decreaseTimeElapsed += Time.deltaTime;
            yield return null;
        }
        bloom.intensity.value = 0f;
        chromaticAberration.intensity.value = 0f;
        filmGrain.intensity.value = 0f;

    }
}
