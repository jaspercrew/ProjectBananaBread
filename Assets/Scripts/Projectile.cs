using UnityEngine;

public class Projectile : Entity {
    // config values
    private const float GravityScale = 0f;

    // trackers
    private new BoxCollider2D collider;
    private new Rigidbody2D rigidbody;
    
    // Start is called before the first frame update
    private void Start() {
        collider = GetComponent<BoxCollider2D>();
        rigidbody = GetComponent<Rigidbody2D>();
        rigidbody.gravityScale = GravityScale;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
