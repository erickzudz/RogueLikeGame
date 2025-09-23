using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class PlaceMapOnPlane : MonoBehaviour
{
    public ARRaycastManager raycastManager;
    public ARAnchorManager anchorManager;
    public GameObject mapRootPrefab;     // Asigna MapRoot.prefab
    public bool placeOnlyOnce = true;
    public Transform player;          // arrastra tu PlayerPrefab de la escena
    public bool enablePlayerOnPlace = true;

    static List<ARRaycastHit> hits = new();
    GameObject placedRoot;

    void Awake()
    {
        if (!raycastManager) raycastManager = GetComponent<ARRaycastManager>();
        if (!anchorManager) anchorManager = GetComponent<ARAnchorManager>();
    }

    void Update()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) TryPlace(Input.mousePosition);
#else
        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began)
            TryPlace(Input.GetTouch(0).position);
#endif
    }

    void TryPlace(Vector2 screenPos)
    {
        if (placeOnlyOnce && placedRoot) return;

        if (raycastManager.Raycast(screenPos, hits, TrackableType.PlaneWithinPolygon))
        {
            var hit = hits[0];
            var anchor = anchorManager.AttachAnchor((ARPlane)hit.trackable,
                              new Pose(hit.pose.position, hit.pose.rotation));

            if (anchor)
            {
                if (!placedRoot)
                    placedRoot = Instantiate(mapRootPrefab, anchor.transform);
                else
                    placedRoot.transform.SetPositionAndRotation(hit.pose.position, hit.pose.rotation);

                // ► Reposicionar/activar al jugador sobre el SpawnPoint del mapa
                if (player)
                {
                    var sp = placedRoot.transform.Find("SpawnPoint");
                    var pos = (sp ? sp.position : hit.pose.position) + Vector3.up * 1.1f;
                    player.position = pos;
                    player.gameObject.SetActive(true);
                }
            }
        }
    }

}
