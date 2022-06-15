using System.Collections;
using UnityEngine;

public class ExplosiveProjectile : Projectile
{
    public bool canHitPlayer;
    public bool canHitEnemy;
    public int damage;
    public float radius = 3f;
    //protected float lifetime = 10f;

    public SpriteRenderer explodeFX;
    private SpriteRenderer instantiatedExplodeFX;
    
    protected override IEnumerator ProjectileLifetimeCheck()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (canHitPlayer && other.gameObject.GetComponent<CharController>() != null) {
            if (CharController.Instance.isParrying)
            {
                Vector2 v = Rigidbody2D.velocity;
                Initialize(new Vector2(-v.x, -v.y));
                canHitEnemy = true;
            }
            else if (CharController.Instance.CanGetIFrames())
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

    private void Explode()
    {
        //Debug.Log("explode");
        instantiatedExplodeFX = Instantiate(explodeFX, transform.position, Quaternion.Euler(Vector3.zero));
        instantiatedExplodeFX.gameObject.transform.localScale *= radius * 2;
        
        // LayerMask explosionMask = new LayerMask();
 
        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(transform.position, radius, hitColliders);
        
        for (int i = 0; i < numHits; i++)
        {
            Collider2D col = hitColliders[i];
            if (col != null)
            {
                //Debug.Log(col.gameObject.name);
                if (canHitPlayer && col.gameObject.GetComponent<CharController>() != null)
                {
                    //Debug.Log("char damage");
                    CharController.Instance.TakeDamage(damage);
                }
                else if (canHitEnemy && col.gameObject.GetComponent<Enemy>() != null)
                {
                    //Debug.Log("enemy dmg");
                    col.gameObject.GetComponent<Enemy>().TakeDamage(damage);
                }
            }

        }
        Destroy(gameObject);
    }
}
