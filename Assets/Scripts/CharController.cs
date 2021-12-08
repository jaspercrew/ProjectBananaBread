using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CharController : MonoBehaviour {
    private Rigidbody2D _rigidbody;
    private float speed = 3.5f;
    private float jumpForce = 5f;
    private bool movementEnabled = true;
    
    // Start is called before the first frame update
    void Start() {
        _rigidbody = transform.GetComponent<Rigidbody2D>();

    }

    // Update is called once per frame
    void FixedUpdate() {

        if (movementEnabled) {
            float move = Input.GetAxisRaw("Horizontal");
            transform.position += new Vector3(move * speed * Time.deltaTime, 0, 0);
            //Vector3 moveVect = new Vector3(Input.GetAxis("Horizontal"), 0, 0);
            //moveVect = moveVect.normalized * (speed * Time.deltaTime);
            //_rigidbody.MovePosition(transform.position + moveVect);
        }
        
    }

    void Update() {
        if (Input.GetButtonDown("Jump") && IsGrounded() && movementEnabled) {
            _rigidbody.AddForce(new Vector2(0, jumpForce), ForceMode2D.Impulse);
            //Debug.Log("fds");
        }

        //shortjump
        if (Input.GetButtonUp("Jump") && !IsGrounded()) {
            _rigidbody.velocity = Vector2.Scale(_rigidbody.velocity, new Vector2(1f, 0.5f));
        }
    }

    private bool IsGrounded() {
        return true;
    }
}
