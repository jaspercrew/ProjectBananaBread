using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BurstOrb : BeatEntity
{
    private const int numCircles = 8;
    private BeatOrb[] circles;
    private int circleIterator = 0;
    protected override void Start()
    {
        circles = transform.GetComponentsInChildren<BeatOrb>();
        base.Start();
    }

    protected override void MicroBeatAction()
    {
        //print(circleIterator);
        
        if (circleIterator == actionMicroBeats.Count - 1)
        {
            PlayerImpulse();
            circleIterator = 0;
        }
        else
        {
            circles[circleIterator].Trigger();
            circleIterator++;
        }
    }

    private void PlayerImpulse()
    {
        Trigger();
        print("impulse triggered");
    }


    protected override void MotifResetAction()
    {
        circleIterator = 0;
    }
    
    public Vector3 beatScale;
    public Vector3 restScale;
    public float timeToBeat;
    public float restSmoothTime;
    private IEnumerator MoveToScale(Vector3 _target)
    {
        Vector3 _curr = transform.localScale;
        Vector3 _initial = _curr;
        float _timer = 0;

        while (_curr != _target)
        {
            _curr = Vector3.Lerp(_initial, _target, _timer / timeToBeat);
            _timer += Time.deltaTime;

            transform.localScale = _curr;

            yield return null;
        }
    }

    private void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, restScale, restSmoothTime * Time.deltaTime);
    }

    public void Trigger()
    {
        StopCoroutine("MoveToScale");
        StartCoroutine("MoveToScale", beatScale);
    }
}
