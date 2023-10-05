using System.Collections;
using UnityEngine;

public class Projectile : MonoBehaviour
{
    // protected float GravityScale = 0f;

    // trackers
    protected Collider2D Collider2D;
    protected Rigidbody2D Rigidbody2D;
    public float lifetime = 10f;

    protected virtual void Start()
    {
        StartCoroutine(ProjectileLifetimeCheck());
    }

    public virtual void Initialize(Vector2 velocity)
    {
        Collider2D = GetComponent<Collider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Rigidbody2D.velocity = velocity;
        transform.eulerAngles = new Vector3(
            0,
            0,
            Mathf.Rad2Deg * (Mathf.Atan2(velocity.y, velocity.x))
        );
    }

    protected virtual IEnumerator ProjectileLifetimeCheck()
    {
        yield return new WaitForSeconds(lifetime);
        Destroy(gameObject);
    }
}
