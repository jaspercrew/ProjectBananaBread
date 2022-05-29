using System.Collections;
using System.Collections.Generic;
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
                Initialize(new Vector2(-Rigidbody2D.velocity.x, -Rigidbody2D.velocity.y));
                canHitEnemy = true;
            }
            else if (CharController.Instance.IFrames())
            {
                return;
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