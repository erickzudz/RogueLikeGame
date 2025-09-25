using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARPlacer : MonoBehaviour
{
    [Header("AR")]
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;
    public ARPlaneManager planeManager;

    [Header("Prefabs")]
    public GameObject mapRootPrefab;   // Prefab que ya genera el dungeon
    public Transform reticle;         // Quad/Plane para previsualizar

    [Header("Escala")]
    [Tooltip("Escala global del mapa instanciado. 0.1 = diez veces más pequeño.")]
    public float mapScale = 0.1f;      // <<< 10× más pequeño
    [Tooltip("Multiplicador visual del reticle (1 = exacto).")]
    public float reticleScaleMult = 1f;

    [Header("Opciones")]
    public bool placeOnlyOnce = true;
    public bool autoSizeReticleToMap = true;

    static readonly List<ARRaycastHit> hits = new();
    GameObject mapInstance;
    ARAnchor anchor;

    void Awake()
    {
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>();
        if (!anchorManager) anchorManager = GetComponent<ARAnchorManager>();
        if (!planeManager) planeManager = GetComponent<ARPlaneManager>();
    }

    void Start()
    {
        // Ajusta el tamaño del reticle a la huella del mapa * mapScale
        if (autoSizeReticleToMap && reticle && mapRootPrefab)
        {
            float tile = 0.05f; // fallback
            int w = 10, h = 10;

            var view = mapRootPrefab.GetComponent<Dungeon3DView>();
            if (view) tile = Mathf.Max(0.005f, view.tileSize);

            var rcd = mapRootPrefab.GetComponent<RoomsCorridorsDungeon>();
            if (rcd) { w = Mathf.Max(1, rcd.width); h = Mathf.Max(1, rcd.length); }
            else
            {
                var rw = mapRootPrefab.GetComponent<RandomWalkDungeon>();
                if (rw) { w = Mathf.Max(1, rw.width); h = Mathf.Max(1, rw.length); }
            }

            // El reticle es un quad en XZ → localScale.x = ancho, .z = largo
            var sizeX = w * tile * mapScale * reticleScaleMult;
            var sizeZ = h * tile * mapScale * reticleScaleMult;
            reticle.localScale = new Vector3(sizeX, 1f, sizeZ);
        }
    }

    void Update()
    {
        if (!raycastManager) return;
        
        // Realizar raycast solo contra planos AR
        var screenCenter = Camera.main.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        raycastManager.Raycast(screenCenter, hits, TrackableType.PlaneWithinPolygon);
        
        // Actualizar posición del reticle
        if (hits.Count > 0)
        {
            // Mostrar y actualizar reticle
            if (reticle) 
            {
                reticle.gameObject.SetActive(true);
                reticle.position = hits[0].pose.position;
                reticle.rotation = hits[0].pose.rotation;
            }
        }
        else if (reticle)
        {
            reticle.gameObject.SetActive(false);
        }
    }

    public void PlaceMap()
    {
        if (hits.Count == 0) return;
        if (placeOnlyOnce && mapInstance) return;

        // Crear ancla y spawn del mapa
        var anchorObject = new GameObject("MapAnchor");
        anchorObject.transform.position = hits[0].pose.position;
        anchorObject.transform.rotation = hits[0].pose.rotation;
        
        if (anchorManager)
            anchor = anchorManager.AttachAnchor(hits[0].trackable as ARPlane, hits[0].pose);
            
        mapInstance = Instantiate(mapRootPrefab, hits[0].pose.position, hits[0].pose.rotation);
        mapInstance.transform.localScale = Vector3.one * mapScale;
        
        if (anchor) 
            mapInstance.transform.parent = anchor.transform;
    }
}
