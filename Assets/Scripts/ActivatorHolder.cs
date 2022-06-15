using UnityEngine;

public class ActivatorHolder : ActivatorTrigger
{
    private SpriteRenderer spriteRenderer;
    // ReSharper disable once NotAccessedField.Local
    private new Collider2D collider2D;
    public float yLockOffset = .5f;
    public Sprite offSprite;
    public Sprite onSprite;
    
    // Start is called before the first frame update
    private void Start()
    {
        collider2D = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = offSprite;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        ActivatorBox activatorBox = other.gameObject.GetComponent<ActivatorBox>();
        if (activatorBox != null)
        {
            spriteRenderer.sprite = onSprite;
            Activate();
            activatorBox.Lock();
            other.transform.position = new Vector3(transform.position.x, transform.position.y + yLockOffset, transform.position.z);
        }
    }
    
}
