using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class Launcher : MonoBehaviourPunCallbacks
{
    [Header("Prefabs (Resources path)")]
    public string playerPrefabPath = "Prefabs/Player";

    [Header("Enemies (Resources paths)")]
    [Tooltip("Rutas en Resources. Ej: Resources/Prefabs/Enemies/Shotgun.prefab -> \"Prefabs/Enemies/Shotgun\"")]
    public string[] enemyPrefabPaths = { "Prefabs/Enemies/Shotgun" };
    public int totalEnemies = 8;
    public float enemySpawnYOffset = 0.02f;

    [Header("Mapa")]
    public RoomsCorridorsDungeon dungeon;
    public int fallbackSeed = 12345;

    const string ROOM_SEED_KEY = "seed";

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        if (!PhotonNetwork.IsConnected) PhotonNetwork.ConnectUsingSettings();
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
                new ExitGames.Client.Photon.Hashtable { { ROOM_SEED_KEY, seed } }
            );
        }
    }

    public override void OnJoinedRoom()
    {
        // 1) Generar mapa con seed compartido
        int seed = fallbackSeed;
        if (PhotonNetwork.CurrentRoom?.CustomProperties?.ContainsKey(ROOM_SEED_KEY) == true)
            seed = (int)PhotonNetwork.CurrentRoom.CustomProperties[ROOM_SEED_KEY];

        if (dungeon != null)
        {
            dungeon.seed = seed;
            dungeon.Generate();                                // crea pisos+paredes con colliders :contentReference[oaicite:0]{index=0}
            var placer = dungeon.GetComponent<SpawnPointPlacer>();
            if (placer) placer.PlaceAll();                     // crea SP_Player, SP_Exit, SP_Enemy :contentReference[oaicite:1]{index=1}
        }

        // 2) Spawn de ENEMIGOS (sólo Master)
        SpawnEnemies(seed);

        // 3) Spawn del PLAYER en SP_Player
        var spPlayer = FindObjectsOfType<SpawnPoint>()
                       .FirstOrDefault(sp => sp.type == SpawnPointType.Player); // :contentReference[oaicite:2]{index=2}
        Vector3 pos = spPlayer ? spPlayer.transform.position : Vector3.zero;
        Quaternion rot = spPlayer ? spPlayer.transform.rotation : Quaternion.identity;
        PhotonNetwork.Instantiate(playerPrefabPath, pos, rot);
    }

    void SpawnEnemies(int seed)
    {
        if (!PhotonNetwork.IsMasterClient) return; // evitar duplicados

        var enemyPoints = FindObjectsOfType<SpawnPoint>()
                          .Where(sp => sp.type == SpawnPointType.Enemy)
                          .ToList();                                                   // :contentReference[oaicite:3]{index=3}

        if (enemyPoints.Count == 0 || enemyPrefabPaths == null || enemyPrefabPaths.Length == 0)
            return;

        // RNG determinista (mismo layout para todos)
        System.Random rng = new System.Random(seed ^ 0x51F00D);

        // Si quieres “uno por SP”, usa: int count = Mathf.Min(totalEnemies, enemyPoints.Count);
        int count = totalEnemies;

        for (int i = 0; i < count; i++)
        {
            var sp = enemyPoints[i % enemyPoints.Count];
            string path = enemyPrefabPaths[rng.Next(enemyPrefabPaths.Length)];
            Vector3 p = sp.transform.position + Vector3.up * enemySpawnYOffset;
            Quaternion r = sp.transform.rotation;
            PhotonNetwork.Instantiate(path, p, r); // owner: Master; otros sólo observan
        }
    }
}
