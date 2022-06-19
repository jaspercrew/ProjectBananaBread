using UnityEngine;

public class ActivatorBox : Entity, IHittableEntity
{
    // ReSharper disable once NotAccessedField.Local
    private new Collider2D collider2D;
    private new Rigidbody2D rigidbody2D;
    private SpriteRenderer spriteRenderer;
    private const float hitVelocity = 13f;
    private bool beingHit;
    public Sprite offSprite;
    public Sprite onSprite; 
    
    // Start is called before the first frame update
    private void Start()
    {
        collider2D = GetComponent<Collider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        spriteRenderer.sprite = offSprite;
        rigidbody2D.mass = 10000;
    }

    public void Lock()
    {
        GetComponent<SpriteRenderer>().sprite = onSprite;
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    public void GetHit(int damage)
    {
        rigidbody2D.mass = 1;
        beingHit = true;
        rigidbody2D.velocity =
            (CharController.position.x > transform.position.x ? Vector2.left : Vector2.right) *
            hitVelocity;
        
    }

    // Update is called once per frame
    private void Update()
    {
        if (beingHit && rigidbody2D.velocity.magnitude < .1f)
        {
            beingHit = false;
            rigidbody2D.mass = 10000;
        }

    }
}
