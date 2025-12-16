using UnityEngine;
using UnityEngine.Tilemaps;

/// <summary>
/// 타일 생성 및 확인을 도와주는 헬퍼 스크립트
/// </summary>
public class TileHelper : MonoBehaviour
{
    [ContextMenu("현재 타일맵의 타일 확인")]
    public void CheckTilesInTilemap()
    {
        Tilemap[] tilemaps = FindObjectsByType<Tilemap>(FindObjectsSortMode.None);
        
        foreach (var tilemap in tilemaps)
        {
            Debug.Log($"=== {tilemap.name} Tilemap ===");
            
            BoundsInt bounds = tilemap.cellBounds;
            int tileCount = 0;
            
            for (int x = bounds.xMin; x < bounds.xMax; x++)
            {
                for (int y = bounds.yMin; y < bounds.yMax; y++)
                {
                    Vector3Int pos = new Vector3Int(x, y, 0);
                    TileBase tile = tilemap.GetTile(pos);
                    if (tile != null)
                    {
                        tileCount++;
                        Debug.Log($"타일 위치: ({x}, {y}), 타일 타입: {tile.GetType().Name}");
                        
                        if (tile is Tile)
                        {
                            Tile spriteTile = tile as Tile;
                            if (spriteTile.sprite != null)
                            {
                                Debug.Log($"  - 스프라이트: {spriteTile.sprite.name}");
                            }
                            else
                            {
                                Debug.LogWarning($"  - 스프라이트가 없습니다!");
                            }
                        }
                    }
                }
            }
            
            Debug.Log($"총 타일 개수: {tileCount}");
        }
    }
    
    [ContextMenu("타일맵 렌더러 설정 확인")]
    public void CheckTilemapRenderer()
    {
        TilemapRenderer[] renderers = FindObjectsByType<TilemapRenderer>(FindObjectsSortMode.None);
        
        foreach (var renderer in renderers)
        {
            Debug.Log($"=== {renderer.gameObject.name} TilemapRenderer ===");
            Debug.Log($"Enabled: {renderer.enabled}");
            Debug.Log($"Material: {(renderer.material != null ? renderer.material.name : "None (기본 Material 사용)")}");
            Debug.Log($"Sorting Layer: {renderer.sortingLayerName}");
            Debug.Log($"Sorting Order: {renderer.sortingOrder}");
            Debug.Log($"Mode: {renderer.mode}");
        }
    }
}

