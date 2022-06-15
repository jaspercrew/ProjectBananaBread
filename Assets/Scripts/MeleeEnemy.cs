using System.Collections;
using UnityEngine;

public class MeleeEnemy : CloseAttackerEnemy
{
    //[SerializeField] protected Transform attackPoint;
    [SerializeField] protected float knockbackVal = 2f;
    protected bool IsAttacking;
    [SerializeField]
    protected int attackDamage = 10;
    protected IEnumerator AttackCo;
    
    // Start is called before the first frame update
    // protected override void Start()
    // {
    //     base.Start();
    //     //attackCD = 2f;
    // }
    

    // protected override bool IsFrozen()
    // {
    //     return base.IsFrozen();
    // }

    protected override bool CanMove()
    {
        return base.CanMove() && !IsAttacking && !PlayerInAttackRange;
    }


    protected override void DoAttack()
    {
        IsAttacking = true;
        Animator.SetTrigger(Attack);
        AttackCo = AttackCoroutine();
        StartCoroutine(AttackCo);
    }

    protected override bool AttackConditions()
    {
        return base.AttackConditions() && !IsAttacking;
    }

    protected IEnumerator AttackCoroutine()
    {
        Rigidbody.velocity = Vector2.ClampMagnitude(Rigidbody.velocity, .01f);
        // enemy attack modifiers
        // float attackBoost = 1.5f;
        const float beginAttackDelay = .55f;
        const float hitEndDelay = .4f;

        yield return new WaitForSeconds(beginAttackDelay);

        const int maxHits = 20;
        Collider2D[] hitColliders = new Collider2D[maxHits];
        int numHits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, attackRange,
            hitColliders, PlayerMask);

        if (numHits > 0) {
            foreach (Collider2D p in hitColliders) {
                if (p != null && p.gameObject.GetComponent<CharController>() != null && CharController.isParrying) {
                    // StartCoroutine(PauseAnimatorCoroutine(.2f));
                    CharController.CounterStrike(GetComponent<Enemy>());
                }
                else if (p != null && p.gameObject.GetComponent<CharController>() != null)
                {
                    CharController.TakeDamage(attackDamage);
                }
            }
        }
        
        yield return new WaitForSeconds(hitEndDelay);
        IsAttacking = false;
    }
    
    

    public override void Interrupt() { // should stop all relevant coroutines
        if (AttackCo != null) {
            //print("stopped AttackCoroutine");
            StopCoroutine(AttackCo); // interrupt attack if take damage
            Rigidbody.velocity = Vector2.zero;
            IsAttacking = false;
        }
    }
    // protected override void DisableFunctionality() {
    //     //StopAllCoroutines();
    //     isAlive = false;
    // }
    //
    // protected override void EnableFunctionality() {
    //     isAlive = true;
    //     isAttacking = false;
    // }
}
