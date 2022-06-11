using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    public int stateTrigger1 = 75;
    public int stateTrigger2 = 50;
    public int stateTrigger3 = 25;
    public float defensiveCooldown = 5f;

    public float dodgeTime = .5f;
    public float blockTime = .5f;
    
    private int attackIter = 0;
    private int bossState = 0;
    private bool isParrying;
    private bool isDodging;
    private bool isAttacking;
    
    
    
    
    
    protected override void Start()
    {
        //playerMask = LayerMask.NameToLayer("Default");
        //Animator.SetBool("Grounded", true);
        base.Start();
    }

    protected override bool AttackConditions()
    {
        return base.AttackConditions() && !isDodging && !isParrying;
    }


    protected override bool CanMove()
    {
        return base.CanMove() && !isAttacking && !playerInAttackRange && !isParrying;
    }

    protected void BossState_Update()
    {
        if (bossState == 0 && CurrentHealth <= stateTrigger1)
        {
            bossState = 1;
        }
        else if (bossState == 1 && CurrentHealth <= stateTrigger2)
        {
            bossState = 2;
        }
        else if (bossState == 2 && CurrentHealth <= stateTrigger3)
        {
            bossState = 3;
        }

    }

    protected override void DoAttack()
    {
        StartCoroutine(SlashLoop());
        // switch (bossState)
        // {
        //     case 0:
        //         if (attackIter == 0)
        //         {
        //             
        //         }
        //         if (attackIter == 1)
        //         {
        //             
        //         }
        //         if (attackIter == 2)
        //         {
        //             
        //         }
        //         break;
        // }

    }
    protected override void TurnAround_Update() {
        if (!CanMove())
        {
            return;
        }
        if (Rigidbody.velocity.x > 0) {
            FaceRight();
        }
        else if (Rigidbody.velocity.x < 0) {
            FaceLeft();
        }
        
        Animator.SetInteger(AnimState, Mathf.Abs(Rigidbody.velocity.x) > .1 ? 1 : 0);
        
    }

    private IEnumerator SlashLoop()
    {

        yield return StartCoroutine(Slash(1, 10.0f, .3f, 
            .2f, 1f, "Attack1"));
        yield return StartCoroutine(Slash(1, 10.0f, .3f, 
            .2f, 1f, "Attack2"));
        yield return StartCoroutine(Slash(1, 10.0f, .3f, 
            .2f, 1f, "Attack3"));

    }

    private IEnumerator Slash(int damage, float attackBoost, float beginAttackDelay, 
        float endAttackDelay, float attackTimeMultiplier, string trigger) {
        Animator.SetTrigger(trigger);
        isAttacking = true;
        
        if (Rigidbody.velocity.x > 0)
        {
            Rigidbody.velocity = 
                new Vector2(Math.Min(Rigidbody.velocity.x, .01f), Rigidbody.velocity.y);
        }
        else
        {
            Rigidbody.velocity = 
                new Vector2(Math.Max(Rigidbody.velocity.x, -.01f), Rigidbody.velocity.y);
        }
        

        Rigidbody.AddForce(new Vector2((transform.localScale.x > 0 ? -1 : 1) * attackBoost, 0), ForceMode2D.Impulse);

        yield return new WaitForSeconds(beginAttackDelay * attackTimeMultiplier);
        
        
        const int maxEnemiesHit = 1;
        Collider2D[] hitColliders = new Collider2D[maxEnemiesHit];
        // scan for hit enemies
        int hits = Physics2D.OverlapCircleNonAlloc(
            attackPoint.position, attackRange, hitColliders, playerMask.value);



        foreach (Collider2D obj in hitColliders)
        {
            // if (obj == null)
            //     print("x");
            // else
            //     print(obj.gameObject);
            
            if (obj != null && obj.gameObject.GetComponent<CharController>() != null)
            {
                CharController.Instance.TakeDamage(damage);
            }
        }

        yield return new WaitForSeconds(endAttackDelay * attackTimeMultiplier * 3 / 4);
        
        if (Rigidbody.velocity.x > 0)
        {
            Rigidbody.velocity = 
                new Vector2(Math.Min(Rigidbody.velocity.x, .01f), Rigidbody.velocity.y);
        }
        else
        {
            Rigidbody.velocity = 
                new Vector2(Math.Max(Rigidbody.velocity.x, -.01f), Rigidbody.velocity.y);
        }

        isAttacking = false;
        yield return new WaitForSeconds(endAttackDelay * attackTimeMultiplier / 4);
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(attackPoint.position, attackRange);
    }

    private void Backstep()
    {
        
    }

    private void Block()
    {
        StartCoroutine(BlockCoroutine());
    }

    private IEnumerator BlockCoroutine()
    {
        Animator.SetTrigger("Block");
        isParrying = true;
        yield return new WaitForSeconds(blockTime);
        isParrying = false;
    }

    private void DashStrike()
    {
        
    }

    public override void GetHit(int damage)
    {
        base.GetHit(damage);
    }


    protected void Update()
    {
        BossState_Update();
        PlayerScan_Update();
        Pathfind_Update();
        AttackLoop_Update();
        TurnAround_Update();
    }


}
