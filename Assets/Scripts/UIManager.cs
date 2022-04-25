using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class UIManager : MonoBehaviour
{

    public static UIManager Instance;
    public static List<GameObject> HealthList;
    private Transform mainParent;
    public GameObject healthObject;
    private const float healthYOffset = -2.5f;
    private const float healthXOffset = -1.75f;
    private const float healthXGap = 1f;

    private void Awake()
    {
        Instance = this;
    }
    
    // Start is called before the first frame update
    void Start()
    {
        mainParent = transform.Find("MainParent");
        HealthList = new List<GameObject>();
        // for (int i = 0; i < CharController.MaxHealth; i++)
        // {
        //     GameObject g = Instantiate(healthObject, mainParent);
        //     //Debug.Log( new Vector3(healthXOffset + (healthXGap * i), healthYOffset, -15));
        //     g.transform.position = new Vector3(healthXOffset + (healthXGap * i), healthYOffset, -15);
        //     HealthList.Add(g);
        // }
        
        StartCoroutine(LateStart());
    }
    IEnumerator LateStart()
    {
        yield return new WaitForEndOfFrame();
        for (int i = 0; i < CharController.MaxHealth; i++)
        {
            GameObject g = Instantiate(healthObject, mainParent);
            //Debug.Log( new Vector3(healthXOffset + (healthXGap * i), healthYOffset, -15));
            g.transform.position = new Vector3(healthXOffset + (healthXGap * i), healthYOffset, 5);
            HealthList.Add(g);
        }
        CheckHealth();
    }

    public void CheckHealth()
    {
        for (int i = 0; i < CharController.MaxHealth; i++)
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

    // Update is called once per frame
    void Update()
    {
        
    }
}
