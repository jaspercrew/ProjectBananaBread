using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public bool Invertable = true;
    public virtual void Shift()
    {
        if (SceneInformation.Instance.isGravityScene && Invertable && GetComponent<Rigidbody2D>() != null)
        {
            GetComponent<Rigidbody2D>().gravityScale *= -1;
        }
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
