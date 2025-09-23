using Photon.Pun;
using UnityEngine;

public class StunReceiver : MonoBehaviourPun
{
    // Usamos el reloj de Photon (en segundos, double)
    private double stunUntil;

    public bool IsStunned => PhotonNetwork.Time < stunUntil;

    /// <summary>Aplica un stun desde el "árbitro" (MasterClient) o desde el propio dueño.</summary>
    public void ApplyStun(float seconds)
    {
        if (seconds <= 0f) return;

        // Elegimos la marca de tiempo nueva
        double newUntil = PhotonNetwork.Time + seconds;

        // Sincroniza a todos; cada cliente conserva el mayor valor (evita bajar la duración)
        photonView.RPC(nameof(RPC_SetStunUntil), RpcTarget.All, newUntil);
    }

    [PunRPC]
    void RPC_SetStunUntil(double newUntil)
    {
        if (newUntil > stunUntil) stunUntil = newUntil;

        // (Opcional) feedback local: VFX/UI, cambiar color, etc.
        // Debug.Log($"Stunned { (stunUntil-PhotonNetwork.Time):0.00}s");
    }
}
