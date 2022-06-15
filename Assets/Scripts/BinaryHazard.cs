using UnityEngine;

public enum HazardBehavior
{
    Teleport, Damage
}

public class BinaryHazard : BinaryEntity
{
    
    private bool isActive;
    public bool activeInReal;
    public bool activeInAlt;
    public HazardBehavior behavior;
    public int damage;
    public Transform destination;
    private SpriteRenderer spriteRenderer;
    // Start is called before the first frame update
    protected override void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        base.Start();
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (activeInAlt)
        {
            isActive = true;
            spriteRenderer.enabled = true;
        }
        if (!activeInAlt)
        {
            isActive = false;
            spriteRenderer.enabled = false;
        }
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (activeInReal)
        {
            isActive = true;
            spriteRenderer.enabled = true;
        }
        if (!activeInReal)
        {
            isActive = false;
            spriteRenderer.enabled = false;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.GetComponent<CharController>() == null || !isActive)
        {
            return;
        }
        
        if (behavior == HazardBehavior.Damage)
        {
            CharController.Instance.TakeDamage(damage);
        }
        else if (behavior == HazardBehavior.Teleport)
        {
            CharController.Instance.transform.position = destination.position;
        }
    }
    
}
