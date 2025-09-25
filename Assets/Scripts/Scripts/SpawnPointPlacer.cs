using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[DisallowMultipleComponent]
public class SpawnPointPlacer : MonoBehaviour
{
    [Header("Refs")]
    public RoomsCorridorsDungeon dungeon;   // tu generador
    public Transform root;                  // normalmente el mismo GameObject del dungeon
    public float tileSize = 2f;             // MISMO valor que en Dungeon3DView
    public Vector3 pivotOffset = new Vector3(0.5f, 0.02f, 0.5f);

    [Header("Enemy spawn config")]
    public int enemySpawns = 8;
    public int minDistanceFromPlayer = 6;   // en celdas

    [Header("Cleanup")]
    public bool clearOldSpawnsOnGenerate = true;

    readonly List<GameObject> created = new();

    public void PlaceAll()
    {
        if (!dungeon)
        {
            Debug.LogError("[SpawnPointPlacer] Falta 'dungeon'.");
            return;
        }
        if (!root) root = dungeon.transform;

        if (clearOldSpawnsOnGenerate)
        {
            foreach (var go in created) if (go) DestroyImmediate(go);
            created.Clear();
            foreach (var sp in GetComponentsInChildren<SpawnPoint>())
                DestroyImmediate(sp.gameObject);
        }

        // 1) Player
        CreateSpawn(dungeon.StartCell, SpawnPointType.Player, "SP_Player", 0.6f);

        // 2) Exit
        CreateSpawn(dungeon.EndCell, SpawnPointType.Exit, "SP_Exit", 0.6f);

        // 3) Enemigos (centros alejados del player)
        var playerCell = dungeon.StartCell;
        var centers = dungeon.RoomCenters ?? new List<Vector2Int> { dungeon.EndCell };

        // Candidatos
        var candidates = centers
            .Where(c => Manhattan(c, playerCell) >= minDistanceFromPlayer)
            .OrderByDescending(c => Manhattan(c, playerCell))
            .ToList();

        // Barajar un poco
        for (int i = 0; i < candidates.Count; i++)
        {
            int j = Random.Range(i, candidates.Count);
            (candidates[i], candidates[j]) = (candidates[j], candidates[i]);
        }

        int count = Mathf.Min(enemySpawns, candidates.Count);
        for (int i = 0; i < count; i++)
            CreateSpawn(candidates[i], SpawnPointType.Enemy, $"SP_Enemy_{i:D2}", 0.5f);

        Debug.Log($"[SpawnPointPlacer] Spawns: Player=1, Exit=1, Enemigos={count}");
    }

    // ----- helpers -----
    int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);

    void CreateSpawn(Vector2Int cell, SpawnPointType type, string name, float radius)
    {
        var local = new Vector3(cell.x, 0f, cell.y);
        local = Vector3.Scale(local, new Vector3(tileSize, 1f, tileSize))
              + Vector3.Scale(pivotOffset, new Vector3(tileSize, 1f, tileSize));

        var go = new GameObject(name);
        go.transform.SetParent(root, false);
        go.transform.localPosition = local;

        var sp = go.AddComponent<SpawnPoint>();
        sp.type = type;
        sp.radius = radius;

        created.Add(go);
    }
}
