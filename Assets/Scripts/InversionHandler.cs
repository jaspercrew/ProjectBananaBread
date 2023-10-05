public class InversionHandler : BeatEntity
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    protected override void MicroBeatAction()
    {
        VolumeManager.instance.ToggleVolume();

        if (CharController.instance.isInverted)
            CharController.instance.DeInvert();
        else
            CharController.instance.Invert();

        base.MicroBeatAction();
    }
}