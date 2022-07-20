using UnityEngine;

public class BeatEntity : Entity
{
    [Min(1)] public int beatsToAction = 1;
    protected int beatsCounter = 0;

    // Start is called before the first frame update
    protected virtual void Start()
    {

    }

    protected virtual void BeatAction()
    {
        
    }
    
    public void Beat()
    {
        beatsCounter++;
        if (beatsCounter == beatsToAction)
        {
            BeatAction();
            beatsCounter = 0;
        }
    }

   
}
