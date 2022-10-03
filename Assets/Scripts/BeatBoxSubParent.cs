using System;
using UnityEngine;

public class BeatBoxSubParent : MonoBehaviour
{
    private BeatBox[] children;
    
    private void Start()
    {
        children = GetComponentsInChildren<BeatBox>();
    }

    private void SetDoChanging(bool doChanging)
    {
        foreach (BeatBox b in children)
        {
            Debug.Log("entered");
            b.doChanging = doChanging;
        }
    }
    
    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            SetDoChanging(false);
        }
    }
    
    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            SetDoChanging(true);
        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("MainCamera"))
        {
            SetDoChanging(true);
        }
    }
}
