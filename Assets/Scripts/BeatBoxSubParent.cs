using System;
using UnityEngine;

public class BeatBoxSubParent : MonoBehaviour
{
    private BeatBox[] children;
    
    private void Start()
    {
        children = GetComponentsInChildren<BeatBox>();
        SetDoChanging(true);
        // Debug.Log("found " + children.Length + " children");
    }

    private void SetDoChanging(bool doChanging)
    {
        // Debug.Log("setting all children to " + doChanging);
        if (children == null) return;
        foreach (BeatBox b in children)
        {
            if (b != null)
            {
                b.doChanging = doChanging;
            }
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
        // string s;
        if (other.gameObject.CompareTag("MainCamera"))
        {
            // s = "entered, good";
            SetDoChanging(true);
        }
        // else
        // {
        //     s = "entered but NOT camera";
        // }
        // Debug.Log(s);
    }
}
