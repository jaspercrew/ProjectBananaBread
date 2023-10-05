/*
using UnityEngine;

public class BouncePad : ActivatedEntity
{
    public float bounceVelocity;
    private const float CharMultiplier = .8f;
    public bool isInverted;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!IsActive)
        {
            return;
        }
        Rigidbody2D rb = other.gameObject.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            //print("force added");
            if (other.gameObject.GetComponent<CharController>() == null)
            {
                rb.velocity = new Vector2(0, isInverted ? -bounceVelocity : bounceVelocity);
            }
            else
            {
                rb.velocity = new Vector2(
                    rb.velocity.x,
                    isInverted
                        ? (-bounceVelocity * CharMultiplier)
                        : (bounceVelocity * CharMultiplier)
                );
            }
        }
    }
}
*/

