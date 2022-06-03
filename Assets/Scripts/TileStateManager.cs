using UnityEngine;
using UnityEngine.Tilemaps;

public class TileStateManager : MonoBehaviour
{
    public static TileStateManager Instance;
    
    // need both renderers to switch graphics and colliders to switch collisions
    // private TilemapRenderer bothLayersRenderer;
    // private TilemapRenderer realLayerRenderer;
    // private TilemapRenderer altLayerRenderer;
    // private TilemapCollider2D bothLayersCollider;
    // private TilemapCollider2D realLayerCollider;
    // private TilemapCollider2D altLayerCollider;
    private Grid real;
    private Grid alt;

    private TileStateManager()
    {
        if (Instance == null)
            Instance = this;
    }
    
    // Start is called before the first frame update
    private void Start()
    {
        Grid both = transform.Find("GridMain").GetComponent<Grid>();
        real = transform.Find("GridA").GetComponent<Grid>();
        alt = transform.Find("GridB").GetComponent<Grid>();
        
        // Transform both = transform.Find("BothLayer");
        // Transform real = transform.Find("RealLayer");
        // Transform alt = transform.Find("AltLayer");
        //
        // bothLayersRenderer = both.GetComponent<TilemapRenderer>();
        // realLayerRenderer = real.GetComponent<TilemapRenderer>();
        // altLayerRenderer = alt.GetComponent<TilemapRenderer>();
        //
        // bothLayersCollider = both.GetComponent<TilemapCollider2D>();
        // realLayerCollider = real.GetComponent<TilemapCollider2D>();
        // altLayerCollider = alt.GetComponent<TilemapCollider2D>();
        //
        // bothLayersRenderer.enabled = true;
        // bothLayersCollider.enabled = true;
        both.enabled = true;
        
        ShiftTilesTo(GameManager.Instance.isGameShifted);
    }

    public void ShiftTilesTo(bool isAlt)
    {
        Debug.Log("shifttiles");
        // realLayerRenderer.enabled = !isAlt;
        // realLayerCollider.enabled = !isAlt;
        // altLayerRenderer.enabled = isAlt;
        // altLayerCollider.enabled = isAlt;
        real.enabled = !isAlt;
        alt.enabled = isAlt;

    }
}
