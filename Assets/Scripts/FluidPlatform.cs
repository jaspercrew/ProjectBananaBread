using System.Runtime.ConstrainedExecution;

public abstract class FluidPlatform: Platform
{

    protected abstract void TurnShifted();
    protected abstract void TurnUnshifted();
        
    

    public override void Shift() {
        CheckPlatform();
    }

    protected void CheckPlatform() {
        if (GameManager.Instance.isGameShifted)
        {
            TurnShifted();
        }
        else
        {
            TurnUnshifted();
        }
    }
    
}
