using UnityEngine;

public class FadeSprite : MonoBehaviour
{
    public float lifetime = .5f;
    private SpriteRenderer spriteRenderer;
    private float timeLeft;

    // Start is called before the first frame update
    private void Awake()
    {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
        //fadeSpriteIterator = 0;
    }

    private void Start()
    {
        timeLeft = lifetime;
    }

    private void FixedUpdate()
    {
        if (timeLeft > 0)
        {
            var c = spriteRenderer.color;
            c.a -= 1 * Time.fixedDeltaTime / lifetime;
            spriteRenderer.color = c;
            timeLeft -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Sprite sprite, bool flippedX, bool flippedY, bool isExtended = false)
    {
        if (isExtended) lifetime *= 2.5f;
        spriteRenderer.sprite = sprite;
        if (flippedX) spriteRenderer.flipX = true;

        if (flippedY) spriteRenderer.flipY = true;
    }
}