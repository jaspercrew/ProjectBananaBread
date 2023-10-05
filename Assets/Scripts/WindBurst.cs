using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class WindBurst : BeatEntity
{
    public Vector2 direction;
    public float windVelocity;
    private bool playerInRange;

    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void MicroBeatAction()
    {
        if (playerInRange)
        {
            //Vector2 vel = CharController.Instance.GetComponent<Rigidbody2D>().velocity;
            CharController.Instance.GetComponent<Rigidbody2D>().velocity =
                windVelocity * direction.normalized;
        }

        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        Color initialColor = spriteRenderer.color;

        Color fullAlpha = initialColor;
        fullAlpha.a = 1;
        spriteRenderer.color = fullAlpha;

        float elapsedTime = 0f;
        float flashTime = 1.5f;
        while (elapsedTime < flashTime)
        {
            spriteRenderer.color = Color.Lerp(fullAlpha, initialColor, elapsedTime / flashTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = initialColor;
        yield return null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerInRange = false;
        }
    }
}
