using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BeatBackground : BeatEntity {

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

    protected override void MicroBeatAction()
    {
        StopCoroutine("MoveToScale");
        StartCoroutine("MoveToScale", beatScale);
    }


}