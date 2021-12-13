using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Enemy : LivingThing
{
    // Start is called before the first frame update
    private float speed = 3f;
    private float moveVector = 0f;

    [SerializeField] 
    private int moveState = 0;
    
    
    void Start() {
        currentHealth = maxHealth;
        _animator = transform.GetComponent<Animator>();
    }

    public override void TakeDamage(int damage) {
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }

    public override void Die() {
        _animator.SetTrigger("Death");
        transform.GetComponent<CapsuleCollider2D>().enabled = false;
    }
    

    private bool isMovementEnabled() {
        if (_animator.GetCurrentAnimatorStateInfo(0).IsName("Attack")) {
            return false;
        }
        return true;
    }
    // Update is called once per frame
    void Update()
    {
        transform.position += new Vector3(moveVector * speed * Time.deltaTime, 0, 0);
        
    }
}
