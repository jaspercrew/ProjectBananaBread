using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class BinaryPlatform : FluidPlatform //is only active in one state
{
    private Collider2D _collider;
    private SpriteRenderer _spriteRenderer;
    public bool isActive = true;
    // Start is called before the first frame update
    void Start() {
        _collider = GetComponent<Collider2D>();
        _spriteRenderer = GetComponent<SpriteRenderer>();

    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public override void SwitchState(EnvironmentState state) {
        //Debug.Log("run");
        if (isActive) {
            _collider.enabled = false;
            Color c = _spriteRenderer.color;
            c.a = 0;
            _spriteRenderer.color = c;
        }
        else {
            _collider.enabled = true;
            Color c = _spriteRenderer.color;
            c.a = 1;
            _spriteRenderer.color = c;
        }
        isActive = !isActive;
    }
}
