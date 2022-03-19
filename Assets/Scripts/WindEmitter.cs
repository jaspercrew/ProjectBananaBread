using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.PlayerLoop;

public enum WindState
{
    Up = 2, Down = -2, Left = -1, Right = 1, None = 0
}
public class WindEmitter : BinaryEntity
{
    public WindState realStateWind;
    public WindState altStateWind;
    public WindState currentWind;
    private AreaEffector2D areaEffector;
    private BoxCollider2D boxCollider;
    private const float WindForce = 5f;
    public float windSpeedOnPlayer = 3;
    
    protected override void TurnShifted()
    {
        base.TurnShifted();
        currentWind = altStateWind;
        ChangeWind(currentWind);
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        currentWind = realStateWind;
        ChangeWind(currentWind);
    }

    private void ChangeWind(WindState wind)
    {
        switch (wind)
        {
            case WindState.Up:
                areaEffector.forceAngle = 90f;
                areaEffector.forceMagnitude = WindForce;
                break;
            case WindState.Down:
                areaEffector.forceAngle = 270f;
                areaEffector.forceMagnitude = WindForce;
                break;
            case WindState.Left:
                areaEffector.forceAngle = 180f;
                areaEffector.forceMagnitude = WindForce;
                break;
            case WindState.Right:
                areaEffector.forceAngle = 0f;
                areaEffector.forceMagnitude = WindForce;
                break;
            case WindState.None:
                areaEffector.forceMagnitude = 0f;
                break;
        }
    }
    
    // Start is called before the first frame update
    protected override void Awake()
    {
        areaEffector = GetComponent<AreaEffector2D>();
        BoxCollider2D[] colliders = GetComponents<BoxCollider2D>();
        Debug.Assert(colliders.Length == 2);
        boxCollider = colliders[colliders[0].usedByEffector? 0 : 1];
        areaEffector.forceMagnitude = WindForce;
        base.Awake();
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        CharController.Instance.currentWindZone = this;
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CharController.Instance.currentWindZone = null;
    }
}
