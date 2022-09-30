using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TransitionSlice : MonoBehaviour
{
    private float fadeTime;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.color = Color.clear;
        
        fadeTime = CameraManager.sliceFadeTime;
    }
    

    public void Appear()
    {
        StartCoroutine(AppearCoroutine());
    }
    
    private IEnumerator AppearCoroutine()
    {
        Color original = spriteRenderer.color;
        
        Color full = original;
        full.a = 1f;
        
        //Color full = Color.white;
        
        
        float elapsedTime = 0f;
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
        Color original = spriteRenderer.color;
        Color faded = original;
        faded.a = 0f;
        
        float elapsedTime = 0f;
        while (elapsedTime < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(original, faded, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = faded;
    }
}
