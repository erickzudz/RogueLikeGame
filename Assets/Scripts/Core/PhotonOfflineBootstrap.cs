// Assets/Scripts/Core/PhotonOfflineBootstrap.cs
using Photon.Pun;
using UnityEngine;

public class PhotonOfflineBootstrap : MonoBehaviourPunCallbacks
{
    [SerializeField] string offlineRoomName = "AR_Offline";

    void Awake()
    {
        // Si ya estás conectado en otra escena, no hagas nada.
        if (PhotonNetwork.IsConnected) return;

        // Activa el modo offline y entra a una "room" local
        PhotonNetwork.OfflineMode = true;
        if (!PhotonNetwork.InRoom)
        {
            Debug.Log("[PUN] OfflineMode ON. Creando sala local…");
            PhotonNetwork.CreateRoom(offlineRoomName);
        }
    }

    public override void OnJoinedRoom()
    {
        Debug.Log($"[PUN] Entró a room offline: {PhotonNetwork.CurrentRoom?.Name}");
    }
}
