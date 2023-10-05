using System;
using System.Collections;
using UnityEngine;

public enum PlatformType
{
    Collider,
    Moving,
    Fading,
    Impulse
}

public class BeatPlatform : ActivatedEntity
{
    private const float DeathCheckFactor = .6f;
    private const float DeactivatedAlpha = .3f;
    public bool isStatic;
    public bool isHazard;
    public PlatformType type;
    public Vector2 moveVector;
    public bool isWallSlideable;

    public Vector2 movingVelocity;
    private Vector2 lastPosition;
    private Vector2 lastVelocity;
    private IEnumerator movingCo;

    private Vector2 originalPosition;

    private Collider2D platformCollider;

    private Rigidbody2D platformRigidbody;
    private bool playerContact;

    //private bool isPlayerTouching;
    private Vector2 playerRelativePosition;
    private SpriteRenderer spriteRenderer;
    private readonly float timeToMove = 4f;

    // Start is called before the first frame update
    protected override void Start()
    {
        platformRigidbody = GetComponent<Rigidbody2D>();
        originalPosition = transform.position;
        platformCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (isHazard) spriteRenderer.color = Color.red;

        if (isWallSlideable)
            spriteRenderer.color = new Color(1f, .5f, 0);
        else
            spriteRenderer.color = new Color(.7f, .7f, 1f, 1f);

        GetComponentInChildren<TrailRenderer>().emitting = type == PlatformType.Moving;
        lastPosition = transform.position;

        base.Start();
    }

    private void Update()
    {
        if (
                playerContact
                && platformRigidbody.velocity.magnitude > 0
                && (
                    CharController.instance.transform.position.y > transform.position.y
                    || isWallSlideable
                )
            )
            //print("sticking player vel");
            //CharController.Instance.Rigidbody.velocity += platformRigidbody.velocity;
            CharController.instance.transform.position += transform.position - (Vector3) lastPosition;

        lastPosition = transform.position;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            CharController.instance.mostRecentlyTouchedPlatform = this;
            //isPlayerTouching = true;
            if (isHazard)
                CharController.instance.Die();
            else if (
                type == PlatformType.Fading
                && (
                    CharController.instance.transform.position.y > transform.position.y
                    || isWallSlideable
                )
            )
                StartCoroutine(FadeCoroutine());

            // else if ((type == PlatformType.Moving  &&
            //          CharController.Instance.transform.position.y > transform.position.y) || isWallSlideable)
            // {
            //     CharController.Instance.transform.SetParent(transform, true);
            // }

            // crushed under platform
            // else if (type == PlatformType.Moving &&
            //          (CharController.Instance.transform.localPosition.y < transform.localPosition.y &&
            //           CharController.Instance.isGrounded))
            // {
            //     CharController.Instance.Die();
            // }
        }
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            playerRelativePosition = Vector2.zero;
            playerContact = false;
            CharController.instance.mostRecentlyTouchedPlatform = null;
            //CharController.Instance.fixedJoint2D.connectedBody = null;
            //CharController.Instance.transform.SetParent(null);
            //isPlayerTouching = false;
            //CharController.Instance.isJumpBoosted = false;
        }
    }

    private void OnCollisionStay2D(Collision2D other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            var collider = other.collider;
            var contactPoint = other.contacts[0].point;
            Vector2 center = collider.bounds.center;
            var dirVector = (contactPoint - center).normalized;
            Vector2 roundedVector;
            if (Math.Abs(dirVector.x) > Math.Abs(dirVector.y))
                roundedVector = Vector2.right * Math.Sign(dirVector.x);
            else
                roundedVector = Vector2.up * Math.Sign(dirVector.y);

            playerRelativePosition = roundedVector;
            playerContact = true;
            //CharController.Instance.fixedJoint2D.connectedBody = platformRigidbody;


            // print(roundedVector);
        }
    }

    private void OnDrawGizmos()
    {
        if (type == PlatformType.Moving)
        {
            if (initialIsActive)
                Gizmos.DrawWireSphere(transform.localPosition + (Vector3) moveVector, 1f);
            else
                Gizmos.DrawWireSphere(transform.localPosition - (Vector3) moveVector, 1f);
        }
    }

    protected override void MicroBeatAction()
    {
        if (isStatic) return;

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

                var charCollider = CharController.instance.GetComponent<BoxCollider2D>();
                var bounds = charCollider.bounds.extents;
                var halfWidth = Mathf.Abs(bounds.x);
                var halfHeight = Mathf.Abs(bounds.y);
                var center =
                    (Vector2) CharController.instance.transform.position
                    + charCollider.offset.y * Vector2.up;

                var bottomMiddle = center + halfHeight * Vector2.down;
                var bottomLeft = bottomMiddle + halfWidth * DeathCheckFactor * Vector2.left;
                var bottomRight = bottomMiddle + halfWidth * DeathCheckFactor * Vector2.right;

                var topMiddle = center + halfHeight * Vector2.up;
                var topLeft = topMiddle + halfWidth * DeathCheckFactor * Vector2.left;
                var topRight = topMiddle + halfWidth * DeathCheckFactor * Vector2.right;
                var platformBounds = platformCollider.bounds;
                if (
                    platformBounds.Contains(bottomLeft)
                    || platformBounds.Contains(bottomRight)
                    || platformBounds.Contains(topLeft)
                    || platformBounds.Contains(topRight)
                    || platformBounds.Contains(center)
                )
                {
                    //CharController.Instance.Die(); //FIND NEW SOLUTION
                }

                break;

            case PlatformType.Moving:
                if (movingCo != null)
                    StopCoroutine(movingCo);
                movingCo = MoveToCoroutine(false);
                StartCoroutine(movingCo);
                break;

            case PlatformType.Impulse:
                StartCoroutine(ImpulseCoroutine());
                break;
        }
    }

    private IEnumerator ImpulseCoroutine()
    {
        yield return null;
        // const float speedFactor = 8;
        // const float enlargeAdder = .5f;
        //
        // //enlarge
        // Vector2 originalScale = transform.localScale;
        // Vector2 positiveMoveVector = new Vector2(Math.Abs(moveVector.normalized.x), Math.Abs(moveVector.normalized.y));
        // Vector2 targetScale = originalScale + (positiveMoveVector * enlargeAdder);
        // Vector2 originalPosition = transform.localPosition;
        // Vector2 targetPosition = originalPosition + (moveVector.normalized * enlargeAdder / 2);
        //
        // float elapsedTime = 0f;
        // float moveTime = (60 * timeToMove / GameManager.Instance.songBpm) / speedFactor;
        //
        // while (elapsedTime < moveTime)
        // {
        //     if (isPlayerTouching /*&& playerRelativePosition == moveVector.normalized*/)
        //     {
        //         CharController.Instance.isJumpBoosted = true;
        //     }
        //     transform.localScale = Vector3.Lerp(originalScale, targetScale, (elapsedTime / moveTime));
        //     transform.localPosition = Vector3.Lerp(originalPosition, targetPosition, (elapsedTime / moveTime));
        //     elapsedTime += Time.deltaTime;
        //     yield return null;
        // }
        // transform.localScale = targetScale;
        // transform.localPosition = targetPosition;
        // yield return null;
        //
        // // if (isPlayerTouching && playerRelativePosition == moveVector.normalized)
        // // {
        // //     CharController.Instance.GetComponent<Rigidbody2D>().AddForce(moveVector, ForceMode2D.Impulse);
        // // }
        //
        //
        // //return to original scale
        // elapsedTime = 0f;
        // moveTime = (60 * timeToMove / GameManager.Instance.songBpm) * ((speedFactor - 1) / speedFactor);
        //
        // while (elapsedTime < moveTime)
        // {
        //     transform.localScale = Vector3.Lerp(targetScale, originalScale, (elapsedTime / moveTime));
        //     transform.localPosition = Vector3.Lerp(targetPosition, originalPosition, (elapsedTime / moveTime));
        //     elapsedTime += Time.deltaTime;
        //     yield return null;
        // }
        // transform.localScale = originalScale;
        // transform.localPosition = originalPosition;
        // yield return null;
    }

    private IEnumerator MoveToCoroutine(bool isMovingBack)
    {
        var destination = isMovingBack ? originalPosition : originalPosition - moveVector;
        var movementDirection = (destination - (Vector2) transform.position).normalized;
        var velocityMultiplier = 1.5f;
        var proximityDetection = .4f;
        var elapsedTime = 0f;
        var moveTime = 60 / GameManager.instance.songBpm * timeToMove;

        while (elapsedTime < moveTime)
        {
            // Vector3 lerpPosition = Vector3.Lerp(transform.position, destination, (elapsedTime / moveTime));

            // print((lerpPosition - transform.position) / Time.deltaTime);
            // transform.position = lerpPosition;
            platformRigidbody.velocity += movementDirection * (velocityMultiplier * (elapsedTime / moveTime));
            movingVelocity = platformRigidbody.velocity;
            elapsedTime += Time.fixedDeltaTime;

            if (Vector2.Distance(transform.position, destination) < proximityDetection)
            {
                lastVelocity = platformRigidbody.velocity;
                movingVelocity = lastVelocity;
                //print(movingVelocity);
                platformRigidbody.velocity = Vector2.zero;
                transform.position = destination;
                break;
            }

            // Yield here
            yield return new WaitForFixedUpdate();
        }
        // Make sure we got there

        yield return new WaitForSeconds(.35f);
        //print("moving vel set to 0");
        movingVelocity = Vector2.zero;
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
                if (movingCo != null)
                    StopCoroutine(movingCo);
                movingCo = MoveToCoroutine(true);
                StartCoroutine(movingCo);
                break;
        }
    }

    private IEnumerator BinaryPlatformCoroutine(bool toFull)
    {
        var original = spriteRenderer.color;
        var faded = original;
        faded.a = DeactivatedAlpha;
        var full = original;
        full.a = 1;
        var fadeTo = toFull ? full : faded;
        var initialBurst = toFull ? Color.white : Color.black;

        var elapsedTime = 0f;
        var fadeTime = .25f;
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
        var original = spriteRenderer.color;
        var faded = original;
        faded.a = 0f;

        var elapsedTime = 0f;
        var fadeTime = .8f;

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