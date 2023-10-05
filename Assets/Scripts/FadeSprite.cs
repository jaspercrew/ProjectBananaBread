using UnityEngine;

public class FadeSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float lifetime = .5f;
    private float timeLeft;

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
        //fadeSpriteIterator = 0;
    }

    void Start()
    {
        timeLeft = lifetime;
    }

    void FixedUpdate()
    {
        if (timeLeft > 0)
        {
            Color c = spriteRenderer.color;
            c.a -= 1 * Time.fixedDeltaTime / lifetime;
            spriteRenderer.color = c;
            timeLeft -= Time.deltaTime;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void Initialize(Sprite sprite, bool flippedX, bool FlippedY, bool isExtended = false)
    {
        if (isExtended)
        {
            lifetime *= 2.5f;
        }
        spriteRenderer.sprite = sprite;
        if (flippedX)
        {
            spriteRenderer.flipX = true;
        }

        if (FlippedY)
        {
            spriteRenderer.flipY = true;
        }
    }
}
