using UnityEngine;
public class CloseAttackerEnemy : Enemy {
    
    protected virtual void Update()
    {
        PlayerScan_Update();
        Pathfind_Update();
        AttackLoop_Update();
        TurnAround_Update();
    }
}
