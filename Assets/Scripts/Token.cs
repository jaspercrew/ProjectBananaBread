using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public enum TokenType
{
    Key, TempoUp, TempoDown
}
public class Token : MonoBehaviour
{
    public TokenType type;
    public bool isActivated = false;
    protected SpriteRenderer background;
    // Start is called before the first frame update
    protected void Start()
    {
        background = transform.Find("Background").GetComponent<SpriteRenderer>();

    }

    protected void OnTriggerEnter2D(Collider2D other)
    {
        if (!isActivated && other.gameObject.CompareTag("Player"))
        {
            Consume();
        }
    }

    protected virtual void Consume()
    {
        isActivated = true;
        Color temp = background.color;
        temp.a = .5f;
        background.color = temp;
        switch (type)
        {
            case TokenType.Key:
                break;
            case TokenType.TempoDown:
                break;
            case TokenType.TempoUp:
                break;
        }
    }

    public virtual void ResetToken()
    {
        isActivated = false;
        Color temp = background.color;
        temp.a = 1f;
        background.color = temp;
    }
}
