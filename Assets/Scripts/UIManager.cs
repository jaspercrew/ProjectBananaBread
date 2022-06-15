using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;
    public List<GameObject> healthList;
    private Transform mainParent;
    private UnityEngine.UI.Image ball;
    private GameObject createdFury;
    private Sprite back;
    private Sprite backFull;
    
    public GameObject healthObject;
    public GameObject furyObject;
    public GameObject healthEnd;

    private const float ParentScale = .7f;
    private const float ParentXOffset = -43.0f;
    private const float ParentYOffset = 4.0f;
    private const float HealthYOffset = -2.0f;
    private const float HealthXOffset = 0.5f;
    private const float HealthXGap = 1.8f;
    private const float FuryXOffset = -2.5f;
    private const float FuryYOffset = -0.1555f;
    // private const float HealthEndXOffset = -0.7f;

    private void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        back = Resources.Load<Sprite>("Sprites/RageBack");
        backFull = Resources.Load<Sprite>("Sprites/RageBackFull");
        mainParent = transform.Find("MainParent");
        healthList = new List<GameObject>();
        StartCoroutine(PopulateHealthBar());
    }

    private bool hasPopulated;

    public void PopulateHealthBarPublic()
    {
        StartCoroutine(PopulateHealthBar());
    }
    
    private IEnumerator PopulateHealthBar()
    {
        yield return new WaitForEndOfFrame();


        if (hasPopulated)
            yield break;
        hasPopulated = true;
        
        healthList.Clear();
        
        createdFury = Instantiate(furyObject, mainParent);
        ball = createdFury.transform.Find("Ball").GetComponent<UnityEngine.UI.Image>();
        createdFury.transform.position = new Vector3(HealthXOffset + FuryXOffset, HealthYOffset + FuryYOffset, 5);
        for (int i = 0; i < CharController.Instance.maxHealth; i++)
        {
            GameObject g = Instantiate(healthObject, mainParent);
            //Debug.Log( new Vector3(healthXOffset + (healthXGap * i), healthYOffset, -15));
            g.transform.position = new Vector3(HealthXOffset + (HealthXGap * i), HealthYOffset, 5);
            healthList.Add(g);
        }

        //GameObject end = Instantiate(healthEnd, mainParent);
        //end.transform.position = new Vector3(healthXOffset + healthEndXOffset + (healthXGap * CharController.Instance.MaxHealth), healthYOffset, 5);
        mainParent.transform.localScale = new Vector3(mainParent.transform.localScale.x * ParentScale, mainParent.transform.localScale.y * ParentScale, 1);
        mainParent.transform.position = new Vector3(mainParent.transform.position.x + ParentXOffset, mainParent.transform.position.y + ParentYOffset, 5);

        CheckHealth();
        CheckFury();
    }

    public void CheckHealth()
    {
        for (int i = 0; i < CharController.Instance.maxHealth; i++)
        {
            if (i < CharController.Instance.currentHealth)
            {
                healthList[i].GetComponent<HealthUnit>().Fill();
            }
            else
            {
                healthList[i].GetComponent<HealthUnit>().Empty();
            }
        }
    }

    public void CheckFury()
    {
        if (Math.Abs(CharController.Instance.fury - CharController.MaxFury) < float.Epsilon)
        {
            createdFury.transform.Find("Back").GetComponent<SpriteRenderer>().sprite = backFull;
        }

        else
        {
            createdFury.transform.Find("Back").GetComponent<SpriteRenderer>().sprite = back;
        }
        ball.fillAmount = (CharController.Instance.fury / CharController.MaxFury);
        //Debug.Log(CharController.Instance.fury);
        //Debug.Log(ball.fillAmount);

    }

    // Update is called once per frame
    // void Update()
    // {
    //     
    // }
}
