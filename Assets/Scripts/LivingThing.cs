using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class LivingThing : MonoBehaviour {
    protected int maxHealth = 100;
    protected int currentHealth;
    protected Animator _animator;
    protected Rigidbody2D _rigidbody;

    public virtual void TakeDamage(int damage) {
        currentHealth -= damage;
        //damage animation
        _animator.SetTrigger("Hurt");
        
        if (currentHealth <= 0) {
            Die();
        }
    }
    
    protected void VelocityDash(int cardinalDirection, float dashSpeed) {
        switch (cardinalDirection) {
            case 0:
                _rigidbody.velocity = new Vector2(0, dashSpeed);
                break;
            case 1:
                _rigidbody.velocity = new Vector2(dashSpeed, _rigidbody.velocity.y);
                break;
            case 2:
                _rigidbody.velocity = new Vector2(0, -dashSpeed);
                break;
            case 3:
                _rigidbody.velocity = new Vector2(-dashSpeed, _rigidbody.velocity.y);
                break;
            default:
                Debug.Log("invalid dash direction");
                break;
        }
    }
    protected virtual void OnLanding() {
        _animator.SetBool("Grounded", true);
        //Debug.Log("sus");
    }
    
    protected IEnumerator PauseAnimatorCoroutine(float pauseTime) {
        float temp = _animator.speed;
        _animator.speed = 0;
        yield return new WaitForSeconds(pauseTime);
        _animator.speed = temp;
    }
    
    protected void KnockAwayFromPoint(float vel, Vector3 point) {
        Debug.Log(vel);
        //GameObject player = GameObject.FindWithTag("Player");
        _rigidbody.velocity = vel * (transform.position - point).normalized;
    }
    
    

    protected virtual void Die() {
        _animator.SetTrigger("Death");
        transform.GetComponent<Collider>().enabled = false;
        _rigidbody.gravityScale = 0;
    }
    

}
