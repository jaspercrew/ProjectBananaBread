using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public static List<GameObject> HealthList;
    private Transform mainParent;
    private UnityEngine.UI.Image ball;
    private GameObject createdFury;
    private Sprite back;
    private Sprite backFull;
    
    public GameObject healthObject;
    public GameObject furyObject;
    public GameObject healthEnd;

    private const float parentScale = .7f;
    private const float parentXOffset = -46.75f;
    private const float parentYOffset = 4.5f;
    private const float healthYOffset = -2.5f;
    private const float healthXOffset = 0.5f;
    private const float healthXGap = 1.25f;
    private const float furyXOffset = -1.3f;
    private const float furyYOffset = -0.1555f;
    private const float healthEndXOffset = -0.7f;

    private void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        back = Resources.Load<Sprite>("Sprites/RageBack");
        backFull = Resources.Load<Sprite>("Sprites/RageBackFull");
        mainParent = transform.Find("MainParent");
        HealthList = new List<GameObject>();
        StartCoroutine(LateStart());
    }
    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        
        createdFury = Instantiate(furyObject, mainParent);
        ball = createdFury.transform.Find("Ball").GetComponent<UnityEngine.UI.Image>();
        createdFury.transform.position = new Vector3(healthXOffset + furyXOffset, healthYOffset + furyYOffset, 5);
        for (int i = 0; i < CharController.Instance.MaxHealth; i++)
        {
            GameObject g = Instantiate(healthObject, mainParent);
            //Debug.Log( new Vector3(healthXOffset + (healthXGap * i), healthYOffset, -15));
            g.transform.position = new Vector3(healthXOffset + (healthXGap * i), healthYOffset, 5);
            HealthList.Add(g);
        }

        GameObject end = Instantiate(healthEnd, mainParent);
        end.transform.position = new Vector3(healthXOffset + healthEndXOffset + (healthXGap * CharController.Instance.MaxHealth), healthYOffset, 5);
        mainParent.transform.localScale = new Vector3(mainParent.transform.localScale.x * parentScale, mainParent.transform.localScale.y * parentScale, 1);
        mainParent.transform.position = new Vector3(mainParent.transform.position.x + parentXOffset, mainParent.transform.position.y + parentYOffset, 5);

        CheckHealth();
        CheckFury();
    }

    public void CheckHealth()
    {
        for (int i = 0; i < CharController.Instance.MaxHealth; i++)
        {
            if (i < CharController.Instance.CurrentHealth)
            {
                HealthList[i].GetComponent<HealthUnit>().Fill();
            }
            else
            {
                HealthList[i].GetComponent<HealthUnit>().Empty();
            }
        }
    }

    public void CheckFury()
    {
        if (CharController.Instance.fury == CharController.MaxFury)
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
    void Update()
    {
        
    }
}
