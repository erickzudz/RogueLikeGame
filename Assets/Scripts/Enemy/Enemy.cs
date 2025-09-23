using System.Collections;
using UnityEngine;
using Photon.Pun;
// opcional: using UnityEngine.AI;

public enum EnemyType
{
    Normal,
    NormalShoot,
    Kamikaze,
    Sniper,
    Dasher,         // Kamikaze + stun al contacto
    Basher,         // Sniper + balas que stunean
    ShooterSpread,  // cono/burst
    SniperV2        // ráfaga precisa
}

[RequireComponent(typeof(PhotonView))]
public class Enemy : MonoBehaviourPunCallbacks
{
    public EnemyType type = EnemyType.Normal;

    [Header("Stats")]
    public float maxLife = 3f;
    public float speed = 2f;
    public float damage = 1f;                 // daño de bala / contacto

    [Header("Ranges")]
    public float aggroRange = 8f;             // detectar y perseguir
    public float shootRange = 7f;             // rango de disparo
    public float meleeRange = 1.6f;           // rango de contacto

    [Header("Shooting")]
    public float timeBtwShoot = 1.2f;
    public int burstCount = 1;
    public float burstInterval = 0.08f;
    public float spreadAngle = 0f;
    public bool shootWhileChasing = false;
    public bool aimAtTarget = true;
    public float projSpeed = 12f;

    [Header("Behaviour")]
    public bool stopToShoot = true;           // snipers se plantan
    public float kamikazeSpeedMult = 2f;      // kamikaze acelera

    [Header("Refs")]
    public Transform firePoint;
    [Tooltip("Ruta bajo Resources/ (p.ej. Prefabs/EnemyBullet o Prefabs/Bullet)")]
    public string bulletResourcePath = "Prefabs/EnemyBullet";
    public GameObject explosionEffect;

    // internos
    float life, shootTimer;
    Transform target;
    PhotonView pv;
    // opcional NavMesh:
    // public bool useNavMesh = false;
    // NavMeshAgent agent;

    void Reset()
    {
        // valores por defecto seguros (como los presets del Brain)
        ApplyPreset(type);
    }

    void OnValidate() => ApplyPreset(type);

    void ApplyPreset(EnemyType t)
    {
        // defaults
        stopToShoot = false;
        shootWhileChasing = false;
        spreadAngle = 0f;
        burstInterval = 0.08f;
        burstCount = 1;
        projSpeed = 12f;
        timeBtwShoot = 1.2f;
        speed = 2f;
        shootRange = 7f;
        aggroRange = 8f;
        kamikazeSpeedMult = 2f;

        switch (t)
        {
            case EnemyType.Normal:
                break;

            case EnemyType.NormalShoot:
                stopToShoot = false;
                timeBtwShoot = 1.2f;
                burstCount = 1;
                spreadAngle = 0f;
                break;

            case EnemyType.Kamikaze:
                speed = 2.5f;
                kamikazeSpeedMult = 2f;
                break;

            case EnemyType.Sniper:
                stopToShoot = true;
                shootRange = 10f;
                timeBtwShoot = 2.0f;
                projSpeed = 18f;
                break;

            case EnemyType.Dasher:
                speed = 2.4f;
                kamikazeSpeedMult = 2.2f;
                stopToShoot = false;
                break;

            case EnemyType.Basher:
                stopToShoot = true;
                shootRange = 10.5f;
                timeBtwShoot = 1.7f;
                burstCount = 1;
                projSpeed = 18f;
                break;

            case EnemyType.ShooterSpread:
                stopToShoot = false;
                shootRange = 6f;
                timeBtwShoot = 1.4f;
                burstCount = 5;
                burstInterval = 0.07f;
                spreadAngle = 18f;
                projSpeed = 12f;
                speed = 1.8f;
                break;

            case EnemyType.SniperV2:
                stopToShoot = true;
                shootRange = 11f;
                timeBtwShoot = 1.6f;
                burstCount = 3;
                burstInterval = 0.07f;
                spreadAngle = 4f;
                projSpeed = 20f;
                speed = 1.6f;
                break;
        }
    }

    void Start()
    {
        pv = GetComponent<PhotonView>();
        life = maxLife;

        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) target = p.transform;

        // opcional NavMesh:
        // agent = GetComponent<NavMeshAgent>();
        // if (!useNavMesh && agent) agent.enabled = false;
    }

    void Update()
    {
        // IA solo en Master (equivalente al [Server] del Brain)
        if (!PhotonNetwork.IsMasterClient) return;

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
            else return;
        }

        float dist = Vector3.Distance(transform.position, target.position);

        switch (type)
        {
            case EnemyType.Normal:
                if (dist <= aggroRange) MoveTowards(target.position, 1f);
                break;

            case EnemyType.NormalShoot:
                if (dist <= aggroRange) MoveTowards(target.position, 1f);
                break;

            case EnemyType.Kamikaze:
            case EnemyType.Dasher:
                if (dist <= aggroRange) MoveTowards(target.position, kamikazeSpeedMult);
                break;

            case EnemyType.Sniper:
            case EnemyType.SniperV2:
                if (dist > shootRange) MoveTowards(target.position, 1f);
                // si está en rango y es “stopToShoot”, no avanza
                break;

            case EnemyType.Basher:
            case EnemyType.ShooterSpread:
                if (dist <= aggroRange) MoveTowards(target.position, 1f);
                break;
        }

        if (aimAtTarget) FaceTarget();

        bool canShoot =
            (type == EnemyType.NormalShoot && dist <= shootRange) ||
            (type == EnemyType.Basher && dist <= shootRange) ||
            (type == EnemyType.ShooterSpread && dist <= shootRange) ||
            (type == EnemyType.Sniper && dist <= shootRange) ||
            (type == EnemyType.SniperV2 && dist <= shootRange);

        if (canShoot) TryShoot();

        // contacto melee + kamikaze
        if (dist <= meleeRange)
        {
            if (target.TryGetComponent(out Health h)) h.TakeDamage(damage);
            if (type == EnemyType.Kamikaze || type == EnemyType.Dasher)
            {
                if (explosionEffect) Instantiate(explosionEffect, transform.position, transform.rotation);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }

    // -------- helpers --------
    void MoveTowards(Vector3 worldPos, float mult)
    {
        // con NavMesh sería SetDestination; aquí nos movemos directo
        Vector3 dir = worldPos - transform.position; dir.y = 0f;
        if (dir.sqrMagnitude > 0.0001f) dir.Normalize();

        // si es sniper y debe pararse a disparar en rango, no se mueve
        if ((type == EnemyType.Sniper || type == EnemyType.SniperV2) && stopToShoot)
        {
            float dist = Vector3.Distance(transform.position, worldPos);
            if (dist <= shootRange) return;
        }

        transform.position += dir * (speed * mult) * Time.deltaTime;
    }

    void FaceTarget()
    {
        if (!target) return;
        Vector3 dir = target.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude <= 0.0001f) return;
        Quaternion to = Quaternion.LookRotation(dir);
        transform.rotation = Quaternion.Slerp(transform.rotation, to, 15f * Time.deltaTime);
    }

    void TryShoot()
    {
        shootTimer += Time.deltaTime;
        if (shootTimer < timeBtwShoot) return;
        shootTimer = 0f;
        int shots = Mathf.Max(1, burstCount);
        StartCoroutine(ShootBurst(shots));
    }

    IEnumerator ShootBurst(int shots)
    {
        for (int i = 0; i < shots; i++)
        {
            if (aimAtTarget) FaceTarget();

            float offset = (shots > 1)
                ? Mathf.Lerp(-spreadAngle, spreadAngle, i / (float)(shots - 1))
                : 0f;

            Quaternion rot = transform.rotation * Quaternion.Euler(0, offset, 0);
            Vector3 dir = rot * Vector3.forward;
            Vector3 spawnPos = firePoint ? firePoint.position : (transform.position + transform.forward * 0.6f);

            float stunSeconds = (type == EnemyType.Basher) ? 1.0f : 0f;

            // Instanciar bala en red
            PhotonNetwork.Instantiate(
                bulletResourcePath,
                spawnPos,
                Quaternion.LookRotation(dir),
                0,
                new object[] { dir, damage, projSpeed, stunSeconds }
            );

            if (i < shots - 1) yield return new WaitForSeconds(burstInterval);
        }
    }

    public void TakeDamage(float dmg)
    {
        if (!PhotonNetwork.IsMasterClient) return;
        life -= dmg;
        if (life <= 0f)
        {
            if (explosionEffect) Instantiate(explosionEffect, transform.position, transform.rotation);
            PhotonNetwork.Destroy(gameObject);
        }
    }

    void OnCollisionEnter(Collision c)
    {
        // contacto con jugador (por si no usamos meleeRange)
        if (!PhotonNetwork.IsMasterClient) return;
        if (c.gameObject.CompareTag("Player"))
        {
            if (c.gameObject.TryGetComponent(out Health h)) h.TakeDamage(damage);
            if (type == EnemyType.Kamikaze || type == EnemyType.Dasher)
            {
                if (explosionEffect) Instantiate(explosionEffect, transform.position, transform.rotation);
                PhotonNetwork.Destroy(gameObject);
            }
        }
    }
}
