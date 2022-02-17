using System.Runtime.ConstrainedExecution;

public abstract class FluidPlatform: Platform
{
    public EnvironmentState enabledState;

    protected abstract void ActivatePlatform();
    protected abstract void DeactivatePlatform();
        
    

    public override void SwitchToState(EnvironmentState state) {
        CheckPlatform(state);
    }

    protected void CheckPlatform(EnvironmentState state) {
        if (enabledState == state)
        {
            ActivatePlatform();
        }
        else
        {
            DeactivatePlatform();
        }
    }
    
    // Start is called before the first frame update
    // void Start()
    // {
    //     
    // }

    // // Update is called once per frame
    // void Update()
    // {
    //     
    // }

    // public override void SwitchToState(EnvironmentState state) {
    //     
    // }
}
