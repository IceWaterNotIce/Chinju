using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;

public class VisibilityManager : MonoBehaviour
{
    [Header("References")]
    public Tilemap tilemap;
    public TileBase chinjuTile;
    public Material maskMaterial; // 用於遮罩的材質（需支援遮罩效果）

    [Header("Mask Settings")]
    public float chinjuRadius = 10f;

    private List<Ship> ships = new List<Ship>();
    private List<Vector3> visibleCenters = new List<Vector3>();

    private Camera mainCamera;
    private Mesh maskMesh;
    private MeshRenderer maskRenderer;

    void Start()
    {
        mainCamera = Camera.main;
        if (tilemap == null)
            tilemap = FindFirstObjectByType<Tilemap>();
        if (chinjuTile == null)
            Debug.LogError("Chinju Tile 未設置！");
        UpdateShipList();
        CreateMaskMesh();
    }

    void Update()
    {
        UpdateShipList();
        UpdateVisibleCenters();
        UpdateMaskMesh();
    }

    void UpdateShipList()
    {
        ships.Clear();
        var allShips = GameObject.FindObjectsByType<Ship>(FindObjectsSortMode.None);
        foreach (var ship in allShips)
        {
            // 假設敵方船艦有特殊標記，這裡只加入非敵方船
            if (!ship.CombatMode)
                ships.Add(ship);
        }
    }

    void UpdateVisibleCenters()
    {
        visibleCenters.Clear();
        // Chinju Tile
        Vector3 chinjuWorldPos = Vector3.zero;
        BoundsInt bounds = tilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (tilemap.GetTile(pos) == chinjuTile)
            {
                chinjuWorldPos = tilemap.GetCellCenterWorld(pos);
                break;
            }
        }
        if (chinjuWorldPos != Vector3.zero)
            visibleCenters.Add(chinjuWorldPos);

        // 所有非敵方船艦
        foreach (var ship in ships)
        {
            visibleCenters.Add(ship.transform.position);
        }
    }

    void CreateMaskMesh()
    {
        if (maskMesh == null)
        {
            maskMesh = new Mesh();
        }
        if (maskRenderer == null)
        {
            maskRenderer = gameObject.AddComponent<MeshRenderer>();
            maskRenderer.material = maskMaterial;
        }
        var meshFilter = gameObject.GetComponent<MeshFilter>();
        if (meshFilter == null)
        {
            meshFilter = gameObject.AddComponent<MeshFilter>();
        }
        meshFilter.mesh = maskMesh;
    }

    void UpdateMaskMesh()
    {
        // 防呆：必要物件未設置時不執行
        if (maskMesh == null || maskMaterial == null)
            return;
        if (visibleCenters.Count == 0)
            return;

        // 產生一個 100x100 的大矩形，中心設置為 (50, 50, -1)
        float width = 100f;
        float height = 100f;
        Vector3 center = new Vector3(50f, 50f, -1f);
        Vector3[] verts = new Vector3[4]
        {
            new Vector3(center.x - width/2f, center.y - height/2f, center.z),
            new Vector3(center.x + width/2f, center.y - height/2f, center.z),
            new Vector3(center.x + width/2f, center.y + height/2f, center.z),
            new Vector3(center.x - width/2f, center.y + height/2f, center.z)
        };
        int[] tris = new int[] { 0, 2, 1, 0, 3, 2 }; // 調整三角形順序，使法線面向 -Z
        maskMesh.Clear();
        maskMesh.vertices = verts;
        maskMesh.triangles = tris;
        maskMesh.RecalculateNormals(); // 確保法線正確計算

        // 將可見圓心與半徑傳給材質
        Vector4[] circles = new Vector4[8]; // 最多8個圓
        int count = Mathf.Min(visibleCenters.Count, 8);
        for (int i = 0; i < count; i++)
        {
            float radius = chinjuRadius;
            if (i > 0 && ships.Count >= i)
                radius = ships[i-1].VisibleRadius;
            circles[i] = new Vector4(visibleCenters[i].x, visibleCenters[i].y, radius, 0);
        }
        // 臨時修正：只傳 1 個元素，避免 Unity 報錯
        maskMaterial.SetInt("_CircleCount", count);
        maskMaterial.SetVectorArray("_Circles", circles.Length > 1 ? new Vector4[] { circles[0] } : circles);
    }
}
