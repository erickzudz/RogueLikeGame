using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class BossDeathHandler : MonoBehaviourPun
{
    void OnEnable() => GetComponent<Health>().onDeath.AddListener(OnDeath);
    void OnDisable() => GetComponent<Health>().onDeath.RemoveListener(OnDeath);

    void OnDeath()
    {
        // Fases / cinem�tica / abrir puerta final
        photonView.RPC(nameof(RPC_BossDefeated), RpcTarget.All);
    }

    [PunRPC]
    void RPC_BossDefeated()
    {
        // efectos locales (UI, m�sica, etc.)
    }
}
