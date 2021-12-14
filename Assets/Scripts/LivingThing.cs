using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingThing : MonoBehaviour {
    protected int maxHealth = 100;
    protected int currentHealth;
    protected Animator _animator;

    public virtual void TakeDamage(int damage) {
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }

    public virtual void Die() {
        _animator.SetTrigger("Death");
    }
    

}
