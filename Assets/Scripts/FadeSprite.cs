using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FadeSprite : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;
    private const float fadeTime = 15f;
    private float timeLeft;
    

    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponentInParent<SpriteRenderer>();
        timeLeft = fadeTime;
        //fadeSpriteIterator = 0;
    }
    
    void Update()
    {
        if (timeLeft > 0)
        {
            Color c = spriteRenderer.color;
            c.a -= 1 / fadeTime;
            spriteRenderer.color = c;
            timeLeft -= Time.deltaTime;
        }
        else {
            Destroy(gameObject);
        }
    }

    public void Initialize(Sprite sprite, bool flipped)
    {
        spriteRenderer.sprite = sprite;
        if (flipped)
        {
            spriteRenderer.flipX = true;
        }
    }
}
