using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private const float HoverRange = 1.5f;
    public static GrapplePoint TargetPoint;
    // ReSharper disable once NotAccessedField.Local
    private SpriteRenderer spriteRenderer;
    private new Camera camera;
    private SpriteRenderer loopFX;
    private const float RotateSpeed = 150f;

    public Color clearColor;
    public Color blockedColor;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        camera = Camera.main;
        loopFX = transform.Find("LoopFX").GetComponent<SpriteRenderer>();
        loopFX.enabled = false;
    }
    
    private void Update()
    {
        Vector3 mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        float thisDist = Vector2.Distance(mousePos, transform.position);
        float currDist = (TargetPoint == null) ? float.MaxValue :
            Vector2.Distance(mousePos, TargetPoint.transform.position);
        // Debug.Log(thisDist);
        // Debug.Log(currDist);
        if (TargetPoint == this)
        {
            loopFX.enabled = true;
            loopFX.gameObject.transform.Rotate(Vector3.forward, RotateSpeed * Time.deltaTime);
        }
        else
        {
            loopFX.enabled = false;
        }
        
        if (thisDist < HoverRange && thisDist < currDist)
        {
            //Debug.Log("point set");
            TargetPoint = this;
            
        } 
        else if (currDist >= HoverRange)
        {
            TargetPoint = null;
            
        }
    }

    public void Blocked()
    {
        loopFX.color = blockedColor;
    }

    public void Cleared()
    {
        loopFX.color = clearColor;
    }

}
