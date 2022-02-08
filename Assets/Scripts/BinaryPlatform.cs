using UnityEngine;

public class BinaryPlatform : FluidPlatform //is only active in one state
{
    //public EnvironmentState enabledState;
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;

    public bool isActive;

    // Start is called before the first frame update
    private void Awake() {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        CheckPlatform(GameManager.Instance.currentState);
    }

    protected override void ActivatePlatform() {
        isActive = true;
        boxCollider.size = Vector2.one;
        Color c = spriteRenderer.color;
        c.a = isActive ? 0 : 1;
        spriteRenderer.color = c;
    }

    protected override void DeactivatePlatform() {
        isActive = false;
        boxCollider.size = Vector2.zero;
        Color c = spriteRenderer.color;
        c.a = isActive ? 0 : 1;
        spriteRenderer.color = c;
    }
}
