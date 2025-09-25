using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class EnemyDeathHandler : MonoBehaviourPun
{
    Health health;

    void Awake() => health = GetComponent<Health>();
    void OnEnable() => health.onDeath.AddListener(OnDeath);
    void OnDisable() => health.onDeath.RemoveListener(OnDeath);

    void OnDeath()
    {
        if (!PhotonNetwork.IsMasterClient) return; // evita duplicados
        // TODO: drop loot / sumar score / efectos
        PhotonNetwork.Destroy(gameObject);
    }
}
