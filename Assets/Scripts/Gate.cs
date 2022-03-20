using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gate : MonoBehaviour
{
    private bool isOpen;
    // private bool prevOpened;
    public Lever lever;

    // private Rigidbody2D rigidbody;
    // private BoxCollider2D boxCollider2D;
    
    // Start is called before the first frame update
    private void Start()
    {
        isOpen = false;
        // rigidbody = GetComponent<Rigidbody2D>();
        // boxCollider2D = GetComponent<BoxCollider2D>();
    }

    // Update is called once per frame
    private void Update()
    {
        if (!isOpen && lever.isActive)
        {
            Open();
        }
    }

    public void Open()
    {
        isOpen = true;
        // prevOpened = true;
    }
}
