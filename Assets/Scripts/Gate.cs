using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    private bool isOpen;
    public List<ActivatorTrigger> triggers;
    public float yOffset = 5f;
    public float openTime = 3f;


    // Start is called before the first frame update
    private void Start()
    {
        
    }

    private void Update()
    {
        if (!isOpen)
        {
            bool allActive = true;
            foreach (ActivatorTrigger trigger in triggers)
            {
                allActive &= trigger.isActivated;
            }

            if (allActive)
            {
                StartCoroutine(Open());
            }
        }

    }


    public IEnumerator Open()
    {
        //Debug.Log("OPENED");
        isOpen = true;
        Vector3 moveTo = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
        float elapsedTime = 0;
        float waitTime = openTime;

        while (elapsedTime < waitTime)
        {
            transform.position = Vector3.Lerp(transform.position, moveTo, (elapsedTime / waitTime));
            elapsedTime += Time.fixedDeltaTime;
            yield return null;
        }  
        // Make sure we got there
        transform.position = moveTo;
        yield return null;
    }
}
