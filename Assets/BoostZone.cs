using UnityEngine;

public class BoostZone : MonoBehaviour
{
    private bool consumed;
    private SpriteRenderer spriteRenderer;

    public void Reset()
    {
        consumed = false;
    }

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        CharController.instance.currentBoostZone = null;
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        CharController.instance.currentBoostZone = this;
    }

    public void Consume()
    {
        consumed = true;
    }
}