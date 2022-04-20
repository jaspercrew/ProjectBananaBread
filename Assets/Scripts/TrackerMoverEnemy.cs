using UnityEngine;

public class TrackerMoverEnemy : Enemy
{

    protected void Update()
    {
        Pathfind_Update();
        TurnAround_Update();
    }
    protected override void Pathfind_Update()
    {
        base.Pathfind_Update();
    }
}