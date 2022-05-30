using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private const float HoverRange = 1.5f;
    public static GrapplePoint targetPoint;
    private SpriteRenderer spriteRenderer;
    private new Camera camera;
    private SpriteRenderer LoopFX;
    private const float rotateSpeed = 150f;

    public Color clearColor;
    public Color blockedColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        camera = Camera.main;
        LoopFX = transform.Find("LoopFX").GetComponent<SpriteRenderer>();
        LoopFX.enabled = false;
    }
    
    private void Update()
    {
        Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        float thisDist = Vector2.Distance(mousePos, transform.position);
        float currDist = (targetPoint == null) ? float.MaxValue :
            Vector2.Distance(mousePos, targetPoint.transform.position);
        // Debug.Log(thisDist);
        // Debug.Log(currDist);
        if (targetPoint == this)
        {
            LoopFX.enabled = true;
            LoopFX.gameObject.transform.Rotate(Vector3.forward, rotateSpeed * Time.deltaTime);
        }
        else
        {
            LoopFX.enabled = false;
        }
        
        if (thisDist < HoverRange && thisDist < currDist)
        {
            //Debug.Log("point set");
            targetPoint = this;
            
        } 
        else if (currDist >= HoverRange)
        {
            targetPoint = null;
            
        }
    }

    public void Blocked()
    {
        LoopFX.color = blockedColor;
    }

    public void Cleared()
    {
        LoopFX.color = clearColor;
    }

}
