using UnityEngine;


public class DamageProjectile : Projectile
{
    public bool canHitPlayer;
    public bool canHitEnemy;
    public int damage = 5;
    //protected float lifetime = 10f;

    // protected void Start()
    // {
    //     Collider2D = GetComponent<Collider2D>();
    //     StartCoroutine(ProjectileLifetimeCheck());
    // }
    //
    // public void Initialize(Vector2 velocity)
    // {
    //     Rigidbody2D = GetComponent<Rigidbody2D>();
    //     Rigidbody2D.velocity = velocity;
    //     transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (Mathf.Atan2(velocity.y, velocity.x)));
    // }
    //
    // protected IEnumerator ProjectileLifetimeCheck()
    // {
    //     yield return new WaitForSeconds(lifetime);
    //     Destroy(gameObject);
    // }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() != null) {
            if (CharController.Instance.isParrying)
            {
                Initialize(new Vector2(-Rigidbody2D.velocity.x, -Rigidbody2D.velocity.y));
                canHitEnemy = true;
            }
            else if (CharController.Instance.HasIFrames())
            {
                // return;
            }
            else
            {
                CharController.Instance.TakeDamage(damage);
                Destroy(gameObject);
            }
        }
        else if (other.gameObject.GetComponent<Enemy>() != null && canHitEnemy)
        {
            other.gameObject.GetComponent<Enemy>().TakeDamage(damage);
            Destroy(gameObject);
        }
        else if (other.gameObject.GetComponent<Platform>() != null)
        {
            Destroy(gameObject);
        }
    }
}