using UnityEngine;
using UnityEngine.Assertions;

public class ThrowerEnemy : Enemy
{
    
    private Rigidbody2D sentProjectile;
    [Header("ThrowerEnemy settings")]
    public Rigidbody2D projectilePrefab;
    public float forceMult;
    protected override void Start()
    {
        base.Start();
        Animator.speed = 2f; //TODO: actual animations
    }

    protected void Update()
    {
        PlayerScan_Update();
        AttackLoop_Update();
        TurnAround_Update();
    }

    protected override void TurnAround_Update()
    {
        if (CharController.Instance.transform.position.x < transform.position.x) {
            FaceLeft();
        }
        else {
            FaceRight();
        }
    }
    

    protected override void DoAttack()
    {
        Animator.SetTrigger(Attack);
        //Vector2 direction = ((charController.transform.position + new Vector3(0, .5f, 0)) - transform.position).normalized;
        sentProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        //Debug.Log(sentProjectile.GetComponent<Projectile>().lifetime);
        float xDiff = CharController.Instance.transform.position.x - transform.position.x;
        Vector2 dir =  xDiff > 0?  
            new Vector2(1, 1) : new Vector2(-1, 1);
        sentProjectile.gameObject.GetComponent<ExplosiveBomb>().Initialize(dir, xDiff * forceMult);
    }
}