using UnityEngine;

public class FadeSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    public float fadeTime = .3f;
    private float timeLeft;
    

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
        timeLeft = fadeTime;
        //fadeSpriteIterator = 0;
    }
    
    void FixedUpdate()
    {
        if (timeLeft > 0)
        {
            Color c = spriteRenderer.color;
            c.a -= 1 * Time.fixedDeltaTime / fadeTime;
            spriteRenderer.color = c;
            timeLeft -= Time.deltaTime;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void Initialize(Sprite sprite, bool flippedX, bool FlippedY)
    {
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
