using UnityEngine;

public class HazardTile : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void OnCollisionStay2D(Collision2D col)
    {
        //print("col");

        if (col.gameObject.CompareTag("Player")) CharController.instance.Die();
    }
}