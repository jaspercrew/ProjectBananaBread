using UnityEngine;

public class ActivatedEntity : BeatEntity
{
    [Min(1)] public int beatsToSwitch = 1;
    public bool IsActive { get; private set; } = false;
    
    private int beatsCounter = 0;

    // Start is called before the first frame update
    // protected override void Start()
    // {
    //     base.Start();
    // }
    
    public override void Beat()
    {
        beatsCounter++;
        if (beatsCounter == beatsToSwitch)
        {
            IsActive = !IsActive;
            beatsCounter = 0;
        }
    }
}
