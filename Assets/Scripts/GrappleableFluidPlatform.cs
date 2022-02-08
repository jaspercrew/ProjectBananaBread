using UnityEngine;

public class GrappleableFluidPlatform : FluidPlatform
{
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    
    // Start is called before the first frame update
    private void Awake() {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CheckPlatform(GameManager.Instance.currentState);
    }

    protected override void ActivatePlatform() {
        isGrappleable = true;
    }
    protected override void DeactivatePlatform() {
        isGrappleable = false;
    }
}

    
