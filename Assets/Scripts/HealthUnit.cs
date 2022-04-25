using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HealthUnit : MonoBehaviour
{
    public Sprite filledSprite;
    public Sprite emptySprite;
    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Fill()
    {
        spriteRenderer.sprite = filledSprite;
    }

    public void Empty()
    {
        spriteRenderer.sprite = emptySprite;
    }

}
