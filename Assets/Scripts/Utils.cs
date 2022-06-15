using UnityEngine;

public static class Utils
{
    // public static bool LOSCheck(Vector2 a, Vector2 b) //returns true if LOS is clear
    // {
    //     RaycastHit2D hit = Physics2D.Raycast(a, (b-a).normalized,
    //         Vector2.Distance(a, b), LayerMask.GetMask("Obstacle"));
    //     return hit.collider == null;
    // }
    public static bool LosCheck(Transform objectA, Transform objectB) //returns true if LOS is clear by taking centers of colliders
    {
        Vector2 a = objectA.GetComponent<Collider2D>().bounds.center;
        Vector2 b = objectB.GetComponent<Collider2D>().bounds.center;
        
        RaycastHit2D hit = Physics2D.Raycast(a, (b-a).normalized,
            Vector2.Distance(a, b), LayerMask.GetMask("Obstacle"));
        return hit.collider == null;
    }
    


}

