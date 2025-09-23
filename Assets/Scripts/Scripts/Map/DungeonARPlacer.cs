using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

[RequireComponent(typeof(ARRaycastManager))]
[RequireComponent(typeof(ARPlaneManager))]
public class DungeonARPlacer : MonoBehaviour
{
    [Header("Prefab del dungeon")]
    public GameObject dungeonPrefab;  // Debe tener Dungeon3DView + (RandomWalkDungeon o RoomsCorridorsDungeon) + PlayerAutoSpawn (+ PortalSpawner opcional)
    public bool placeOnce = true;

    [Header("UI/Feedback (opcional)")]
    public Transform reticle;         // Un quad/pequeño gizmo para indicar dónde se colocará

    ARRaycastManager ray;
    ARPlaneManager planes;
    GameObject spawned;
    Pose lastPose;
    bool hasPose;

    static readonly List<ARRaycastHit> hits = new();

    void Awake()
    {
        ray = GetComponent<ARRaycastManager>();
        planes = GetComponent<ARPlaneManager>();
        if (reticle) reticle.gameObject.SetActive(false);
    }

    void Update()
    {
        if (spawned && placeOnce) return;

        // Raycast al CENTRO de la pantalla (robusto para cualquier sistema de entrada)
        Vector2 center = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        hasPose = ray.Raycast(center, hits, TrackableType.PlaneWithinPolygon);
        if (!hasPose)
        {
            if (reticle) reticle.gameObject.SetActive(false);
            return;
        }

        var hit = hits[0];
        lastPose = hit.pose;

        // Alinear yaw hacia la cámara (horizontal)
        var fwd = Camera.main ? Camera.main.transform.forward : Vector3.forward;
        fwd.y = 0f; if (fwd.sqrMagnitude < 1e-3f) fwd = Vector3.forward;
        lastPose.rotation = Quaternion.LookRotation(fwd.normalized, Vector3.up);

        if (reticle)
        {
            reticle.gameObject.SetActive(true);
            reticle.SetPositionAndRotation(lastPose.position, lastPose.rotation);
        }
    }

    // LLAMA ESTE MÉTODO DESDE TU BOTÓN "ColocarAquí"
    public void PlaceAtReticle()
    {
        if (!hasPose || (spawned && placeOnce)) return;
        SpawnAt(lastPose);
    }

    void SpawnAt(Pose pose)
    {
        spawned = Instantiate(dungeonPrefab, pose.position, pose.rotation);

        // Ajustar tileSize para encajar en el plano más cercano
        var view = spawned.GetComponent<Dungeon3DView>();
        if (view)
        {
            int cellsX = 16, cellsZ = 16;
            var rw = spawned.GetComponent<RandomWalkDungeon>();
            var rc = spawned.GetComponent<RoomsCorridorsDungeon>();
            if (rw) { cellsX = Mathf.Max(1, rw.width); cellsZ = Mathf.Max(1, rw.length); }
            if (rc) { cellsX = Mathf.Max(1, rc.width); cellsZ = Mathf.Max(1, rc.length); }

            // tamaño aproximado del plano objetivo
            Vector2 planeSize = new Vector2(0.8f, 0.8f);
            if (planes && planes.trackables.count > 0)
            {
                ARPlane best = null; float bestD = float.PositiveInfinity;
                foreach (var p in planes.trackables)
                {
                    float d = (p.transform.position - pose.position).sqrMagnitude;
                    if (d < bestD) { bestD = d; best = p; }
                }
                if (best) planeSize = best.size;
            }

            float fit = Mathf.Min(planeSize.x, planeSize.y) * 0.9f; // margen
            view.tileSize = Mathf.Clamp(fit / Mathf.Max(cellsX, cellsZ), 0.01f, 0.1f);
        }

        // Regenerar con el nuevo tamaño
        spawned.GetComponent<RandomWalkDungeon>()?.Generate();
        spawned.GetComponent<RoomsCorridorsDungeon>()?.Generate();

        if (placeOnce)
        {
            planes.enabled = false;
            foreach (var p in planes.trackables) p.gameObject.SetActive(false);
        }

        // después de crear 'spawned'
        var respawns = FindObjectsOfType<RespawnOnFall>(true);
        foreach (var r in respawns) r.spawnPoint = spawned.transform;   // usa la instancia del mapa
    }
}
