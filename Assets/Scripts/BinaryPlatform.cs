using UnityEngine;

public class BinaryPlatform : FluidPlatform //is only active in one state
{
    private BoxCollider2D boxCollider;
    private SpriteRenderer spriteRenderer;
    public bool isActive = true;
    // Start is called before the first frame update
    private void Awake() {
        boxCollider = GetComponent<BoxCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void SwitchToState(EnvironmentState state) {
        // Debug.Log("run");
        boxCollider.enabled = !isActive;
        Color c = spriteRenderer.color;
        c.a = isActive? 0 : 1;
        spriteRenderer.color = c;
        isActive = !isActive;
    }
}
