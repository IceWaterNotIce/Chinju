using UnityEngine;
using UnityEngine.Tilemaps;

public class MapManager : MonoBehaviour
{
    [SerializeField] private Tilemap tilemap;
    public TileBase chinjuTile;

    public void SaveMapData()
    {
        var mapData = GameDataController.Instance?.CurrentGameData?.mapData;
        if (mapData != null)
        {
            mapData.ChinjuTiles.Clear();
            foreach (var position in tilemap.cellBounds.allPositionsWithin)
            {
                if (tilemap.GetTile(position) == chinjuTile)
                {
                    mapData.ChinjuTiles.Add(position);
                }
            }
            GameDataController.Instance.TriggerMapDataChanged();
            Debug.Log("[MapManager] 地圖數據已保存");
        }
    }
}
