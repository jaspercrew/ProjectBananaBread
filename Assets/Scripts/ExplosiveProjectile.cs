using UnityEngine;

public class ExplosiveProjectile : ExplosiveProjectileParent
{
    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (canHitPlayer && other.gameObject.GetComponent<CharController>() != null) {
            if (CharController.Instance.isParrying)
            {
                Vector2 v = Rigidbody2D.velocity;
                Initialize(-v);
                canHitEnemy = true;
            }
            else if (CharController.Instance.HasIFrames())
            {
                // return;
            }
            else
            {
                Explode();
            }
        }
        else if (canHitEnemy && other.gameObject.GetComponent<Enemy>() != null)
        {
            Explode();
        }
        else if (other.gameObject.GetComponent<Platform>() != null)
        {
            Explode();
        }
    }
}
