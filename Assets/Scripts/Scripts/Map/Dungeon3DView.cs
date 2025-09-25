using System.Collections.Generic;
using UnityEngine;

public class Dungeon3DView : MonoBehaviour
{
    [Header("Tamaño/Materiales")]
    public float tileSize = 0.05f;   // metros por celda
    public float wallHeight = 0.08f;
    public float wallThickness = 0.01f;
    public Material floorMat;
    public Material wallMat;

    readonly List<GameObject> spawned = new();

    public void Clear()
    {
        foreach (var go in spawned) if (go) Destroy(go);
        spawned.Clear();
    }

    public void Build(DungeonGrid grid)
    {
        Clear();

        var clamp = grid.Clamp;

        foreach (var kv in grid.AllCells())
        {
            var p = kv.Key;
            int code = kv.Value;

            Vector3 center = new Vector3(p.x * tileSize, 0f, p.y * tileSize);

            // Piso
            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.transform.SetParent(transform, false);
            floor.transform.localPosition = center;
            floor.transform.localScale = new Vector3(tileSize, 0.005f, tileSize);
            if (floorMat) floor.GetComponent<MeshRenderer>().material = floorMat;
            spawned.Add(floor);

            // — vecinos para saber si existen —
            Vector2Int north = new Vector2Int(p.x, p.y + 1);
            Vector2Int east = new Vector2Int(p.x + 1, p.y);
            Vector2Int south = new Vector2Int(p.x, p.y - 1);
            Vector2Int west = new Vector2Int(p.x - 1, p.y);

            // N y E: si están cerrados, dibujamos pared (no hay duplicado)
            if ((code & 1) == 0) PlaceWallNS(center, +1); // Norte
            if ((code & 2) == 0) PlaceWallEW(center, +1); // Este

            // S y O: dibujar solo si el vecino NO existe (borde del mapa)
            if (!grid.Contains(south)) PlaceWallNS(center, -1);
            if (!grid.Contains(west)) PlaceWallEW(center, -1);
        }
    }

    // Pared a lo largo del eje X (N/S): larga en X, fina en Z
    void PlaceWallNS(Vector3 center, int sign)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(transform, false);
        float half = tileSize * 0.5f;
        wall.transform.localPosition = center + new Vector3(0, wallHeight * 0.5f, sign * half);
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = new Vector3(tileSize, wallHeight, wallThickness);
        if (wallMat) wall.GetComponent<MeshRenderer>().material = wallMat;
        spawned.Add(wall);
    }

    // Pared a lo largo del eje Z (E/O): larga en Z, fina en X
    void PlaceWallEW(Vector3 center, int sign)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.transform.SetParent(transform, false);
        float half = tileSize * 0.5f;
        wall.transform.localPosition = center + new Vector3(sign * half, wallHeight * 0.5f, 0);
        wall.transform.localRotation = Quaternion.identity;
        wall.transform.localScale = new Vector3(wallThickness, wallHeight, tileSize);
        if (wallMat) wall.GetComponent<MeshRenderer>().material = wallMat;
        spawned.Add(wall);
    }
}
