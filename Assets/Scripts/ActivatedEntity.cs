using System.Text;
using UnityEngine;

public class ActivatedEntity : BeatEntity
{
    [Min(1)] public int beatsToSwitch = 1;
    public bool InitialIsActive = false;
    public bool IsActive { get; private set; }
    private int beatsCounter = 0;

    //Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        IsActive = InitialIsActive;
    }
    
    public override void Beat()
    {
        beatsCounter++;
        if (beatsCounter == beatsToSwitch)
        {
            if (IsActive)
                Deactivate();
            else
                Activate();
            
            beatsCounter = 0;
        }
    }

    protected virtual void Activate()
    {
        IsActive = true;
    }

    protected virtual void Deactivate()
    {
        IsActive = false;
    }
}
