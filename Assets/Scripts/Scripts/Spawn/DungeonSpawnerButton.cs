using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class DungeonSpawnerButton : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject exitPortalPrefab;
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Spawn Options")]
    [Tooltip("Altura sobre el piso para evitar clipping.")]
    public float yOffset = 0.02f;
    public bool snapToGrid = true;
    [Range(0.05f, 0.5f)]
    public float enemyRingRadiusRatio = 0.30f;

    [Header("Scale (relative to tile)")]
    [Tooltip("Tamaño del Player en múltiplos del tile. Ej: 0.5 -> la mitad de una celda.")]
    public float playerScaleInTiles = 0.5f;
    [Tooltip("Tamaño de cada enemigo en múltiplos del tile.")]
    public float enemyScaleInTiles = 0.45f;
    [Tooltip("Tamaño del portal en múltiplos del tile.")]
    public float portalScaleInTiles = 0.6f;
    [Tooltip("Si está activo, forzamos la escala local del prefab en base al tile del mapa.")]
    public bool overridePrefabScale = true;

    // cache
    float tile = 0.05f;
    GameObject dungeonInstance;
    Dungeon3DView view;
    RoomsCorridorsDungeon rooms;
    RandomWalkDungeon walk;

    // Llama este método desde el botón "Prefacts" o similar
    public void SpawnEntities()
    {
        if (!ResolveDungeonInstance())
        {
            Debug.LogWarning("❌ No se encontró el dungeon instanciado. Coloca primero el mapa.");
            return;
        }

        if (view) tile = Mathf.Max(0.001f, view.tileSize);

        // Celdas de inicio/meta
        Vector2Int startCell = Vector2Int.zero;
        Vector2Int goalCell = Vector2Int.zero;

        if (rooms)
        {
            startCell = rooms.StartCell;
            goalCell = rooms.EndCell;
        }
        else if (walk)
        {
            startCell = Vector2Int.zero;     // RandomWalk empieza en (0,0)
            goalCell = walk.EndCell;
        }

        // --- PLAYER ---
        if (playerPrefab)
        {
            var p = Instantiate(playerPrefab, dungeonInstance.transform);
            p.transform.localPosition = CellToLocal(startCell) + Vector3.up * yOffset;
            p.transform.localRotation = Quaternion.identity;
            if (overridePrefabScale) SetScaleInTiles(p.transform, playerScaleInTiles);
        }

        // --- PORTAL ---
        if (exitPortalPrefab)
        {
            var g = Instantiate(exitPortalPrefab, dungeonInstance.transform);
            g.transform.localPosition = CellToLocal(goalCell) + Vector3.up * yOffset;
            g.transform.localRotation = Quaternion.identity;
            if (overridePrefabScale) SetScaleInTiles(g.transform, portalScaleInTiles);
        }

        // --- ENEMIGOS (anillo en el centro del mapa) ---
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            int n = enemyPrefabs.Count;

            int cellsX = rooms ? rooms.width : (walk ? walk.width : 16);
            int cellsZ = rooms ? rooms.length : (walk ? walk.length : 16);

            float halfX = cellsX * tile * 0.5f;
            float halfZ = cellsZ * tile * 0.5f;
            float r = Mathf.Min(halfX, halfZ) * enemyRingRadiusRatio;

            for (int i = 0; i < n; i++)
            {
                var prefab = enemyPrefabs[i];
                if (!prefab) continue;

                float t = (i / (float)n) * Mathf.PI * 2f;
                Vector3 local = new Vector3(Mathf.Cos(t) * r, yOffset, Mathf.Sin(t) * r);

                if (snapToGrid) local = Snap(local);

                var e = Instantiate(prefab, dungeonInstance.transform);
                e.transform.localPosition = local;
                e.transform.localRotation = Quaternion.identity;
                if (overridePrefabScale) SetScaleInTiles(e.transform, enemyScaleInTiles);
            }
        }

        Debug.Log("✅ Player, portal y enemigos spawneados (escala ligada al tile del mapa).");
    }

    // ---------- Helpers ----------
    bool ResolveDungeonInstance()
    {
        if (dungeonInstance && view) return true;

        // 1) Busca por nombre estándar
        var go = GameObject.Find("DungeonRoot(Clone)");
        // 2) Si no está, toma el más reciente con Dungeon3DView
        if (!go)
        {
            var views = FindObjectsOfType<Dungeon3DView>();
            if (views != null && views.Length > 0)
                go = views.OrderBy(v => v.gameObject.GetInstanceID()).Last().gameObject;
        }

        if (!go) return false;

        dungeonInstance = go;
        view = go.GetComponent<Dungeon3DView>();
        rooms = go.GetComponent<RoomsCorridorsDungeon>();
        walk = go.GetComponent<RandomWalkDungeon>();
        return view != null || rooms != null || walk != null;
    }

    Vector3 CellToLocal(Vector2Int c) => new Vector3(c.x * tile, 0f, c.y * tile);

    Vector3 Snap(Vector3 local)
    {
        float sx = Mathf.Round(local.x / tile) * tile;
        float sz = Mathf.Round(local.z / tile) * tile;
        return new Vector3(sx, local.y, sz);
    }

    void SetScaleInTiles(Transform t, float tiles)
    {
        // Escala local = (factor en tiles) * (tamaño del tile en metros)
        float s = Mathf.Max(0.0001f, tiles) * tile;
        t.localScale = Vector3.one * s;
    }
}
