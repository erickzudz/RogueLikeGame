using System.Collections.Generic;
using UnityEngine;

/// Pon este script en el ROOT del prefab del mapa (el que instancias en AR).
/// Solo arrastra los prefabs en el Inspector.
public class DungeonAutoSpawner : MonoBehaviour
{
    [Header("Prefabs")]
    public GameObject playerPrefab;
    public GameObject exitPortalPrefab;
    [Tooltip("Arrastra exactamente 9 (o los que tengas).")]
    public List<GameObject> enemyPrefabs = new List<GameObject>();

    [Header("Altura y alineación")]
    [Tooltip("Altura de aparición sobre el piso.")]
    public float yOffset = 0.02f;
    [Tooltip("Alinear al centro de celda usando tileSize de Dungeon3DView.")]
    public bool snapToGrid = true;

    [Header("Distribución")]
    [Tooltip("Radio relativo (0..0.5) del círculo para enemigos dentro del mapa.")]
    [Range(0.05f, 0.5f)] public float enemyRingRadiusRatio = 0.30f;

    // caches
    Dungeon3DView view;
    int cellsX = 10, cellsZ = 10;
    float tile;

    IEnumerator<WaitForEndOfFrame> Start()
    {
        // Espera un frame para que el generador (RandomWalk/RoomsCorridors) ya haya hecho Generate()
        yield return new WaitForEndOfFrame();

        view = GetComponent<Dungeon3DView>();
        tile = view ? Mathf.Max(0.005f, view.tileSize) : 0.05f;   // :contentReference[oaicite:3]{index=3}

        var rw = GetComponent<RandomWalkDungeon>();
        var rc = GetComponent<RoomsCorridorsDungeon>();
        if (rw) { cellsX = Mathf.Max(1, rw.width); cellsZ = Mathf.Max(1, rw.length); } // :contentReference[oaicite:4]{index=4}
        if (rc) { cellsX = Mathf.Max(1, rc.width); cellsZ = Mathf.Max(1, rc.length); } // :contentReference[oaicite:5]{index=5}

        SpawnAll();
    }

    void SpawnAll()
    {
        // tamaño del mapa en metros (local)
        float sizeX = cellsX * tile;
        float sizeZ = cellsZ * tile;
        float halfX = sizeX * 0.5f;
        float halfZ = sizeZ * 0.5f;

        // 1) Player (esquina “inferior-izquierda” del mapa)
        if (playerPrefab)
        {
            Vector3 local = new Vector3(-halfX + 1.5f * tile, yOffset, -halfZ + 1.5f * tile);
            if (snapToGrid) local = Snap(local);
            var p = Instantiate(playerPrefab, transform);
            p.transform.localPosition = local;
            p.transform.localRotation = Quaternion.identity;

            // Si tiene RespawnOnFall, su punto de respawn será el root del mapa (o cámbialo por un child "PlayerSpawn")
            var rf = p.GetComponent<RespawnOnFall>();
            if (rf) rf.spawnPoint = transform;
        }

        // 2) Portal de salida (esquina “superior-derecha” opuesta)
        if (exitPortalPrefab)
        {
            Vector3 local = new Vector3(halfX - 1.5f * tile, yOffset, halfZ - 1.5f * tile);
            if (snapToGrid) local = Snap(local);
            var g = Instantiate(exitPortalPrefab, transform);
            g.transform.localPosition = local;
            g.transform.localRotation = Quaternion.identity;
        }

        // 3) Enemigos: distribuidos en círculo dentro del mapa
        if (enemyPrefabs != null && enemyPrefabs.Count > 0)
        {
            int n = enemyPrefabs.Count;
            float r = Mathf.Min(halfX, halfZ) * enemyRingRadiusRatio;

            for (int i = 0; i < n; i++)
            {
                var prefab = enemyPrefabs[i];
                if (!prefab) continue;
                float t = (i / Mathf.Max(1f, n)) * Mathf.PI * 2f;
                Vector3 local = new Vector3(Mathf.Cos(t) * r, yOffset, Mathf.Sin(t) * r);
                // desplaza al centro del mapa (0,0 ya es centro porque el grid se construye alrededor de (0,0))
                if (snapToGrid) local = Snap(local);
                var e = Instantiate(prefab, transform);
                e.transform.localPosition = local;
                e.transform.localRotation = Quaternion.identity;
            }
        }
    }

    Vector3 Snap(Vector3 local)
    {
        // centra en el "centro de la celda": múltiplos de tile
        float sx = Mathf.Round(local.x / tile) * tile;
        float sz = Mathf.Round(local.z / tile) * tile;
        return new Vector3(sx, local.y, sz);
    }
}
