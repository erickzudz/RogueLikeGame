using Photon.Pun;
using UnityEngine;

public class PlayerSpawnerPhoton : MonoBehaviour
{
    [SerializeField] string playerPrefabName = "Player/Player"; // Resources/Player/Player.prefab
    [SerializeField] Transform[] spawnPoints;

    void Start()
    {
        if (!PhotonNetwork.InRoom) return;
        Transform p = spawnPoints[Random.Range(0, spawnPoints.Length)];
        PhotonNetwork.Instantiate(playerPrefabName, p.position, p.rotation);
    }
}
