using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Debug = System.Diagnostics.Debug;

public class GrapplePoint : MonoBehaviour
{
    private const float HoverRange = 3f;
    public static GrapplePoint CurrentGrapplePoint;
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

        float thisDist = Vector3.Distance(mousePos, transform.position);
        float currDist = (CurrentGrapplePoint is null) ? float.MaxValue :
            Vector3.Distance(mousePos, CurrentGrapplePoint.transform.position);
        
        if (thisDist < HoverRange && thisDist < currDist)
        {
            CurrentGrapplePoint = this;
        } 
        else if (currDist >= HoverRange)
        {
            CurrentGrapplePoint = null;
        }
    }

}
