using UnityEngine;
using UnityEngine.AI;

[DisallowMultipleComponent]
public class EnemyAnimatorBridge : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Animator anim;          // Animator del modelo
    [SerializeField] NavMeshAgent agent;     // opcional
    [SerializeField] CharacterController cc; // opcional

    [Header("Tuning")]
    public float runSpeedForOne = 3.5f;      // velocidad que equivale a Speed=1

    // Hashes
    static readonly int H_SPEED = Animator.StringToHash("Speed");
    static readonly int H_HIT = Animator.StringToHash("Hit");
    static readonly int H_SHOOT = Animator.StringToHash("Shoot");
    static readonly int H_DIE = Animator.StringToHash("Die");
    static readonly int H_ISATTACKING = Animator.StringToHash("IsAttacking");

    void Reset()
    {
        anim = GetComponentInChildren<Animator>(true);
        agent = GetComponent<NavMeshAgent>();
        cc = GetComponent<CharacterController>();
    }

    void Awake()
    {
        if (!anim) anim = GetComponentInChildren<Animator>(true);
        if (!agent) agent = GetComponent<NavMeshAgent>();
        if (!cc) cc = GetComponent<CharacterController>();
    }

    void Update()
    {
        if (!anim) return;

        float speed = 0f;
        if (agent) speed = agent.velocity.magnitude;
        else if (cc) speed = cc.velocity.magnitude;
        else speed = 0f;

        float blend = runSpeedForOne > 0 ? Mathf.Clamp01(speed / runSpeedForOne) : 0f;
        anim.SetFloat(H_SPEED, blend);
    }

    // —— Llamadas públicas desde tu lógica de Enemy ——
    public void PlayHit() { if (anim) anim.SetTrigger(H_HIT); }
    public void PlayShoot() { if (anim) anim.SetTrigger(H_SHOOT); }
    public void PlayDie() { if (anim) anim.SetTrigger(H_DIE); }
    public void SetAttacking(bool v) { if (anim) anim.SetBool(H_ISATTACKING, v); }
}
