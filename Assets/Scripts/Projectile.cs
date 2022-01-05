using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity {
    // config values
    private float gravityScale = 0f;
    
    // trackers
    private new BoxCollider2D collider;
    private new Rigidbody2D rigidbody;
    // Start is called before the first frame update
    void Start() {
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = gravityScale;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
