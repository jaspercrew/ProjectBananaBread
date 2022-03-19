using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private const float HoverRange = 3f;
    public static GrapplePoint targetPoint;
    private SpriteRenderer spriteRenderer;
    private new Camera camera;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        camera = Camera.main;
    }
    
    private void Update()
    {
        Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        float thisDist = Vector2.Distance(mousePos, transform.position);
        float currDist = (targetPoint is null) ? float.MaxValue :
            Vector2.Distance(mousePos, targetPoint.transform.position);
        // Debug.Log(thisDist);
        // Debug.Log(currDist);
        
        if (thisDist < HoverRange && thisDist < currDist)
        {
            Debug.Log("point set");
            targetPoint = this;
        } 
        else if (currDist >= HoverRange)
        {
            targetPoint = null;
        }
    }

}
