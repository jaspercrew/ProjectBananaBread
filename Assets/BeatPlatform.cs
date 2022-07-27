using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType
{
    Collider, Wallslide, 
}
public class BeatPlatform : ActivatedEntity
{
    public bool isStatic = false;
    public PlatformType type;
    private Collider2D collider2D;
    private SpriteRenderer spriteRenderer;

    private const float deactivatedAlpha = .3f;
    // Start is called before the first frame update
    protected override void Start()
    {
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        base.Start();
    }

    protected override void BeatAction()
    {
        if (isStatic)
        {
            return;
        }
        base.BeatAction();
    }

    protected override void Activate()
    {
        base.Activate();
        switch (type)
        {
            case PlatformType.Collider:
                collider2D.enabled = true;
                Color temp = spriteRenderer.color;
                temp.a = 1;
                spriteRenderer.color = temp;
                break;

            case PlatformType.Wallslide:
                gameObject.layer = LayerMask.NameToLayer("Slide");
                spriteRenderer.color = new Color(1, .5f, 0);
                break;
        }
    }

    protected override void Deactivate()
    {
        base.Deactivate();
        switch (type)
        {
            
            case PlatformType.Collider:
                base.Deactivate();
                collider2D.enabled = false;
                Color temp = spriteRenderer.color;
                temp.a = deactivatedAlpha;
                spriteRenderer.color = temp;
                break;
            
            case PlatformType.Wallslide:
                gameObject.layer = LayerMask.NameToLayer("Obstacle");
                spriteRenderer.color = Color.white;
                break;
        }

    }
    
}
