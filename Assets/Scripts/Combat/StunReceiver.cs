using Photon.Pun;
using UnityEngine;

public class StunReceiver : MonoBehaviourPun
{
    double stunUntil; // tiempo absoluto (seg) en reloj de juego local
    public bool IsStunned => Time.timeAsDouble < stunUntil;

    public void ApplyStun(float seconds)
    {
        if (seconds <= 0f) return;

        // El dueño actual decide y replica a todos (buffered)
        if (photonView.IsMine)
        {
            double newUntil = Mathf.Max((float)stunUntil, (float)(Time.timeAsDouble + seconds));
            photonView.RPC(nameof(RPC_SetStun), RpcTarget.AllBuffered, newUntil);
        }
        else
        {
            // si no somos el dueño, pedimos por RPC al Owner (evita race)
            photonView.RPC(nameof(RPC_RequestStun), photonView.Owner, seconds);
        }
    }

    [PunRPC]
    void RPC_RequestStun(float seconds)
    {
        double newUntil = Mathf.Max((float)stunUntil, (float)(Time.timeAsDouble + seconds));
        photonView.RPC(nameof(RPC_SetStun), RpcTarget.AllBuffered, newUntil);
    }

    [PunRPC]
    void RPC_SetStun(double until)
    {
        stunUntil = until;
        // aquí podrías disparar VFX/UI de stun si quieres
        // Debug.Log($"Stunned {(stunUntil - Time.timeAsDouble):0.00}s");
    }
}
