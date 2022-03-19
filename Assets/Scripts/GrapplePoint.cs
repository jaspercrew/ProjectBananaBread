using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private const float hoverRange = 3f;
    public static HashSet<GameObject> availablePoints;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }
    
    private void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if ((mousePos - transform.position).magnitude < hoverRange) //in range of mouse
        {
            //do stuff
            
            
            if (!availablePoints.Contains(gameObject))
            {
                availablePoints.Add(gameObject);
            }
        }
        else if (availablePoints.Contains(gameObject))
        {
            availablePoints.Remove(gameObject);
        }
    }

}
