using UnityEngine;

public class WindForce : BinaryEntity
{

    private AreaEffector2D areaEffector;
    private LayerMask moveableMask;
    // Start is called before the first frame update
    protected override void Start()
    {
        areaEffector = GetComponent<AreaEffector2D>();
        moveableMask = LayerMask.NameToLayer("Moveable");
        areaEffector.useColliderMask = true;
        areaEffector.colliderMask = moveableMask;
        base.Start();
        if (!SceneInformation.Instance.isWindScene)
        {
            areaEffector.forceMagnitude = 0;
        }
    }
    
    private void ChangeWind(WindInfo wind)
    {
         if (!SceneInformation.Instance.isWindScene) return;
         areaEffector.forceAngle = wind.isHorizontal ? 0 : 90;
         areaEffector.forceMagnitude = wind.forceStrength;
     }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        ChangeWind(SceneInformation.Instance.altStateWind);
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        ChangeWind(SceneInformation.Instance.realStateWind);
    }
}
