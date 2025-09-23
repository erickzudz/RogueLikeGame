using UnityEngine;
using Photon.Pun;

[DisallowMultipleComponent]
public class NetworkBullet : MonoBehaviour, IPunInstantiateMagicCallback
{
    [Header("Base")]
    public float Speed = 12f;
    public float Life = 3f;
    public float Damage = 1f;

    [Header("Target filter")]
    public LayerMask targetMask;   // Asignar en el Inspector (p.ej. Enemy para balas del jugador)

    [Header("CC (opcional)")]
    public float stunSeconds = 0f;

    // datos que llegan desde el shooter
    Vector3 dir = Vector3.zero;
    float lifeLeft;

    // === PUN instantiate ===
    public void OnPhotonInstantiate(PhotonMessageInfo info)
    {
        object[] data = info.photonView.InstantiationData;
        if (data != null)
        {
            if (data.Length > 0) dir = (Vector3)data[0];
            if (data.Length > 1) Damage = (float)data[1];
            if (data.Length > 2) Speed = (float)data[2];
            if (data.Length > 3) stunSeconds = (float)data[3];
        }

        if (dir == Vector3.zero) dir = transform.forward; // fallback
        lifeLeft = Life;
    }

    void Update()
    {
        transform.position += dir * Speed * Time.deltaTime;

        lifeLeft -= Time.deltaTime;
        if (lifeLeft <= 0f) Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        // filtra por capas
        if ((targetMask.value & (1 << other.gameObject.layer)) == 0) return;

        // --- Daño ---
        Health victim = null;
        if (!other.TryGetComponent(out victim))
            victim = other.GetComponentInParent<Health>();

        if (victim != null)
            victim.TakeDamage(Damage);

        // --- Stun opcional ---
        if (stunSeconds > 0f)
        {
            StunReceiver sr = null;
            if (!other.TryGetComponent(out sr))
                sr = other.GetComponentInParent<StunReceiver>();

            if (sr != null)
                sr.ApplyStun(stunSeconds);
        }

        Destroy(gameObject);
    }
}
