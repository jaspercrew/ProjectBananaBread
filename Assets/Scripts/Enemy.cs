using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    // Start is called before the first frame update
    public int maxHealth = 100;
    private int currentHealth;
    private Animator _animator;
    
    
    void Start() {
        currentHealth = maxHealth;
        _animator = transform.GetComponent<Animator>();
    }

    public void TakeDamage(int damage) {
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }

    private void Die() {
        _animator.SetTrigger("Death");
        transform.GetComponent<BoxCollider2D>().enabled = false;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
