using UnityEngine;

/// Útil si el player/IA se cae. Se resetea a spawnPoint.
/// Nota: `DungeonARPlacer` ya asigna automáticamente `spawnPoint` al mapa instanciado. :contentReference[oaicite:6]{index=6}
public class RespawnOnFall : MonoBehaviour
{
    public Transform spawnPoint;   // si lo dejas vacío, usa el padre (mapa)
    public float minY = -2f;

    Vector3 localStart;

    void Start()
    {
        if (!spawnPoint) spawnPoint = transform.parent ? transform.parent : transform;
        localStart = transform.localPosition;
    }

    void Update()
    {
        if (transform.position.y < minY)
            Respawn();
    }

    public void Respawn()
    {
        if (!spawnPoint) return;
        // reubica manteniendo el sistema local del mapa
        transform.SetParent(spawnPoint, worldPositionStays: false);
        transform.localPosition = localStart;
        transform.localRotation = Quaternion.identity;
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
    }
}
