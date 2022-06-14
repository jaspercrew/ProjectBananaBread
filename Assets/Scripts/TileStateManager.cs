using UnityEngine;

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
        
        both.enabled = true;

        realGround = realGrid.transform.Find("A-Ground");
        altGround = altGrid.transform.Find("B-Ground");
        realCc = realGround.gameObject.GetComponent<CompositeCollider2D>();
        altCc = altGround.gameObject.GetComponent<CompositeCollider2D>();

        CalculateAndSpawnOutlines("A", out realLrParent, realCc, realGround);
        CalculateAndSpawnOutlines("B", out altLrParent, altCc, altGround);
        
        ShiftTilesTo(GameManager.Instance.isGameShifted);
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
