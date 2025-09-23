using System.Collections;
using UnityEngine;
using UnityEngine.XR.ARFoundation;

public class ARDungeonBootstrap : MonoBehaviour
{
    const string LOG = "[Bootstrap] ";

    [Header("XR refs (desde XR Origin)")]
    public ARRaycastManager raycastMgr;
    public ARPlaneManager planeMgr;
    public ARAnchorManager anchorMgr;

    [Header("Prefabs (Resources paths)")]
    public string dungeonPrefabPath = "Prefabs/map/DungeonRoot"; // solo para referencia
    public string playerPrefabPath = "Prefabs/Player";
    public string bossPrefabPath = "Prefabs/Boss";
    public string enemyPrefabPath = "Prefabs/Enemy";

    [Header("Spawns")]
    public int initialEnemies = 3;
    public float arenaRadius = 8f;
    public bool spawnBoss = true;
    public bool spawnEnemies = true;
    public float yOffset = 0.25f; // evitar incrustar en el piso

    [Header("Opcional UI")]
    public Transform reticle;

    DungeonARPlacer placer;   // si existe
    Transform dungeonRoot;    // instancia viva

    void Awake()
    {
        placer = FindObjectOfType<DungeonARPlacer>(true);
        Debug.Log($"{LOG}Awake. Placer: {(placer ? placer.name : "null")}");
    }

    void Start()
    {
        StartCoroutine(WaitAndSpawn());
    }

    // Puedes llamarlo desde un botón si quieres reintentar
    public void Respawn()
    {
        if (dungeonRoot) StartCoroutine(SpawnAllCo(true));
        else Debug.LogWarning($"{LOG}Respawn pedido pero aún no hay dungeonRoot.");
    }

    IEnumerator WaitAndSpawn()
    {
        // Espera hasta que aparezca un objeto llamado "DungeonRoot"
        // (ajusta el nombre si tu prefab final se llama distinto)
        float timeout = 12f;
        float t = 0f;

        Debug.Log($"{LOG}Buscando 'DungeonRoot' en escena...");
        while (!dungeonRoot && t < timeout)
        {
            var found = GameObject.Find("DungeonRoot"); // <- cambia el nombre si usas otro
            if (found)
            {
                dungeonRoot = found.transform;
                Debug.Log($"{LOG}DungeonRoot encontrado: {dungeonRoot.name}");
                break;
            }

            // si tienes un placer que instancia más tarde, esperamos
            t += 0.25f;
            yield return new WaitForSeconds(0.25f);
        }

        if (!dungeonRoot)
        {
            Debug.LogError($"{LOG}No se encontró 'DungeonRoot' en {timeout} s. " +
                           "Verifica que el placer realmente lo instancie con ese nombre.");
            yield break;
        }

        yield return SpawnAllCo(false);
    }

    IEnumerator SpawnAllCo(bool forceDespawn)
    {
        // Deja respirar un frame al generador del mapa
        yield return new WaitForEndOfFrame();
        yield return new WaitForSeconds(0.05f);

        Vector3 center = GetDungeonCenter(dungeonRoot);
        Vector3 up = Vector3.up * yOffset;

        // --- Player ---
        var playerPrefab = Resources.Load<GameObject>(playerPrefabPath);
        if (!playerPrefab)
        {
            Debug.LogError($"{LOG}No existe Resources.Load('{playerPrefabPath}')");
            yield break;
        }

        if (forceDespawn)
        {
            foreach (var p in GameObject.FindGameObjectsWithTag("Player")) Destroy(p);
            foreach (var e in GameObject.FindGameObjectsWithTag("Enemy")) Destroy(e);
        }

        var player = Instantiate(playerPrefab, center + up, Quaternion.identity);
        Debug.Log($"{LOG}Player instanciado en {player.transform.position}");

        // --- Boss ---
        if (spawnBoss && !string.IsNullOrEmpty(bossPrefabPath))
        {
            var bossPrefab = Resources.Load<GameObject>(bossPrefabPath);
            if (!bossPrefab) Debug.LogWarning($"{LOG}No existe '{bossPrefabPath}'");
            else
            {
                Vector3 bossPos = center + new Vector3(arenaRadius * 0.5f, 0, 0) + up;
                Instantiate(bossPrefab, bossPos, Quaternion.identity);
                Debug.Log($"{LOG}Boss instanciado en {bossPos}");
            }
        }

        // --- Enemigos ---
        if (spawnEnemies && !string.IsNullOrEmpty(enemyPrefabPath) && initialEnemies > 0)
        {
            var enemyPrefab = Resources.Load<GameObject>(enemyPrefabPath);
            if (!enemyPrefab)
            {
                Debug.LogWarning($"{LOG}No existe '{enemyPrefabPath}'");
            }
            else
            {
                for (int i = 0; i < initialEnemies; i++)
                {
                    float ang = (360f / Mathf.Max(1, initialEnemies)) * i;
                    Vector3 pos = center + new Vector3(
                        Mathf.Cos(ang * Mathf.Deg2Rad),
                        0,
                        Mathf.Sin(ang * Mathf.Deg2Rad)
                    ) * (arenaRadius * 0.75f);

                    Instantiate(enemyPrefab, pos + up, Quaternion.identity);
                    Debug.Log($"{LOG}Enemy {i} en {pos + up}");
                    yield return null;
                }
            }
        }
    }

    Vector3 GetDungeonCenter(Transform root)
    {
        var rends = root.GetComponentsInChildren<MeshRenderer>();
        if (rends != null && rends.Length > 0)
        {
            var b = new Bounds(rends[0].bounds.center, Vector3.zero);
            for (int i = 1; i < rends.Length; i++) b.Encapsulate(rends[i].bounds);
            var c = b.center; c.y = root.position.y; // nivel al piso del AR
            Debug.Log($"{LOG}Centro calculado por bounds: {c}");
            return c;
        }
        Debug.LogWarning($"{LOG}No hay MeshRenderers; uso root.position ({root.position})");
        return root.position;
    }
}
