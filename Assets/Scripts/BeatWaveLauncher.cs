using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatWaveLauncher : BeatEntity
{
    public GameObject wavePrefab;
    private GameObject instantiatedWave;
    public Vector2 direction;

    public Vector2 playerVelocityMultiplier = new Vector2(3, 4);
    //private float speedMultiplier = 5f;
    
    // Start is called before the first frame update
    protected override void Start()
    {
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        instantiatedWave = Instantiate(wavePrefab,  transform.position, Quaternion.Euler(Vector3.zero), transform);
        instantiatedWave.GetComponent<BeatWave>().Initialize(direction, playerVelocityMultiplier);
        base.MicroBeatAction();
    }


}
