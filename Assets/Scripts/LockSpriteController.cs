using System.Collections;
using UnityEngine;

public class LockSpriteController : MonoBehaviour
{
    public int prevSceneIndex;
    private SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    private void Start()
    {
        StartCoroutine(LateStart());
    }

    // Update is called once per frame
    private void Update()
    {
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
        if (
            prevSceneIndex < GameManager.instance.levelProgress.Length
            && GameManager.instance.levelProgress[prevSceneIndex]
            == SaveData.levelLengths[prevSceneIndex] - 1
        )
        {
            var temp = spriteRenderer.color;
            temp.a = 0f;
            spriteRenderer.color = temp;
        }
    }
}