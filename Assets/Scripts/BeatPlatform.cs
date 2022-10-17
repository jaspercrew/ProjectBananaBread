using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum PlatformType
{
    Collider, Moving, Fading, Impulse
}
public class BeatPlatform : ActivatedEntity
{
    public bool isStatic = false;
    public bool isHazard = false;
    public PlatformType type;
    public float timeToMove = 2f;
    public Vector2 moveVector;
    public bool isWallSlideable;
    
    private Collider2D platformCollider;
    private SpriteRenderer spriteRenderer;
    private Vector2 originalPosition;
    private bool isPlayerTouching;
    private Vector2 playerRelativePosition;
    
    private const float deathCheckFactor = .75f;
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



        if (isWallSlideable)
        {
            spriteRenderer.color = new Color(1f, .5f, 0);
        }
        base.Start();
    }

    private void OnDrawGizmos()
    {
        if (type == PlatformType.Moving)
        {
            if (initialIsActive)
            {
                Gizmos.DrawWireSphere(transform.localPosition + (Vector3) moveVector, 1f);
            }
            else
            {
                Gizmos.DrawWireSphere(transform.localPosition - (Vector3) moveVector, 1f);
            }
                
        }
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
                StartCoroutine(BinaryPlatformCoroutine(true));


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

            case PlatformType.Moving:
                StopCoroutine("MoveToCoroutine");
                StartCoroutine(MoveToCoroutine(false));
                break;
            
            case PlatformType.Impulse:
                StartCoroutine(ImpulseCoroutine());
                break;
        }
    }

    private IEnumerator ImpulseCoroutine()
    {
        const float speedFactor = 8;
        const float enlargeAdder = .5f;
        
        //enlarge
        Vector2 originalScale = transform.localScale;
        Vector2 positiveMoveVector = new Vector2(Math.Abs(moveVector.normalized.x), Math.Abs(moveVector.normalized.y));
        Vector2 targetScale = originalScale + (positiveMoveVector * enlargeAdder);
        Vector2 originalPosition = transform.localPosition;
        Vector2 targetPosition = originalPosition + (moveVector.normalized * enlargeAdder / 2);

        float elapsedTime = 0f;
        float moveTime = (60 * timeToMove / GameManager.Instance.songBpm) / speedFactor;

        while (elapsedTime < moveTime)
        {
            if (isPlayerTouching /*&& playerRelativePosition == moveVector.normalized*/)
            {
                CharController.Instance.isJumpBoosted = true;
            }
            transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / moveTime));
            transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / moveTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = targetScale;
        transform.localPosition = targetPosition;
        yield return null;
        
        // if (isPlayerTouching && playerRelativePosition == moveVector.normalized)
        // {
        //     CharController.Instance.GetComponent<Rigidbody2D>().AddForce(moveVector, ForceMode2D.Impulse);
        // }
        
        
        //return to original scale
        elapsedTime = 0f;
        moveTime = (60 * timeToMove / GameManager.Instance.songBpm) * ((speedFactor - 1) / speedFactor);

        while (elapsedTime < moveTime)
        {
            transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / moveTime));
            transform.localPosition = Vector3.Lerp(targetPosition, originalPosition, (elapsedTime / moveTime));
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        transform.localScale = originalScale;
        transform.localPosition = originalPosition;
        yield return null;
        
        

    }

    private IEnumerator MoveToCoroutine(bool isMovingBack)
    {
        Vector2 destination = isMovingBack ? (Vector2)transform.position - moveVector : (Vector2)transform.position + moveVector;
        float elapsedTime = 0f;
        float moveTime = 60 * timeToMove / GameManager.Instance.songBpm;

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
                StartCoroutine(BinaryPlatformCoroutine(false));
                // Color temp = spriteRenderer.color;
                // temp.a = deactivatedAlpha;
                // spriteRenderer.color = temp;
                break;
            
            case PlatformType.Moving:
                StopCoroutine("MoveToCoroutine");
                StartCoroutine(MoveToCoroutine(true));
                break;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharController.Instance.mostRecentlyTouchedPlatform = this;
            isPlayerTouching = true;
            if (isHazard)
            {
                CharController.Instance.Die();
            }

            else if (type == PlatformType.Fading)
            {
                StartCoroutine(FadeCoroutine());
            }

            else if (type == PlatformType.Moving  &&
                     (CharController.Instance.transform.localPosition.y > transform.localPosition.y || isWallSlideable))
            {
                CharController.Instance.transform.SetParent(transform);
            }

            // crushed under platform
            else if (type == PlatformType.Moving  &&
                     (CharController.Instance.transform.localPosition.y < transform.localPosition.y && CharController.Instance.isGrounded))
            {
                CharController.Instance.Die();
            }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerRelativePosition = Vector2.zero;
            CharController.Instance.transform.SetParent(null);
            isPlayerTouching = false;
            CharController.Instance.isJumpBoosted = false;
        }
    }
    
    private void OnCollisionStay2D(Collision2D other)
    {
        if(other.gameObject.CompareTag("Player"))
        { 
            Collider2D collider = other.collider;
            Vector2 contactPoint = other.contacts[0].point;
            Vector2 center = collider.bounds.center;
            Vector2 dirVector = (contactPoint - center).normalized;
            Vector2 roundedVector;
            if (Math.Abs(dirVector.x) > Math.Abs(dirVector.y))
            {
                roundedVector = Vector2.right * Math.Sign(dirVector.x); 
            }
            else
            {
                roundedVector = Vector2.up * Math.Sign(dirVector.y); 
            }
            
            playerRelativePosition = roundedVector;
            // print(roundedVector);
        }
    }
    
    private IEnumerator BinaryPlatformCoroutine(bool toFull)
    {
        Color original = spriteRenderer.color;
        Color faded = original;
        faded.a = deactivatedAlpha;
        Color full = original;
        full.a = 1;
        Color fadeTo = toFull ? full : faded;
        Color initialBurst = toFull ? Color.cyan : Color.black;
        
        
        float elapsedTime = 0f;
        float fadeTime = .35f;
        spriteRenderer.color = initialBurst;

        while (elapsedTime < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(initialBurst, fadeTo, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        spriteRenderer.color = fadeTo;
    }


    private IEnumerator FadeCoroutine()
    {
        Color original = spriteRenderer.color;
        Color faded = original;
        faded.a = 0f;
        
        float elapsedTime = 0f;
        float fadeTime = .5f;

        while (elapsedTime < fadeTime)
        {
            spriteRenderer.color = Color.Lerp(original, faded, elapsedTime / fadeTime);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        platformCollider.enabled = false;
        spriteRenderer.color = faded;
        yield return new WaitForSeconds(3f);
        platformCollider.enabled = true;
        spriteRenderer.color = original;
    }
   
    
}
