using UnityEngine;

public class GrapplePoint : MonoBehaviour
{
    private const float HoverRange = 1.5f;
    private const float RotateSpeed = 150f;
    public static GrapplePoint targetPoint;

    public Color clearColor;
    public Color blockedColor;
    private new Camera camera;
    private SpriteRenderer loopFX;

    // ReSharper disable once NotAccessedField.Local
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        camera = Camera.main;
        loopFX = transform.Find("LoopFX").GetComponent<SpriteRenderer>();
        loopFX.enabled = false;
    }

    private void Update()
    {
        var mousePos = camera.ScreenToWorldPoint(Input.mousePosition);

        var thisDist = Vector2.Distance(mousePos, transform.position);
        var currDist =
            targetPoint == null
                ? float.MaxValue
                : Vector2.Distance(mousePos, targetPoint.transform.position);
        // Debug.Log(thisDist);
        // Debug.Log(currDist);
        if (targetPoint == this)
        {
            loopFX.enabled = true;
            loopFX.gameObject.transform.Rotate(Vector3.forward, RotateSpeed * Time.deltaTime);
        }
        else
        {
            loopFX.enabled = false;
        }

        if (thisDist < HoverRange && thisDist < currDist)
            //Debug.Log("point set");
            targetPoint = this;
        else if (currDist >= HoverRange) targetPoint = null;
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