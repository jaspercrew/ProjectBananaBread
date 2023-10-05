using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Rotator : BeatEntity
{
    protected override void MicroBeatAction()
    {
        StartCoroutine(RotateCoroutine());
    }

    private IEnumerator RotateCoroutine()
    {
        Vector3 originalAngle = transform.eulerAngles;
        float elapsedTime = 0;
        float rotateTime = .5f;
        while (elapsedTime < rotateTime)
        {
            transform.RotateAround(
                CharController.Instance.transform.position,
                Vector3.forward,
                Time.deltaTime * (90f / (rotateTime))
            );
            elapsedTime += Time.deltaTime;
            yield return null;
        }

        yield return null;
        transform.eulerAngles = originalAngle + new Vector3(0, 0, 90);
    }
}
