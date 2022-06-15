using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Lever : ActivatorTrigger, IHittableEntity
{
    public string leverName;
    private Dictionary<string, bool> leverDict;
    private SpriteRenderer sr;
    
    public Sprite offSprite;
    public Sprite onSprite;

    public override void Activate()
    {
        base.Activate();
        Debug.Log("lever activated");
        sr.sprite = onSprite;
        //leverDict[leverName] = true; TODO: fix error here (savefile failure)
    }

    private void Start()
    {
        StartCoroutine(CoStart());
    }

    private IEnumerator CoStart()
    {
        // // TODO: this is stinky
        // Sprite[] leverSprites = Resources.LoadAll<Sprite>("Sprites/lever");
        // offSprite = leverSprites[0];
        // onSprite = leverSprites[1];
        sr = GetComponent<SpriteRenderer>();

        yield return new WaitUntil(() => GameManager.Instance.isReady);
        leverDict = GameManager.Instance.LeverDict;
        
        if (!leverDict.ContainsKey(leverName))
        {
            Debug.Log("lever not detected");
            leverDict.Add(leverName, false);
        }
        
        isActivated = leverDict[leverName];
        sr.sprite = isActivated? onSprite : offSprite;
    }

    public void GetHit(int damage)
    {
        if (!isActivated)
        {
            Activate();
        }
    }

    
}
