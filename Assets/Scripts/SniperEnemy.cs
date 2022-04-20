using UnityEngine;

public class SniperEnemy : Enemy
{
    protected void Update()
    {
        PlayerScan_Update();
        AttackLoop_Update();
    }

    protected override void DoAttack()
    {
        
    }
}