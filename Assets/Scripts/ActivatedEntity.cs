using System.Text;
using UnityEngine;

public class ActivatedEntity : BeatEntity
{

    public bool InitialIsActive = false;
    public bool IsActive { get; private set; }
    

    //Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        IsActive = InitialIsActive;
    }

    protected override void BeatAction()
    {
        if (IsActive)
        {
            Deactivate();
        }
        else
        {
            Activate();
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
