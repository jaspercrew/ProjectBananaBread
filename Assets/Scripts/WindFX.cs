using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.WSA;

public enum WindDirection{
    Left, Right, Up, Down
}
public class WindFX : MonoBehaviour
{
    public WindDirection direction;
    // Update is called once per frame

    void Update()
    {
        if (direction == WindDirection.Left || direction == WindDirection.Right)
        {
            transform.position = new Vector3(transform.position.x, CharController.Instance.transform.position.y, transform.position.z);
        }
        else if (direction == WindDirection.Up || direction == WindDirection.Down)
        {
            transform.position = new Vector3(CharController.Instance.transform.position.x, transform.position.y, transform.position.z);
        }
        
    }
}
