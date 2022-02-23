using System;
using UnityEditor;
using UnityEngine;

public class BladeProjectile : Projectile
{
    private float outSpeed = 5f;
    private float inSpeed = 10f;
    private Vector3 dir;

    public bool isStuck;
    private bool isReturning;
    private GameObject stuckTo;
    private void Start()
    {
        Collider2D = GetComponent<BoxCollider2D>();
        Rigidbody2D = GetComponent<Rigidbody2D>();
        Rigidbody2D.velocity = outSpeed * dir;
        isStuck = false;
        isReturning = false;
    }

    public void Initialize(Vector3 dir, float speed)
    {
        Rigidbody2D.isKinematic = false;
        this.dir = dir.normalized;
        outSpeed = speed;
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        if (isReturning)
        {
            return;
        }
        if (other.gameObject.GetComponent<Platform>() != null || 
            other.gameObject.GetComponent<Entity>() != null || 
            other.gameObject.GetComponent<Enemy>() != null)
        {
            Stick(other.gameObject);
        }

        else if (other.gameObject.GetComponent<CharController>() != null && isReturning)
        {
            isReturning = false;
            Destroy(this);

        }
    }

    private void Stick(GameObject obj)
    {
        Rigidbody2D.velocity = Vector2.zero;
        transform.SetParent(obj.transform);
        isStuck = true;
        Rigidbody2D.isKinematic = true;
        stuckTo = obj;
    }

    public void Yoink()
    {
        if (!isStuck)
        {
            return;
        }

        if (stuckTo.GetComponent<Platform>() != null)
        {
            
        }
        else if (stuckTo.GetComponent<Enemy>() != null)
        {
            stuckTo.GetComponent<Enemy>().TakeDamage(10);
            stuckTo.GetComponent<Enemy>().Yoink(5f);
            
        }
        else if (stuckTo.GetComponent<Entity>() != null)
        {
            if (stuckTo.GetComponent<Rigidbody2D>() != null)
            {
                stuckTo.GetComponent<Entity>().Yoink(5f);
            }
        }

        isReturning = true;
        Rigidbody2D.isKinematic = false;
        isStuck = false;
        Rigidbody2D.velocity = inSpeed * (CharController.Instance.transform.position - transform.position);
        stuckTo = null;

    }


}