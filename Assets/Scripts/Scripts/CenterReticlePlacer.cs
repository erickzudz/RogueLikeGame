using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class CenterReticlePlacer : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;
    public ARPlaneManager planeManager;

    [Header("Prefabs")]
    public GameObject previewPrefab;   // tu FloorPrefab (malla guía)
    public GameObject mapRootPrefab;   // tu mapRootPrefab (con MapGenerator)
    public Transform player;           // PlayerPrefab en la escena

    [Header("Mapa")]
    public float targetWidthMeters = 0.6f;   // ancho deseado en metros
    public float targetLengthMeters = 0.6f;  // largo deseado en metros
    public int width = 1;
    public int length = 1;
    public float tileSize = 0.01f;
    public bool placeOnlyOnce = true;
    public bool lockYawToCamera = true;

    static List<ARRaycastHit> hits = new();
    GameObject preview;       // instancia de la malla guía
    GameObject placedRoot;    // instancia del mapa
    Pose lastPose;
    ARPlane lastPlane;

    void Awake()
    {
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>();
        if (!anchorManager) anchorManager = GetComponent<ARAnchorManager>();
        if (!planeManager) planeManager = GetComponent<ARPlaneManager>();
    }

    void Start()
    {
        // crea la vista previa (pero oculta hasta que haya hit)
        if (previewPrefab)
        {
            preview = Instantiate(previewPrefab);
            preview.SetActive(false);
            preview.transform.localScale = new Vector3(targetWidthMeters, 1f, targetLengthMeters);
        }
    }

    void Update()
    {
        // Raycast desde el centro de la pantalla a planos
        var screenCenter = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        if (raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon))
        {
            var hit = hits[0];
            lastPose = hit.pose;
            lastPlane = hit.trackable as ARPlane;

            // rotación: solo yaw si se quiere (alineado a la cámara)
            if (lockYawToCamera && Camera.main)
            {
                var yaw = Quaternion.Euler(0, Camera.main.transform.eulerAngles.y, 0);
                lastPose = new Pose(lastPose.position, yaw);
            }

            if (preview)
            {
                preview.SetActive(true);
                preview.transform.SetPositionAndRotation(lastPose.position, lastPose.rotation);
            }
        }
        else
        {
            if (preview) preview.SetActive(false);
            lastPlane = null;
        }
    }

    public void Place()
    {
        if (placeOnlyOnce && placedRoot) return;
        if (lastPlane == null) return; // aún no hay plano válido

        // ancla sobre el plano detectado
        var anchor = anchorManager.AttachAnchor(lastPlane, lastPose);
        if (!anchor) return;

        // instancia mapa como hijo del ancla
        placedRoot = Instantiate(mapRootPrefab, anchor.transform);
        placedRoot.transform.localPosition = Vector3.zero;
        placedRoot.transform.localRotation = Quaternion.identity;

        var gen = placedRoot.GetComponent<RandomWalkDungeon>();
        var view = placedRoot.GetComponent<Dungeon3DView>();
        // Escala física deseada (ej. 0.6 m x 0.6 m)
        float targetMeters = 0.6f;
        view.tileSize = targetMeters / Mathf.Max(gen.width, gen.length);
        gen.Generate();

        // activar/posicionar player sobre SpawnPoint (si existe)
        if (player)
        {
            var sp = placedRoot.transform.Find("SpawnPoint");
            var pos = (sp ? sp.position : lastPose.position) + Vector3.up * 1.1f;
            player.position = pos;
            player.gameObject.SetActive(true);
        }

        // Congela la detección/visualización de planos (opcional)
        if (planeManager)
        {
            planeManager.requestedDetectionMode = PlaneDetectionMode.None;
            foreach (var p in planeManager.trackables)
            {
                p.gameObject.SetActive(false); // oculta las mallas amarillas
            }
        }

        if (preview) preview.SetActive(false);
    }
}
