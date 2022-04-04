using System;
using UnityEngine;

public class WindEmitter : BinaryEntity
{
    [Serializable]
    public class WindInfo
    {
        public float forceStrength;
        public float speedOnPlayer;
        public bool isHorizontal;
    }
    
    public bool isWindEnabled;
    public WindInfo realStateWind;
    public WindInfo altStateWind;
    // public float windSpeedOnPlayer = 3;
    
    [HideInInspector]
    public WindInfo currentWind;
    
    private AreaEffector2D areaEffector;
    // private BoxCollider2D boxCollider;
    // private const float WindForce = 5f;
    
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

    private void ChangeWind(WindInfo wind)
    {
        if (!isWindEnabled) return;
        areaEffector.forceAngle = wind.isHorizontal ? 0 : 90;
        areaEffector.forceMagnitude = wind.forceStrength;
    }
    
    // Start is called before the first frame update
    protected override void Start()
    {
        areaEffector = GetComponent<AreaEffector2D>();
        // boxCollider = GetComponent<BoxCollider2D>();
        base.Start();
    }

    // moved to WindEmitterChild.cs
    // private void OnTriggerEnter2D(Collider2D other)
    // {
    //     CharController.Instance.currentWindZone = this;
    // }
    //
    // private void OnTriggerExit2D(Collider2D other)
    // {
    //     CharController.Instance.currentWindZone = null;
    // }
}
