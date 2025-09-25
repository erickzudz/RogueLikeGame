using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Prefabs (Resources path)")]
    [Tooltip("Ruta dentro de Resources. Ej: Resources/Prefabs/Player.prefab -> \"Prefabs/Player\"")]
    public string playerPrefabPath = "Prefabs/Player";

    [Header("Enemies (Resources paths)")]
    [Tooltip("Rutas en Resources. Ej: Resources/Prefabs/Enemies/Shotgun.prefab -> \"Prefabs/Enemies/Shotgun\"")]
    public string[] enemyPrefabPaths = { "Prefabs/Enemies/Shotgun" };
    public int totalEnemies = 8;
    public float enemySpawnYOffset = 0.02f;

    [Header("Exit portal (Resources path)")]
    [Tooltip("Ruta dentro de Resources para el portal de salida")]
    public string exitPortalPath = "Prefabs/ExitPortal";
    public float portalYOffset = 0.02f;

    [Header("Mapa")]
    public RoomsCorridorsDungeon dungeon;  // arrastra el componente del mapa (en escena)
    public int fallbackSeed = 12345;

    const string ROOM_SEED_KEY = "seed";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinRandomOrCreateRoom(
            roomOptions: new RoomOptions { MaxPlayers = 4 },
            typedLobby: TypedLobby.Default
        );
    }

    public override void OnCreatedRoom()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            int seed = Random.Range(int.MinValue, int.MaxValue);
            PhotonNetwork.CurrentRoom.SetCustomProperties(
                new Hashtable { { ROOM_SEED_KEY, seed } }
            );
        }
    }

    public override void OnJoinedRoom()
    {
        // 1) Obtener seed compartido y generar el mapa
        int seed = fallbackSeed;
        if (PhotonNetwork.CurrentRoom?.CustomProperties?.ContainsKey(ROOM_SEED_KEY) == true)
            seed = (int)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_SEED_KEY];

        if (dungeon != null)
        {
            dungeon.seed = seed;
            dungeon.Generate(); // construye pisos + paredes con colliders

            var placer = dungeon.GetComponent<SpawnPointPlacer>();
            if (placer) placer.PlaceAll(); // crea SP_Player, SP_Exit, SP_Enemy
        }

        // 2) Spawnear portal (sólo Master para evitar duplicados)
        SpawnExitPortal();

        // 3) Spawnear enemigos (sólo Master)
        SpawnEnemies(seed);

        // 4) Spawnear Player en SP_Player
        var spPlayer = FindObjectsOfType<SpawnPoint>()
                       .FirstOrDefault(sp => sp.type == SpawnPointType.Player);

        Vector3 pos = spPlayer ? spPlayer.transform.position : Vector3.zero;
        Quaternion rot = spPlayer ? spPlayer.transform.rotation : Quaternion.identity;

        PhotonNetwork.Instantiate(playerPrefabPath, pos, rot);
    }

    void SpawnExitPortal()
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (string.IsNullOrEmpty(exitPortalPath)) return;

        var spExit = FindObjectsOfType<SpawnPoint>()
                    .FirstOrDefault(sp => sp.type == SpawnPointType.Exit);
        if (!spExit) return;

        Vector3 p = spExit.transform.position + Vector3.up * portalYOffset;
        Quaternion r = spExit.transform.rotation;

        PhotonNetwork.Instantiate(exitPortalPath, p, r);
    }

    void SpawnEnemies(int seed)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        if (enemyPrefabPaths == null || enemyPrefabPaths.Length == 0) return;

        var enemyPoints = FindObjectsOfType<SpawnPoint>()
                          .Where(sp => sp.type == SpawnPointType.Enemy)
                          .ToList();
        if (enemyPoints.Count == 0) return;

        // RNG determinista para elegir prefabs
        System.Random rng = new System.Random(seed ^ 0x51F00D);

        // Si deseas “uno por SP” en lugar de totalEnemies, usa:
        // int count = Mathf.Min(totalEnemies, enemyPoints.Count);
        int count = totalEnemies;

        for (int i = 0; i < count; i++)
        {
            var sp = enemyPoints[i % enemyPoints.Count];
            string path = enemyPrefabPaths[rng.Next(enemyPrefabPaths.Length)];
            Vector3 p = sp.transform.position + Vector3.up * enemySpawnYOffset;
            Quaternion r = sp.transform.rotation;

            PhotonNetwork.Instantiate(path, p, r);
        }
    }
}
