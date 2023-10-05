public class PlayerRewinder : BeatEntity
{
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        StartCoroutine(CharController.instance.StartRewind());
    }
}