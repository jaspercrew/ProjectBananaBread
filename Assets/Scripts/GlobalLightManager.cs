using UnityEngine.Experimental.Rendering.Universal;

public class GlobalLightManager : BeatEntity
{
    private Light2D lightComponent;

    // Start is called before the first frame update
    protected override void Start()
    {
        lightComponent = GetComponent<Light2D>();
        base.Start();
    }

    // protected override void TurnUnshifted()
    // {
    //     base.TurnUnshifted();
    //     if (SceneInformation.Instance.isDarkScene)
    //     {
    //         lightComponent.enabled = true;
    //     }
    // }
}
