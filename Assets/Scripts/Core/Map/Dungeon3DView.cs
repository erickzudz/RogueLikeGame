using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class Dungeon3DView : MonoBehaviour
{
    [Header("Tamaño/Materiales")]
    public float tileSize = 2f;
    public float wallHeight = 2f;
    public float wallThickness = 0.2f;
    public Material floorMat;
    public Material wallMat;

    [Header("Layer")]
    public string generatedLayerName = "Floor"; // Crea esta Layer en el proyecto

    Transform content;        // contenedor de lo generado
    DungeonGrid lastGrid;     // para Rebuild opcional

    // Llamado por tu generador
    public void Build(DungeonGrid grid)
    {
        lastGrid = grid;
        EnsureContent();
        ClearChildren(content);

        int layer = LayerMask.NameToLayer(generatedLayerName);
        if (layer < 0) layer = 0; // Default si no existe la layer

        // -------- PISOS (una losa por celda) --------
        foreach (var kv in grid.AllCells())         // kv: KeyValuePair<Vector2Int,int>
        {
            Vector2Int cell = kv.Key;

            var floor = GameObject.CreatePrimitive(PrimitiveType.Cube);
            floor.name = $"Floor_{cell.x}_{cell.y}";
            floor.transform.SetParent(content, false);
            floor.transform.localPosition = CellCenter(cell);
            floor.transform.localScale = new Vector3(tileSize, 0.05f, tileSize);

            ApplyMat(floor, floorMat);
            SetLayerRecursive(floor, layer);

            var bc = floor.GetComponent<BoxCollider>();
            if (bc) bc.size = Vector3.one; // con primitive, 1 = bounds base
        }

        // -------- PAREDES (bits 0..3: N,E,S,W; 1 = abierto) --------
        foreach (var kv in grid.AllCells())
        {
            Vector2Int cell = kv.Key;
            int mask = kv.Value;
            Vector3 c = CellCenter(cell);

            // N: coloca siempre si está cerrado
            if ((mask & (1 << 0)) == 0)
            {
                MakeWall("Wall_N",
                    c + new Vector3(0, wallHeight * 0.5f, +tileSize * 0.5f - wallThickness * 0.5f),
                    new Vector3(tileSize, wallHeight, wallThickness), layer);
            }

            // E: coloca siempre si está cerrado
            if ((mask & (1 << 1)) == 0)
            {
                MakeWall("Wall_E",
                    c + new Vector3(+tileSize * 0.5f - wallThickness * 0.5f, wallHeight * 0.5f, 0),
                    new Vector3(wallThickness, wallHeight, tileSize), layer);
            }

            // S: coloca SOLO si está cerrado y NO hay vecino al sur (evitar duplicados)
            Vector2Int south = new Vector2Int(cell.x, cell.y - 1);
            if ((mask & (1 << 2)) == 0 && !grid.Contains(south))
            {
                MakeWall("Wall_S",
                    c + new Vector3(0, wallHeight * 0.5f, -tileSize * 0.5f + wallThickness * 0.5f),
                    new Vector3(tileSize, wallHeight, wallThickness), layer);
            }

            // W: coloca SOLO si está cerrado y NO hay vecino al oeste (evitar duplicados)
            Vector2Int west = new Vector2Int(cell.x - 1, cell.y);
            if ((mask & (1 << 3)) == 0 && !grid.Contains(west))
            {
                MakeWall("Wall_W",
                    c + new Vector3(-tileSize * 0.5f + wallThickness * 0.5f, wallHeight * 0.5f, 0),
                    new Vector3(wallThickness, wallHeight, tileSize), layer);
            }
        }
    }

    public void Clear()
    {
        EnsureContent();
        ClearChildren(content);
    }

    public void Rebuild()
    {
        if (lastGrid != null) Build(lastGrid);
    }

    // ---------- helpers ----------
    GameObject MakeWall(string name, Vector3 localPos, Vector3 localScale, int layer)
    {
        var wall = GameObject.CreatePrimitive(PrimitiveType.Cube);
        wall.name = name;
        wall.transform.SetParent(content, false);
        wall.transform.localPosition = localPos;
        wall.transform.localScale = localScale;

        ApplyMat(wall, wallMat);
        SetLayerRecursive(wall, layer);

        var bc = wall.GetComponent<BoxCollider>();
        if (bc) bc.size = Vector3.one;
        return wall;
    }

    Vector3 CellCenter(Vector2Int cell)
    {
        // centro de celda → multiplica por tileSize y centra en (+0.5,+0.5)
        return new Vector3((cell.x + 0.5f) * tileSize, 0f, (cell.y + 0.5f) * tileSize);
    }

    void ApplyMat(GameObject go, Material m)
    {
        if (!m) return;
        var r = go.GetComponent<Renderer>();
        if (r) r.sharedMaterial = m;
    }

    void EnsureContent()
    {
        if (!content)
        {
            var t = transform.Find("__CONTENT__");
            content = t ? t as Transform : new GameObject("__CONTENT__").transform;
            content.SetParent(transform, false);
            content.localPosition = Vector3.zero;
            content.localRotation = Quaternion.identity;
            content.localScale = Vector3.one;
        }
    }

    void ClearChildren(Transform t)
    {
        for (int i = t.childCount - 1; i >= 0; i--)
        {
            var c = t.GetChild(i);
            if (Application.isPlaying) Destroy(c.gameObject);
            else DestroyImmediate(c.gameObject);
        }
    }

    void SetLayerRecursive(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform child in go.transform)
            SetLayerRecursive(child.gameObject, layer);
    }

#if UNITY_EDITOR
    void OnValidate()
    {
        if (tileSize <= 0.01f) tileSize = 0.01f;
        if (wallThickness <= 0.01f) wallThickness = 0.01f;
    }
#endif
}
