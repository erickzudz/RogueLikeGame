using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider))]
public class ExitPortalTrigger : MonoBehaviour
{
    [Tooltip("Evento local cuando el jugador due�o toca el portal")]
    public UnityEvent onLocalPlayerWin;

    void Reset()
    {
        var col = GetComponent<Collider>();
        col.isTrigger = true;
    }

    void OnTriggerEnter(Collider other)
    {
        // �El que entra es un Player controlado por m�?
        var pv = other.GetComponentInParent<PhotonView>();
        if (pv != null && pv.IsMine)
        {
            // Mensaje local (s�lo para el que lo toc�)
            Debug.Log("[Portal] �Ganaste!");
            onLocalPlayerWin?.Invoke();
            // Si quieres cerrar la partida/lobby:
            // PhotonNetwork.LeaveRoom();
        }
    }
}
