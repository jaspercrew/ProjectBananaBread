using UnityEngine.Experimental.Rendering.Universal;

public class GlobalLightManager : BinaryEntity
{
    private Light2D lightComponent;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        lightComponent = GetComponent<Light2D>();
        base.Start();
    }

    // protected override void TurnShifted()
    // {
    //     base.TurnShifted();
    // }
    
    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (SceneInformation.Instance.isDarkScene)
        {
            lightComponent.enabled = true;
        }
    }
}
