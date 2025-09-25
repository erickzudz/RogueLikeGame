using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;  

public class Launcher : MonoBehaviourPunCallbacks
{
    public PhotonView playerPrefab;
    public Transform spwanPoint;
    public PhotonView mapa;
    void Start()
    {
        PhotonNetwork.ConnectUsingSettings();
    }

    // Update is called once per frame
    public override void OnConnectedToMaster()
    {
        Debug.Log("Coenctados al master");
        PhotonNetwork.JoinRandomOrCreateRoom();
    }

    public override void OnJoinedRoom()
    {
        PhotonNetwork.Instantiate(playerPrefab.name,spwanPoint.position,spwanPoint.rotation);
    }

}
