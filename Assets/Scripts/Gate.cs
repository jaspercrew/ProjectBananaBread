using System;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    public List<Token> triggers;
    public float yOffset = 5f;
    public float timeToOpen = 3f;

    private enum GateState
    {
        Closed, Opening, Open
    }

    private GateState state = GateState.Closed;
    private float openingStartTime;

    private Vector3 closedPos, openPos;

    private void Start()
    {
        closedPos = transform.position;
        openPos = closedPos + yOffset * Vector3.up;
    }

    private void Update()
    {
        switch (state)
        {
            case GateState.Closed:
                bool allActive = true;
                foreach (Token trigger in triggers)
                {
                    allActive &= trigger.isActivated;
                }

                if (allActive)
                {
                    state = GateState.Opening;
                    openingStartTime = Time.time;
                }
                break;
            
            case GateState.Opening:
                float elapsedTime = Time.time - openingStartTime;
                float t = elapsedTime / timeToOpen;
                transform.position = Vector3.Lerp(closedPos, openPos, t);
                if (t >= 1)
                {
                    state = GateState.Open;
                }
                break;
            
            case GateState.Open:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    public void ResetGate()
    {
        transform.position = closedPos;
        state = GateState.Closed;
    }


    // public IEnumerator Open()
    // {
    //     //Debug.Log("OPENED");
    //     isOpen = true;
    //     Vector3 moveTo = new Vector3(transform.position.x, transform.position.y + yOffset, transform.position.z);
    //     float elapsedTime = 0;
    //     float waitTime = openTime;
    //
    //     while (elapsedTime < waitTime)
    //     {
    //         transform.position = Vector3.Lerp(transform.position, moveTo, (elapsedTime / waitTime));
    //         elapsedTime += Time.fixedDeltaTime;
    //         yield return null;
    //     }  
    //     // Make sure we got there
    //     transform.position = moveTo;
    //     yield return null;
    // }
}
