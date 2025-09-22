using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

public class NetworkBootstrap : MonoBehaviourPunCallbacks
{
    [SerializeField] string gameVersion = "0.1";
    [SerializeField] string roomName = "room-1";
    [SerializeField] byte maxPlayers = 4;

    void Start()
    {
        PhotonNetwork.AutomaticallySyncScene = true;
        PhotonNetwork.GameVersion = gameVersion;
        PhotonNetwork.ConnectUsingSettings();
        Debug.Log("[PUN] Connecting...");
    }

    public override void OnConnectedToMaster()
    {
        var opts = new RoomOptions { MaxPlayers = maxPlayers };
        PhotonNetwork.JoinOrCreateRoom(roomName, opts, TypedLobby.Default);
        Debug.Log("[PUN] Connected. Joining room...");
    }

    public override void OnJoinedRoom()
    {
        Debug.Log("[PUN] Joined room!");
        // el Player se spawnea en PlayerSpawnerPhoton
    }
}
