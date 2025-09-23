using UnityEngine;
using Photon.Pun;
using System.Collections;

public class TestPlayModeBootstrap : MonoBehaviourPunCallbacks
{
    [Header("Prefabs (Resources paths)")]
   public string playerPrefabPath = "Prefabs/Player";
public string enemyPrefabPath  = "Prefabs/Shotgun";


    [Header("Player Spawn")]
    public Vector3 playerSpawnPos = new Vector3(0f, 1f, 0f);

    [Header("Enemy Spawner")]
    public int initialEnemies = 3;
    public float arenaRadius = 10f;

    void Start()
    {
        // 1) Activar modo offline ANTES de nada
        PhotonNetwork.OfflineMode = true;

        // 2) Simular como si hubiéramos entrado a una sala
        PhotonNetwork.CreateRoom("OfflineRoom");
        OnJoinedRoom();
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("Offline room joined, spawning player + enemies...");

        // Spawn Player
        PhotonNetwork.Instantiate(playerPrefabPath, playerSpawnPos, Quaternion.identity);

        // Spawn enemigos iniciales
        for (int i = 0; i < initialEnemies; i++)
        {
            Vector2 circle = Random.insideUnitCircle * arenaRadius;
            Vector3 pos = new Vector3(circle.x, 0.6f, circle.y);
            PhotonNetwork.Instantiate(enemyPrefabPath, pos, Quaternion.identity);
        }
    }
}
