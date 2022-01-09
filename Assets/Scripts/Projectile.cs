using UnityEngine;

public abstract class Projectile : Entity {
    // config values
    // protected float GravityScale = 0f;

    // trackers
    protected BoxCollider2D Collider2D;
    protected Rigidbody2D Rigidbody2D;
    
    // Start is called before the first frame update
    private void Start() {
        Collider2D = GetComponent<BoxCollider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        // rigidbody.gravityScale = GravityScale;
    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
