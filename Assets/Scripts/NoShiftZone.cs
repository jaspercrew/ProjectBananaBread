using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NoShiftZone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() != null)
        {
            CharController.Instance.noShiftZone = true;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() != null)
        {
            CharController.Instance.noShiftZone = false;
        }
    }
}
