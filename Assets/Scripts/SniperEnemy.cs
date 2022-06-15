using UnityEngine;

public class SniperEnemy : Enemy
{
    
    private Rigidbody2D sentProjectile;
    [Header("SniperEnemy settings")]
    public Rigidbody2D projectilePrefab;
    public float projSpeed;
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
        Vector2 direction = ((CharController.transform.position + new Vector3(0, -.5f, 0)) - transform.position).normalized;
        sentProjectile = Instantiate(projectilePrefab, transform.position, transform.rotation);
        //Debug.Log(sentProjectile.GetComponent<Projectile>().lifetime);
        sentProjectile.gameObject.GetComponent<Projectile>().Initialize(direction * projSpeed);
    }
}