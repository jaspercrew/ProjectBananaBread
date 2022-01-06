using UnityEngine;

public abstract class Projectile : Entity {
    // config values
    // protected float GravityScale = 0f;

    // trackers
    protected new BoxCollider2D collider;
    protected new Rigidbody2D rigidbody;
    
    // Start is called before the first frame update
    private void Start() {
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        // rigidbody.gravityScale = GravityScale;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
