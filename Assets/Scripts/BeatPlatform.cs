using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType
{
    Collider, Wallslide, Moving
}
public class BeatPlatform : ActivatedEntity
{
    public bool isStatic = false;
    public bool isHazard = false;
    public PlatformType type;
    private Collider2D platformCollider;
    private SpriteRenderer spriteRenderer;
    private const float deathCheckFactor = .75f;
    public Vector2 moveVector;
    private Vector2 originalPosition;

    //[HideInInspector]
    public bool isWallSlideable;

    private const float deactivatedAlpha = .3f;
    // Start is called before the first frame update
    protected override void Start()
    {
        platformCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (isHazard)
        {
            spriteRenderer.color = Color.red;
        }
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        if (isStatic)
        {
            return;
        }
        base.MicroBeatAction();
    }

    protected override void Activate()
    {
        base.Activate();
        switch (type)
        {
            case PlatformType.Collider:
                platformCollider.enabled = true;
                Color temp = spriteRenderer.color;
                temp.a = 1;
                spriteRenderer.color = temp;

                BoxCollider2D charCollider = CharController.Instance.GetComponent<BoxCollider2D>();
                Vector3 bounds = charCollider.bounds.extents;
                float halfWidth = Mathf.Abs(bounds.x);
                float halfHeight = Mathf.Abs(bounds.y);
                Vector2 center = (Vector2) CharController.Instance.transform.position + charCollider.offset.y * Vector2.up;

                Vector2 bottomMiddle = center + halfHeight * Vector2.down;
                Vector2 bottomLeft = bottomMiddle + halfWidth * deathCheckFactor * Vector2.left;
                Vector2 bottomRight = bottomMiddle + halfWidth * deathCheckFactor * Vector2.right;
                
                Vector2 topMiddle = center + halfHeight * Vector2.up;
                Vector2 topLeft = topMiddle + halfWidth * deathCheckFactor * Vector2.left;
                Vector2 topRight = topMiddle + halfWidth * deathCheckFactor * Vector2.right;
                Bounds platformBounds = platformCollider.bounds;
                if (platformBounds.Contains(bottomLeft) || platformBounds.Contains(bottomRight) || 
                    platformBounds.Contains(topLeft) || platformBounds.Contains(topRight) || platformBounds.Contains(center))
                {
                    CharController.Instance.Die();
                }
                break;

            case PlatformType.Wallslide:
                //gameObject.layer = LayerMask.NameToLayer("Slide");
                isWallSlideable = true;
                spriteRenderer.color = new Color(1, .5f, 0);
                break;
            
            case PlatformType.Moving:
                StopCoroutine("MoveToCoroutine");
                StartCoroutine(MoveToCoroutine(false));
                break;
        }
    }

    private IEnumerator MoveToCoroutine(bool isMovingBack)
    {
        Vector2 destination = isMovingBack ? (Vector2)transform.position - moveVector : (Vector2)transform.position + moveVector;
        float elapsedTime = 0f;
        float moveTime = GameManager.Instance.songBpm / 60;

        while (elapsedTime < moveTime)
        {
            transform.position = Vector3.Lerp(transform.position, destination, (elapsedTime / moveTime));
            elapsedTime += Time.deltaTime;
 
            // Yield here
            yield return null;
        }  
        // Make sure we got there
        transform.position = destination;
        yield return null;
    }

    protected override void Deactivate()
    {
        base.Deactivate();
        switch (type)
        {
            
            case PlatformType.Collider:
                base.Deactivate();
                platformCollider.enabled = false;
                Color temp = spriteRenderer.color;
                temp.a = deactivatedAlpha;
                spriteRenderer.color = temp;
                break;
            
            case PlatformType.Wallslide:
                //gameObject.layer = LayerMask.NameToLayer("Obstacle");
                isWallSlideable = false;
                spriteRenderer.color = Color.white;
                break;
            case PlatformType.Moving:
                StopCoroutine("MoveToCoroutine");
                StartCoroutine(MoveToCoroutine(true));
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isHazard && other.gameObject.CompareTag("Player"))
        {
            CharController.Instance.Die();
        }
    }
    
}
