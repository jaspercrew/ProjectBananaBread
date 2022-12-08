using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class FlashTilemap : ActivatedEntity
{
    private int actionCount;
    private Tilemap tilemap;
    private TilemapCollider2D tilemapCollider2D;

    protected override void Start()
    {
        tilemapCollider2D = GetComponent<TilemapCollider2D>();
        tilemap = GetComponent<Tilemap>();
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        if (actionCount % 4 == 3)
        {
            StartCoroutine(HazardCoroutine());
        }
        else
        {
            StartCoroutine(FlashCoroutine());
        }
        

        actionCount++;
        base.MicroBeatAction();
    }

    private IEnumerator ColorFade(bool fadeIn)
    {
        float fadeTime = .1f;
        float timeElapsed = 0;
        Color start = tilemap.color;
        Color full = start;
        full.a = 1f;
        Color faded = start;
        faded.a = .5f;

        Color destination = fadeIn ? full : faded;
        while (timeElapsed < fadeTime)
        {
            tilemap.color = Color.Lerp(start, destination, timeElapsed / fadeTime);
            timeElapsed += Time.deltaTime;
            yield return null;
        }

        tilemap.color = destination;
    }
    
    private IEnumerator HazardCoroutine()
    {
        tilemapCollider2D.enabled = true;
        StartCoroutine(ColorFade(true));
        float flashDuration = .8f;
        yield return new WaitForSeconds(flashDuration);
        StartCoroutine(ColorFade(false));
        tilemapCollider2D.enabled = false;
    }

    private IEnumerator FlashCoroutine()
    {
        StartCoroutine(ColorFade(true));
        float flashDuration = .25f;
        yield return new WaitForSeconds(flashDuration);
        StartCoroutine(ColorFade(false));
    }


}
