using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoostZone : MonoBehaviour
{
    private bool consumed;
    private SpriteRenderer spriteRenderer;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public void Consume()
    {
        consumed = true;
    }

    public void Reset()
    {
        consumed = false;
    }

    void OnTriggerStay2D(Collider2D other)
    {
        CharController.Instance.currentBoostZone = this;
    }

    void OnTriggerExit2D(Collider2D other)
    {
        CharController.Instance.currentBoostZone = null;
    }
}
