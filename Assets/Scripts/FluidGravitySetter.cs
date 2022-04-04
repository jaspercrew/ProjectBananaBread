using System;
using UnityEngine;

public class FluidGravitySetter : BinaryEntity
{
    [Serializable]
    public class GravityInfo
    {
        public bool isDown;
        public bool isEnabled;
    }
    // private Collider2D collider;
    public GravityInfo realStateGravity;
    public GravityInfo altStateGravity;

    private Transform arrowTransform;
    private SpriteRenderer arrowSprite;
    private readonly Vector3 upArrow = new Vector3(.5f, .5f, 0);
    private readonly Vector3 downArrow = new Vector3(.5f, -.5f, 0);
    
    
    [HideInInspector]
    public GravityInfo currentGravity;
    
    protected override void Start()
    {
        base.Start();
        arrowTransform = transform.parent.Find("Arrow");
        arrowSprite = arrowTransform.GetComponent<SpriteRenderer>();
        //Debug.Log(arrowTransform.position);
        CheckArrow();
    }

    protected override void TurnShifted()
    {
        if (!arrowTransform)
        {
            arrowTransform = transform.parent.Find("Arrow");
        }

        currentGravity = realStateGravity;
        CheckArrow();
    }

    protected override void TurnUnshifted()
    {
        if (!arrowTransform)
        {
            arrowTransform = transform.parent.Find("Arrow");
        }
        currentGravity = altStateGravity;
        CheckArrow();
    }

    private void CheckArrow()
    {
        if (!arrowSprite)
        {
            arrowSprite = arrowTransform.GetComponent<SpriteRenderer>();
        }
        if (!currentGravity.isEnabled)
        {
            arrowSprite.enabled = false;
        }
        else
        {
            arrowSprite.enabled = true;
            if (currentGravity.isDown)
            {
                arrowTransform.localScale = downArrow;
            }
            else
            {
                arrowTransform.localScale = upArrow;
            }
        }

    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (currentGravity.isEnabled)
        {
            Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();

            if (other.CompareTag("Player"))
            {
                if (!currentGravity.isDown)
                {
                    CharController.Instance.Invert();
                }
                else
                {
                    CharController.Instance.DeInvert();
                }
            }
            else if (rb != null)
            {
                Debug.Log("rb is NOT null, not player");
                GravityInfo toSetTo = currentGravity;
                if (toSetTo.isEnabled)
                {
                    int dir = toSetTo.isDown ? 1 : -1;
                    float strength = Mathf.Abs(rb.gravityScale);
                    rb.gravityScale = dir * strength;
                }
            }
        }
    }
}
