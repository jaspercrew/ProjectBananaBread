using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class PPManager : MonoBehaviour
{
    private Volume volume;
    public static PPManager Instance;
    private bool shiftEffecting;
    private Bloom bloom;
    private ChromaticAberration chromaticAberration;
    private FilmGrain filmGrain;
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
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShiftEffect()
    {
        StartCoroutine(ShiftEffectCoroutine());
    }

    private IEnumerator ShiftEffectCoroutine()
    {
        const float increaseDuration = .13f;
        const float decreaseDuration = .2f;
        const float effectIntensity = .5f;
        const float chromaticAberrationIntensity = .7f;
        const float filmGrainIntensity = .4f;

        chromaticAberration.intensity.value = chromaticAberrationIntensity;
        filmGrain.intensity.value = filmGrainIntensity;
        
        float increaseTimeElapsed = 0f;
        while (increaseTimeElapsed < increaseDuration)  {
            bloom.intensity.value = Mathf.Lerp(0f, effectIntensity, increaseTimeElapsed / increaseDuration);
            increaseTimeElapsed += Time.deltaTime;
            yield return null;
        }
        bloom.intensity.value = effectIntensity;
        yield return new WaitForSeconds(increaseDuration);

        float decreaseTimeElapsed = 0f;
        while (decreaseTimeElapsed < increaseDuration)  {
            bloom.intensity.value = Mathf.Lerp(effectIntensity, 0f, decreaseTimeElapsed / decreaseDuration);
            decreaseTimeElapsed += Time.deltaTime;
            yield return null;
        }
        bloom.intensity.value = 0f;
        chromaticAberration.intensity.value = 0f;
        filmGrain.intensity.value = 0f;

    }
}
