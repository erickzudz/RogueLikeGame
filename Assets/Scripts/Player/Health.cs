using Photon.Pun;
using UnityEngine;
using UnityEngine.Events;

public enum HealthAuthority { Owner, Master }

[DisallowMultipleComponent]
public class Health : MonoBehaviourPun, IPunObservable
{
    [Header("Config")]
    public HealthAuthority authority = HealthAuthority.Owner; // Player=Owner, Enemy/Boss=Master
    public float maxHP = 100f;

    [Header("Runtime")]
    [SerializeField] private float hp;
    [SerializeField] private bool isDead;

    [Header("Events")]
    public UnityEvent<float, float> onHealthChanged; // (hp, maxHP)
    public UnityEvent onDeath;

    void Awake() => hp = Mathf.Max(1f, maxHP);

    bool HasAuthority() =>
        authority == HealthAuthority.Owner ? photonView.IsMine : PhotonNetwork.IsMasterClient;

    public bool IsDead => isDead;
    public float CurrentHP => hp;

    // Llamar desde balas, golpes, etc.
    public void RequestDamage(float dmg, int attackerViewId = -1)
    {
        if (authority == HealthAuthority.Owner)
            photonView.RPC(nameof(RPC_ApplyDamage), photonView.Owner, dmg, attackerViewId);
        else
            photonView.RPC(nameof(RPC_ApplyDamage), RpcTarget.MasterClient, dmg, attackerViewId);
    }

    [PunRPC]
    void RPC_ApplyDamage(float dmg, int attackerViewId)
    {
        if (!HasAuthority() || isDead) return;

        hp = Mathf.Max(0f, hp - Mathf.Max(0f, dmg));
        // sincroniza HP a todos
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.Others, hp);

        onHealthChanged?.Invoke(hp, maxHP);

        if (hp <= 0f && !isDead)
        {
            isDead = true;
            onDeath?.Invoke(); // dispara en autoridad
            photonView.RPC(nameof(RPC_RemoteDeath), RpcTarget.Others);
        }
    }

    [PunRPC]
    void RPC_SyncHP(float newHp)
    {
        hp = newHp;
        onHealthChanged?.Invoke(hp, maxHP);
    }

    [PunRPC]
    void RPC_RemoteDeath()
    {
        if (isDead) return;
        isDead = true;
        onDeath?.Invoke();
    }

    public void ResetHPFull()
    {
        if (!HasAuthority()) return;
        isDead = false;
        hp = maxHP;
        photonView.RPC(nameof(RPC_SyncHP), RpcTarget.Others, hp);
        photonView.RPC(nameof(RPC_SetDead), RpcTarget.All, false);
        onHealthChanged?.Invoke(hp, maxHP);
    }
    public void TakeDamage(float dmg)
    {
        // Compatibilidad con código viejo
        RequestDamage(dmg, -1);
    }

    [PunRPC] void RPC_SetDead(bool dead) => isDead = dead;

    // Para late-joiners, sincroniza hp en el stream
    public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        if (stream.IsWriting) { stream.SendNext(hp); stream.SendNext(isDead); }
        else
        {
            float rhp = (float)stream.ReceiveNext();
            bool rdead = (bool)stream.ReceiveNext();
            bool hpChanged = Mathf.Abs(rhp - hp) > 0.001f;
            hp = rhp; isDead = rdead;
            if (hpChanged) onHealthChanged?.Invoke(hp, maxHP);
            if (isDead) onDeath?.Invoke();
        }
    }
}
