using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class SetupEdgeCollider : MonoBehaviour
{
    public Tilemap tilemap;
    public EdgeCollider2D edgeCollider;

    private void Start()
    {
        Bounds tilemapBounds = tilemap.localBounds;
        List<Vector2> edgePoints = new List<Vector2>();
        edgePoints.Add(tilemapBounds.min);
        edgePoints.Add(tilemapBounds.max - new Vector3(0, tilemapBounds.extents.y*2, 0));
        edgePoints.Add(tilemapBounds.max);
        edgePoints.Add(tilemapBounds.min + new Vector3(0, tilemapBounds.extents.y * 2, 0));
        edgePoints.Add(tilemapBounds.min);
        edgeCollider.SetPoints(edgePoints);
    }
}
