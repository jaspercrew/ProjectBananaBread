using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    public float lifetime = 10f;
    // protected float GravityScale = 0f;

    // trackers
    protected Collider2D collider2D;
    protected Rigidbody2D rigidbody2D;

    protected virtual void Start()
    {
        StartCoroutine(ProjectileLifetimeCheck());
    }

    public virtual void Initialize(Vector2 velocity)
    {
        collider2D = GetComponent<Collider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        rigidbody2D.velocity = velocity;
        transform.eulerAngles = new Vector3(
            0,
            0,
            Mathf.Rad2Deg * Mathf.Atan2(velocity.y, velocity.x)
        );
    }

    protected virtual IEnumerator ProjectileLifetimeCheck()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}