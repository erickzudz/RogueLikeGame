using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(PhotonView))]
public class PlayerNetwork : MonoBehaviourPun
{
    [SerializeField] MonoBehaviour[] localOnly; // arrastra PlayerMovement, WeaponController, etc.

    void Start()
    {
        bool isMine = photonView.IsMine || PhotonNetwork.OfflineMode;
        foreach (var c in localOnly) if (c) c.enabled = isMine;
        // opcional: capa diferente para remotos
        // if(!isMine) gameObject.layer = LayerMask.NameToLayer("Enemy");
    }
}
