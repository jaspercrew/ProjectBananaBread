using UnityEngine;

public class BinaryCollider : BinaryEntity
{
    public bool pushInReal;

    private PlatformEffector2D platformEffector2D;
    // Start is called before the first frame update
    protected override void Start()
    {
        platformEffector2D = GetComponent<PlatformEffector2D>();
        base.Start();
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (pushInReal)
        {
            platformEffector2D.rotationalOffset = 180;
            platformEffector2D.surfaceArc = 270;
        }
        else
        {
            platformEffector2D.rotationalOffset = 0;
            platformEffector2D.surfaceArc = 90;
        }

    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (pushInReal)
        {
            platformEffector2D.rotationalOffset = 0;
            platformEffector2D.surfaceArc = 90;

        }
        else
        {
            platformEffector2D.rotationalOffset = 180;
            platformEffector2D.surfaceArc = 270;

        }
    }
}
