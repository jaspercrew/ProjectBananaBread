using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActivatorBox : Entity
{
    private Collider2D collider2D;
    private Rigidbody2D rigidbody2D;
    
    // Start is called before the first frame update
    void Start()
    {
        collider2D = GetComponent<Collider2D>();
        rigidbody2D = GetComponent<Rigidbody2D>();
    }

    public void Lock()
    {
        rigidbody2D.constraints = RigidbodyConstraints2D.FreezeAll;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
