using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    public int stateTrigger1 = 75;
    public int stateTrigger2 = 50;
    public int stateTrigger3 = 25;
    
    private int attackIter = 0;
    private int bossState = 0;
    private bool isParrying;
    private bool isDodging;
    
    
    
    
    protected override void Start()
    {
        base.Start();
        
    }

    protected override bool AttackConditions()
    {
        return base.AttackConditions();
    }

    protected override bool IsUnfrozen()
    {
        return base.IsUnfrozen() && !isParrying;
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
        switch (bossState)
        {
            case 0:
                if (attackIter == 0)
                {
                    
                }
                if (attackIter == 1)
                {
                    
                }
                if (attackIter == 2)
                {
                    
                }
                break;
        }

    }

    private void TripleSlash()
    {
        
    }

    private void Backstep()
    {
        
    }

    private void Block()
    {
        
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
        
    }


}
