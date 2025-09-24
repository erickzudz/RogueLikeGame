using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class PlayerAnimatorBridgePhoton : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;                 // Animator del modelo (hijo visual)
    [SerializeField] CharacterController cc;        // Para leer la velocidad real
    [SerializeField] StunReceiver stun;             // Opcional (si usas stun)
    PhotonView pv;

    [Header("Tuning")]
    [Tooltip("Velocidad a la que Speed llega a 1.0 (≈ a tu MoveSpeed).")]
    public float speedNormalize = 6f;

    // Hashes de parámetros del Animator
    static readonly int HASH_SPEED = Animator.StringToHash("Speed");
    static readonly int HASH_ISSTUNNED = Animator.StringToHash("IsStunned");
    static readonly int HASH_HIT = Animator.StringToHash("Hit");
    static readonly int HASH_DIE = Animator.StringToHash("Die");
    static readonly int HASH_SHOOT = Animator.StringToHash("Shoot");

    void Reset()
    {
        anim = GetComponentInChildren<Animator>(true);
        cc = GetComponent<CharacterController>();
        stun = GetComponent<StunReceiver>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!cc) cc = GetComponent<CharacterController>();
        if (!stun) stun = GetComponent<StunReceiver>();
        pv = GetComponent<PhotonView>();
    }

    void Update()
    {
        // Solo el propietario escribe los parámetros
        if (pv != null && !pv.IsMine) return;
        if (!anim) return;

        // 1) SPEED (Idle/Run)
        float spd = cc ? cc.velocity.magnitude : 0f;
        float blend = speedNormalize > 0 ? Mathf.Clamp01(spd / speedNormalize) : spd;
        anim.SetFloat(HASH_SPEED, blend);

        // 2) STUN (opcional)
        bool isStunned = stun && stun.IsStunned;
        anim.SetBool(HASH_ISSTUNNED, isStunned);
    }

    // ---- Triggers públicos (llámalos localmente) ----
    public void LocalTriggerShoot() { if (anim) anim.SetTrigger(HASH_SHOOT); }
    public void LocalTriggerHit() { if (anim) anim.SetTrigger(HASH_HIT); }
    public void LocalTriggerDie() { if (anim) anim.SetTrigger(HASH_DIE); }
}
