/*using System.Collections;
using UnityEngine;

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

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) playerInRange = false;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Player")) playerInRange = true;
    }

    protected override void MicroBeatAction()
    {
        if (playerInRange)
            //Vector2 vel = CharController.Instance.GetComponent<Rigidbody2D>().velocity;
            CharController.instance.GetComponent<Rigidbody2D>().velocity =
                windVelocity * direction.normalized;

        StartCoroutine(FlashCoroutine());
    }

    private IEnumerator FlashCoroutine()
    {
        var initialColor = spriteRenderer.color;

        var fullAlpha = initialColor;
        fullAlpha.a = 1;
        spriteRenderer.color = fullAlpha;

        var elapsedTime = 0f;
        var flashTime = 1.5f;
        while (elapsedTime < flashTime)
        {
            spriteRenderer.color = Color.Lerp(fullAlpha, initialColor, elapsedTime / flashTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        spriteRenderer.color = initialColor;
        yield return null;
    }
}*/

