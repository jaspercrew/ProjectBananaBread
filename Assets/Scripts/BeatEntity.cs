using System.Collections.Generic;
using UnityEngine;

public class BeatEntity : Entity
{
    [Min(1)] public int motifLength = 1; //length of motif in beats

    public List<int> actionMicroBeats = new List<int>();
    protected int beatsCounter;
    protected int microBeatCount;

    // Start is called before the first frame update
    protected virtual void Start()
    {
    }

    protected virtual void MicroBeatAction()
    {
    }

    protected virtual void FullBeatAction()
    {
    }

    protected virtual void MotifResetAction()
    {
    }

    public void MicroBeat()
    {
        //print(actionMicroBeats.Contains(microbeat));
        if (actionMicroBeats.Count > 0 && actionMicroBeats.Contains(microBeatCount)) MicroBeatAction();
        //beatsCounter = 0;
        if (microBeatCount % GameManager.MicroBeatsInBeat == 0)
        {
            FullBeatAction();
            beatsCounter++;
        }

        microBeatCount++;
        if (microBeatCount == GameManager.MicroBeatsInBeat * motifLength)
        {
            microBeatCount = 0;
            MotifResetAction();
        }
    }
}