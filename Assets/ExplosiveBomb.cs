using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ExplosiveBomb : Projectile
{
    public bool canHitPlayer;
    public bool canHitEnemy;
    public int damage;
    public float radius = 3f;

    //protected float lifetime = 10f;

    public SpriteRenderer explodeFX;
    private SpriteRenderer instantiatedExplodeFX;

    public void Initialize(Vector2 dir, float magnitude)
    {

        Collider2D = GetComponent<Collider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Rigidbody2D.velocity = dir * Mathf.Abs(magnitude);
    }

    protected override IEnumerator ProjectileLifetimeCheck()
    {
        yield return new WaitForSeconds(lifetime);
        Explode();
    }
    

    private void Explode()
    {
        //Debug.Log("explode");
        instantiatedExplodeFX = Instantiate(explodeFX, transform.position, Quaternion.Euler(Vector3.zero));
        instantiatedExplodeFX.gameObject.transform.localScale *= radius * 2;
        
        LayerMask explosionMask = new LayerMask();
 
        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(transform.position, radius,
            hitColliders);
        foreach (Collider2D col in hitColliders)
        {
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
