using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BinaryPlatform : FluidPlatform //is only active in one state
{
    private BoxCollider2D _collider;
    private SpriteRenderer _spriteRenderer;
    public bool isActive = true;
    // Start is called before the first frame update
    private void Awake() {
        _collider = GetComponent<BoxCollider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SwitchToState(EnvironmentState state) {
        // Debug.Log("run");
        _collider.enabled = !isActive;
        Color c = _spriteRenderer.color;
        c.a = isActive? 0 : 1;
        _spriteRenderer.color = c;
        isActive = !isActive;
    }
}
