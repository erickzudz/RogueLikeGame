using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using TMPro;

public class GameManager : MonoBehaviourPunCallbacks
{
    public TMP_Text textIndicator;

    // Mensaje de inicio
    private void Start()
    {
        if (!PhotonNetwork.IsConnected)
            PhotonNetwork.ConnectUsingSettings();
    }

    // Cuando se conecta correctamente a Photon
    public override void OnConnected()
    {
        base.OnConnected();
        Debug.Log("Conectados a Photon");
        textIndicator.text = "Conectados correctamente...";
    }
}
