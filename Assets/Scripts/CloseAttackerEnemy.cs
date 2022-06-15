public class CloseAttackerEnemy : Enemy
{

    protected override bool AttackConditions()
    {
        return base.AttackConditions() && PlayerInAttackRange;
    }

    protected virtual void Update()
    {
        PlayerScan_Update();
        Pathfind_Update();
        AttackLoop_Update();
        TurnAround_Update();
        PlayerPush_Update();
    }
}
