using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;

public class TileStateManager : MonoBehaviour
{
    public static TileStateManager Instance;
    public GameObject lineRendererPrefab;
    
    private Transform realGround;
    private Transform altGround;
    private Grid realGrid;
    private Grid altGrid;

    private GameObject realLrParent;
    private GameObject altLrParent;
    private CompositeCollider2D realCc;
    private CompositeCollider2D altCc;

    private HashSet<PlatformEffector2D> platforms;

    public bool platformsActivated = true;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        Grid both = transform.Find("GridMain").GetComponent<Grid>();
        realGrid = transform.Find("GridA").GetComponent<Grid>();
        altGrid = transform.Find("GridB").GetComponent<Grid>();
       // print(transform.Find("GridMain").transform.Find("Main-Platforms").GetComponent<PlatformEffector2D>());
        platforms = new HashSet<PlatformEffector2D>();
        platforms.Add(transform.Find("GridMain").transform.Find("Main-Platforms").GetComponent<PlatformEffector2D>());
        platforms.Add(transform.Find("GridA").transform.Find("A-Platforms").GetComponent<PlatformEffector2D>());
        platforms.Add(transform.Find("GridB").transform.Find("B-Platforms").GetComponent<PlatformEffector2D>());
        
        both.enabled = true;

        realGround = realGrid.transform.Find("A-Ground");
        altGround = altGrid.transform.Find("B-Ground");
        realCc = realGround.gameObject.GetComponent<CompositeCollider2D>();
        altCc = altGround.gameObject.GetComponent<CompositeCollider2D>();

        CalculateAndSpawnOutlines("A", out realLrParent, realCc, realGround);
        CalculateAndSpawnOutlines("B", out altLrParent, altCc, altGround);
        
        ShiftTilesTo(GameManager.Instance.isGameShifted);
    }

    public void DeactivatePlatforms()
    {
        print("deactivate plats");
        Assert.IsTrue(platformsActivated);
        platformsActivated = false;
        foreach (PlatformEffector2D platform in platforms)
        {
            platform.colliderMask = LayerMask.NameToLayer("Player");
        }
        StartCoroutine(DeactivatePlatformCoroutine());
    }

    private IEnumerator DeactivatePlatformCoroutine()
    {
        yield return new WaitForSeconds(1.2f);
        ActivatePlatforms();
    }

    public void ActivatePlatforms()
    {
        print("activate plats");
        Assert.IsTrue(!platformsActivated);
        platformsActivated = true;
        foreach (PlatformEffector2D platform in platforms)
        {
            platform.colliderMask = Physics.AllLayers;
        }
        
    }

    public void ShiftTilesTo(bool isAlt)
    {
        realGrid.enabled = !isAlt;
        realLrParent.SetActive(isAlt);
        altGrid.enabled = isAlt;
        altLrParent.SetActive(!isAlt);
    }

    private void CalculateAndSpawnOutlines(string prefix, out GameObject lrParent, 
        CompositeCollider2D cc, Transform groundParent)
    {
        lrParent = new GameObject(prefix + "-Ground-Outline")
        {
            transform =
            {
                parent = groundParent
            }
        };

        for (int path = 0; path < cc.pathCount; path++)
        {
            GameObject outline = Instantiate(lineRendererPrefab, lrParent.transform);
            outline.name = prefix + "-Ground-Outline-" + path;
            LineRenderer lr = outline.GetComponent<LineRenderer>();
            
            Vector2[] pathPoints2 = new Vector2[cc.GetPathPointCount(path) + 1];
            cc.GetPath(path, pathPoints2);
            pathPoints2[pathPoints2.Length - 1] = pathPoints2[0];
            
            Vector3[] pathPoints3 = new Vector3[pathPoints2.Length];
            for (int i = 0; i < pathPoints2.Length; i++)
            {
                pathPoints3[i] = pathPoints2[i];
            }

            lr.positionCount = pathPoints3.Length;
            lr.SetPositions(pathPoints3);
        }
    }
}
