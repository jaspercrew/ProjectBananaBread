using System;
using UnityEditor;
using UnityEngine;

public class BladeProjectile : Projectile
{
    //private float outSpeed = 3f;
    private float inSpeed = 35f;
    //private Vector3 dir;

    public bool isStuck;
    private bool isReturning;
    private GameObject stuckTo;
    private void Awake()
    {
        Collider2D = transform.GetComponent<BoxCollider2D>();
        Rigidbody2D = transform.GetComponent<Rigidbody2D>();
        //Rigidbody2D.velocity = outSpeed * dir;
        isStuck = false;
        isReturning = false;
    }

    private void Update()
    {
        if (isReturning)
        {
            Rigidbody2D.velocity = inSpeed * (CharController.Instance.transform.position - transform.position).normalized;
        }
    }

    public void Initialize(Vector3 dir, float speed)
    {
        //Rigidbody2D.isKinematic = false;
        //this.dir = dir.normalized;
        //outSpeed = speed;
        transform.eulerAngles = new Vector3(0, 0, Mathf.Rad2Deg * (Mathf.Atan2(dir.y, dir.x)));
        Rigidbody2D.velocity = speed * dir.normalized;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
         if (other.gameObject.GetComponent<CharController>() != null && isReturning)
        {
            isReturning = false;
            CharController.Instance.ReturnCast();
            Destroy(gameObject);
        }
        else if (isReturning || other.gameObject.GetComponent<CharController>() != null)
        {
            return;
        }
        else if (other.gameObject.GetComponent<Platform>() != null || 
            other.gameObject.GetComponent<Entity>() != null || 
            other.gameObject.GetComponent<Enemy>() != null)
        {
            Stick(other.gameObject);
            Rigidbody2D.velocity = Vector2.zero;
        }
        
    }

    private void Stick(GameObject obj)
    {
        Debug.Log("stick");
        transform.SetParent(obj.transform);
        isStuck = true;
        stuckTo = obj;
        Rigidbody2D.velocity = Vector2.zero;
        if (obj.GetComponent<Enemy>() != null)
        {
            obj.GetComponent<Enemy>().TakeDamage(1);
        }
        
    }

    public void Yoink()
    {
        Enemy enemyStuck = stuckTo.GetComponent<Enemy>();
        Platform platformStuck = stuckTo.GetComponent<Platform>();
        Entity entityStuck = stuckTo.GetComponent<Entity>();
        if (!isStuck)
        {
            return;
        }
        Debug.Log("yoink from" + stuckTo.name);
        transform.SetParent(null);

        if (platformStuck != null)
        {
            
        }
        else if (enemyStuck != null)
        {
            enemyStuck.TakeDamage(10);
            enemyStuck.Yoink(5f);
            
        }
        else if (entityStuck != null)
        {
            if (stuckTo.GetComponent<Rigidbody2D>() != null)
            {
                entityStuck.Yoink(5f);
            }
        }

        isReturning = true;
        //Rigidbody2D.isKinematic = false;
        isStuck = false;
        stuckTo = null;

    }
}