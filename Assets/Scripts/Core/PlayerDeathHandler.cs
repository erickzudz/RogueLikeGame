using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class PlayerDeathHandler : MonoBehaviourPun
{
    Health health;
    PlayerRespawn respawn;

    void Awake()
    {
        health = GetComponent<Health>();
        respawn = GetComponent<PlayerRespawn>();
    }

    void OnEnable() => health.onDeath.AddListener(OnDeath);
    void OnDisable() => health.onDeath.RemoveListener(OnDeath);

    void OnDeath()
    {
        if (!photonView.IsMine) return; // sólo el dueño respawnea su player
        if (respawn != null) respawn.Die(); // tu corrutina que teletransporta al spawn y reactiva
    }
}
