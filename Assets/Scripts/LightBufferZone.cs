using UnityEngine;

public class LightBufferZone : BinaryEntity
{
    public float radius;

    public bool activeInReal;
    public bool activeInAlt;
    private bool isActive;

    // ReSharper disable once NotAccessedField.Local
    private new Collider2D collider2D;

    protected override void Start()
    {
        base.Start();
        collider2D = transform.GetComponent<Collider2D>();
    }

    protected override void TurnShifted()
    {
        base.TurnShifted();
        if (activeInAlt)
        {
            isActive = true;
            
        }
    }

    protected override void TurnUnshifted()
    {
        base.TurnUnshifted();
        if (activeInReal)
        {
            isActive = true;
        }
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && isActive)
        {
            CharController.Instance.lightBuffer = CharController.MaxLightBuffer;
        }
    }
}