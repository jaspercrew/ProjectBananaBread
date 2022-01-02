using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Projectile : Entity {
    //config values
    private float gravityScale = 0f;
    
    //trackers
    private BoxCollider2D _collider;
    private Rigidbody2D _rigidbody;
    // Start is called before the first frame update
    void Start() {
        _collider = GetComponent<BoxCollider2D>();
        _rigidbody = GetComponent<Rigidbody2D>();
        _rigidbody.gravityScale = gravityScale;


    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
