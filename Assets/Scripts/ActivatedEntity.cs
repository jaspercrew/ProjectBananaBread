public class ActivatedEntity : BeatEntity
{
    public bool initialIsActive;
    public bool IsActive { get; private set; }

    //Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
        IsActive = initialIsActive;
        if (initialIsActive)
            Activate();
        else
            Deactivate();
    }

    protected override void MicroBeatAction()
    {
        if (IsActive)
            Deactivate();
        else
            Activate();
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