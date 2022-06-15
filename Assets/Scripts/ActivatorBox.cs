using UnityEngine;

public class ActivatorBox : Entity
{
    // ReSharper disable once NotAccessedField.Local
    private new Collider2D collider2D;
    private new Rigidbody2D rigidbody2D;
    private SpriteRenderer spriteRenderer;
    public Sprite offSprite;
    public Sprite onSprite; 
    
    // Start is called before the first frame update
    private void Start()
    {
        collider2D = GetComponent<Collider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = offSprite;
    }

    public void Lock()
    {
        GetComponent<SpriteRenderer>().sprite = onSprite;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    // Update is called once per frame
    // private void Update()
    // {
    //     
    // }
}
