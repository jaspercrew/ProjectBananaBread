using System.Collections;
using UnityEngine;

public class ExplosiveProjectileParent : Projectile
{
    public bool canHitPlayer;
    public bool canHitEnemy;
    public int damage;
    public float radius = 3f;

    public SpriteRenderer explodeFX;
    private SpriteRenderer instantiatedExplodeFX;

    protected override IEnumerator ProjectileLifetimeCheck()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }
    

    protected void Explode()
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
