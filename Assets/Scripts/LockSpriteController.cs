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
        StartCoroutine(LateStart());
    }
    
    private IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        spriteRenderer = GetComponent<SpriteRenderer>();
        // if (GameManager.Instance.levelProgress.Length < 1)
        // {
        //     SaveData.LoadFromFile(1);
        // }
        //print(GameManager.Instance.levelProgress.Length);
        if (prevSceneIndex < GameManager.Instance.levelProgress.Length)
        {
            bool[] checkpointOfPrevious = GameManager.Instance.levelProgress[prevSceneIndex];
            if (checkpointOfPrevious != null && checkpointOfPrevious.Length > 0 &&
                checkpointOfPrevious[checkpointOfPrevious.Length - 1])
            {
                Color temp = spriteRenderer.color;
                temp.a = 0f;
                spriteRenderer.color = temp;
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
