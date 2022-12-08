using UnityEngine;
using System.Collections;

public class HazardTile : MonoBehaviour
{
    private void OnCollisionStay2D(Collision2D col)
    {
        //print("col");
        
        if (col.gameObject.CompareTag("Player"))
        {
            CharController.Instance.Die();
        }
    }
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
    }
}