using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LockSpriteController : MonoBehaviour
{
    private SpriteRenderer spriteRenderer;

    public int prevSceneIndex;
    // Start is called before the first frame update
    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (GameManager.Instance.scenesCompleted.Length < 1)
        {
            SaveData.LoadFromFile(1);
        }
        if (GameManager.Instance.scenesCompleted[prevSceneIndex])
        {
            Color temp = spriteRenderer.color;
            temp.a = 0f;
            spriteRenderer.color = temp;
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
