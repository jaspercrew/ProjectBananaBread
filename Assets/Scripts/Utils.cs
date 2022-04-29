using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class Utils
{
    public static bool LOSCheck(Vector2 a, Vector2 b) //returns true if LOS is clear
    {
        RaycastHit2D hit = Physics2D.Raycast(a, (b-a).normalized,
            Vector2.Distance(a, b), LayerMask.GetMask("Obstacle"));
        return hit.collider == null;
    }
    public static bool LOSCheck(Transform object_A, Transform object_B) //returns true if LOS is clear by taking centers of colliders
    {
        Vector2 a = object_A.GetComponent<Collider2D>().bounds.center;
        Vector2 b = object_B.GetComponent<Collider2D>().bounds.center;
        
        RaycastHit2D hit = Physics2D.Raycast(a, (b-a).normalized,
            Vector2.Distance(a, b), LayerMask.GetMask("Obstacle"));
        return hit.collider == null;
    }

}

