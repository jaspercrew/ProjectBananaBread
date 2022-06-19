using UnityEngine;

public class ExplosiveBomb : ExplosiveProjectileParent
{
    public void Initialize(Vector2 dir, float magnitude)
    {
        Collider2D = GetComponent<Collider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Rigidbody2D.velocity = dir * Mathf.Abs(magnitude);
    }
}
