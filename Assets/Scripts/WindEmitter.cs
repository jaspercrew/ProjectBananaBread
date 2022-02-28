using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum WindState
{
    Up, Down, Left, Right, None
}
public class WindEmitter : BinaryEntity
{
    public WindState realStateWind;
    public WindState altStateWind;
    private WindState currentWind;
    private AreaEffector2D areaEffector;
    private BoxCollider2D boxCollider;
    private const float windForce = 5f;
    
    protected override void ShiftEntity()
    {
        base.ShiftEntity();
        currentWind = altStateWind;
        ChangeWind(currentWind);
    }

    protected override void DeshiftEntity()
    {
        base.DeshiftEntity();
        currentWind = realStateWind;
        ChangeWind(currentWind);
    }

    private void ChangeWind(WindState wind)
    {
        switch (wind)
        {
            case WindState.Up:
                areaEffector.forceAngle = 90f;
                areaEffector.forceMagnitude = windForce;
                break;
            case WindState.Down:
                areaEffector.forceAngle = 270f;
                areaEffector.forceMagnitude = windForce;
                break;
            case WindState.Left:
                areaEffector.forceAngle = 180f;
                areaEffector.forceMagnitude = windForce;
                break;
            case WindState.Right:
                areaEffector.forceAngle = 0f;
                areaEffector.forceMagnitude = windForce;
                break;
            case WindState.None:
                areaEffector.forceMagnitude = 0f;
                break;
        }
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        areaEffector = GetComponent<AreaEffector2D>();
        boxCollider = GetComponent<BoxCollider2D>();
        areaEffector.forceMagnitude = windForce;
    }

}
