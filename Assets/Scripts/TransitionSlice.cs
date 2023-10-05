using System.Collections;
using UnityEngine;

public class TransitionSlice : MonoBehaviour
{
    private float fadeTime;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.clear;

        fadeTime = CameraManager.SliceFadeTime;
    }

    public void Appear()
    {
        StartCoroutine(AppearCoroutine());
    }

    private IEnumerator AppearCoroutine()
    {
        var original = spriteRenderer.color;

        var full = original;
        full.a = 1f;

        //Color full = Color.white;


        var elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(original, full, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = full;
    }

    public void Disappear()
    {
        StartCoroutine(DisappearCoroutine());
    }

    private IEnumerator DisappearCoroutine()
    {
        var original = spriteRenderer.color;
        var faded = original;
        faded.a = 0f;

        var elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(original, faded, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = faded;
    }
}