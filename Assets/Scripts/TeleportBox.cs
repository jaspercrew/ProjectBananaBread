using UnityEngine;

public class TeleportBox : MonoBehaviour
{
    // private BoxCollider2D boxCollider;

    private Vector3 point;
    // Start is called before the first frame update
    private void Start()
    {
        // boxCollider = GetComponent<BoxCollider2D>();
        point = transform.Find("TPPoint").transform.position;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            CharController.Instance.transform.position = point;
        }
    }
}
