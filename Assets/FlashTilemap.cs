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
        Color temp = Color.cyan;
        temp.a = 0;
        tilemap.color = temp;
        tilemapCollider2D.enabled = false;
        SimpleVolumeManager.instance.volume.enabled = false;
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

    private IEnumerator ColorFade(bool fadeIn, bool hazard)
    {
        float fadeTime = .05f;
        float timeElapsed = 0;
        Color empty = tilemap.color;
        empty.a = 0f;
        Color full = tilemap.color;
        full.a = 1f;
        Color faded = tilemap.color;
        faded.a = .2f;

        Color start = fadeIn ? empty : tilemap.color;
        Color destination;

        if (fadeIn)
        {
            destination = hazard ? full : faded;
        }
        else
        {
            destination = empty;
        }

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
        SimpleVolumeManager.instance.volume.enabled = true;
        tilemapCollider2D.enabled = true;
        StartCoroutine(ColorFade(true, true));
        float flashDuration = .6f;
        yield return new WaitForSeconds(flashDuration);
        StartCoroutine(ColorFade(false, true));
        tilemapCollider2D.enabled = false;
        SimpleVolumeManager.instance.volume.enabled = false;
    }

    private IEnumerator FlashCoroutine()
    {
        StartCoroutine(ColorFade(true, false));
        float flashDuration = .15f;
        yield return new WaitForSeconds(flashDuration);
        StartCoroutine(ColorFade(false, false));
    }
}
