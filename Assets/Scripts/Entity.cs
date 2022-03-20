using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour {
    public virtual void Shift()
    {
        
    }
    
    public virtual void Yoink(float yoinkForce)
    {
        if (GetComponent<Rigidbody2D>() == null)
        {
            return;
        }
        GetComponent<Rigidbody2D>().AddForce(yoinkForce * (CharController.Instance.transform.position - transform.position).normalized, ForceMode2D.Impulse);
    }
}
