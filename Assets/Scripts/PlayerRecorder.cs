public class PlayerRecorder : BeatEntity
{
    protected override void MicroBeatAction()
    {
        CharController.instance.SpawnExtendedFadeSprite();
        CharController.instance.doRecord = true;
    }
}