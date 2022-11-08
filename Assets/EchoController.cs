using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EchoController : MonoBehaviour
{
    public float echoDelay = 2f;


    private IEnumerator DelayCoroutine(Vector3 pos)
    {
        yield return new WaitForSeconds(echoDelay);
        transform.position = pos;
    }

    private void FixedUpdate()
    {
        StartCoroutine(DelayCoroutine(CharController.Instance.transform.position));
    }
}
