using UnityEngine;

public class GrappleableFluidPlatform : FluidPlatform
{
    public EnvironmentState grappleableState;
    private BoxCollider2D boxCollider;

    private SpriteRenderer spriteRenderer;

    //public bool isActive = true;
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

    
