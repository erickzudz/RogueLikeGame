using System.Collections;
using System.Collections.Generic;
using Photon.Pun;
using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhotonView))]
public class BossController : MonoBehaviourPun
{
    [Header("Movement")]
    public float moveSpeed = 3f;
    public float stopDistance = 6f;
    public float turnSpeed = 720f;

    [Header("Brain")]
    public float baseCooldown = 1.25f;      // pausa entre ataques
    public bool phase2 = true;              // 2 ataques por ronda si está activo

    [Header("Refs")]
    public Transform firePointsRoot;        // padre con FP0..FPn (opcional)
    public Transform laserPivot;            // pivote que rota el láser
    public LineRenderer laserLine;          // LineRenderer del hijo "Laser"
    public LayerMask laserHitMask = ~0;     // incluye Player

    [Header("Laser")]
    public float laserLength = 40f;
    public float laserDPS = 10f;            // daño por segundo
    public float laserTurnSpeed = 360f;     // qué tan rápido “persigue” al player
    public float laserFireTime = 2.5f;      // duración del láser encendido

    [Header("Toggle attacks")]
    public bool useCone = true;
    public bool useRadial = true;
    public bool useLaser = true;
    public bool useSummon = false;          // Slam eliminado

    [Header("Cone (shotgun)")]
    public string enemyBulletPath = "Prefabs/EnemyBullet";
    public int coneBullets = 5;
    public float coneSpread = 24f;          // grados totales
    public float coneBulletSpeed = 14f;
    public float coneDamage = 1f;

    [Header("Radial")]
    public int radialBullets = 16;
    public float radialBulletSpeed = 12f;
    public float radialDamage = 1f;

    [Header("Summon")]
    [Tooltip("Lista de rutas (Resources/) de minions posibles. Se elige al azar por cada invocación.")]
    public string[] minionPaths;            // ej: "Prefabs/Basher", "Prefabs/SniperV2"
    public int summonCount = 3;         // cuántos en total
    public float summonRadius = 5f;

    [Header("Telegraph (opcional)")]
    public float telegraphTime = 0.35f;
    public Renderer telegraphRenderer;

    // internos
    Transform target;
    PhotonView pv;
    readonly List<Transform> firePoints = new();
    bool busy;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        if (firePointsRoot)
        {
            firePoints.Clear();
            foreach (Transform t in firePointsRoot)
                firePoints.Add(t);
        }
        if (laserLine) { laserLine.enabled = false; laserLine.positionCount = 2; }
    }

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(CoBrain());
    }

    void Update()
    {
        if (!PhotonNetwork.IsMasterClient) return;

        if (!target)
        {
            var p = GameObject.FindGameObjectWithTag("Player");
            if (p) target = p.transform;
        }

        // Movimiento básico
        if (target)
        {
            Vector3 to = target.position - transform.position; to.y = 0;
            float dist = to.magnitude;

            if (dist > stopDistance)
            {
                Vector3 dir = to / Mathf.Max(dist, 0.0001f);
                transform.position += dir * moveSpeed * Time.deltaTime;
            }

            if (to.sqrMagnitude > 0.0001f)
            {
                var look = Quaternion.LookRotation(to);
                transform.rotation = Quaternion.RotateTowards(transform.rotation, look, turnSpeed * Time.deltaTime);
            }
        }
    }

    // ================== BRAIN ==================
    IEnumerator CoBrain()
    {
        var wait = new WaitForSeconds(baseCooldown);

        while (true)
        {
            // Construir lista de ataques habilitados
            var allowed = new List<System.Func<IEnumerator>>();
            if (useCone) allowed.Add(CoCone);
            if (useRadial) allowed.Add(CoRadial);
            if (useLaser) allowed.Add(CoLaserLock);
            if (useSummon) allowed.Add(CoSummon);

            if (allowed.Count == 0) { yield return wait; continue; }

            // Telegraph opcional
            yield return Telegraph(telegraphTime);

            int attacksThisRound = phase2 ? Mathf.Min(2, allowed.Count) : 1;
            for (int i = 0; i < attacksThisRound; i++)
            {
                var pick = allowed[Random.Range(0, allowed.Count)];
                yield return pick();
                if (phase2 && i == 0) yield return new WaitForSeconds(0.25f);
            }

            yield return wait;
        }
    }

    IEnumerator Telegraph(float dur)
    {
        if (!telegraphRenderer) { yield return new WaitForSeconds(dur); yield break; }

        float t = 0f;
        Color baseC = telegraphRenderer.material.color;
        while (t < dur)
        {
            t += Time.deltaTime;
            float a = Mathf.PingPong(t * 8f, 1f);
            var c = baseC; c.a = Mathf.Lerp(0.25f, 0.85f, a);
            telegraphRenderer.material.color = c;
            yield return null;
        }
        telegraphRenderer.material.color = baseC;
    }

    // ================== ATTACKS ==================

    // 1) Escopeta hacia el jugador
    IEnumerator CoCone()
    {
        if (!target) yield break;

        Vector3 origin = firePoints.Count > 0
            ? firePoints[0].position
            : transform.position + transform.forward * 0.6f;

        Vector3 fwd = (target.position - origin); fwd.y = 0f;
        if (fwd.sqrMagnitude < 0.0001f) fwd = transform.forward; else fwd.Normalize();

        int n = Mathf.Max(1, coneBullets);
        for (int i = 0; i < n; i++)
        {
            float t = (n == 1) ? 0f : i / (float)(n - 1);
            float ang = Mathf.Lerp(-coneSpread * 0.5f, coneSpread * 0.5f, t);
            Quaternion rot = Quaternion.AngleAxis(ang, Vector3.up) * Quaternion.LookRotation(fwd, Vector3.up);
            SpawnBullet(origin, rot * Vector3.forward, coneDamage, coneBulletSpeed);
        }

        yield return null;
    }

    // 2) Radial (bullet hell)
    IEnumerator CoRadial()
    {
        Vector3 origin = transform.position + transform.forward * 0.6f;
        int n = Mathf.Max(3, radialBullets);
        for (int i = 0; i < n; i++)
        {
            float ang = (360f / n) * i;
            Quaternion rot = Quaternion.Euler(0f, ang, 0f);
            SpawnBullet(origin, rot * Vector3.forward, radialDamage, radialBulletSpeed);
        }
        yield return null;
    }

    // 3) Láser que se “pega” al jugador mientras dura (tu versión)
    IEnumerator CoLaserLock()
    {
        if (!laserPivot || !laserLine) yield break;

        busy = true;
        laserLine.enabled = true;

        float t = 0f;
        while (t < laserFireTime)
        {
            t += Time.deltaTime;

            // Alinear pivot al jugador continuamente
            if (target)
            {
                Vector3 look = target.position; look.y = laserPivot.position.y;
                Vector3 to = (look - laserPivot.position);
                if (to.sqrMagnitude > 0.0001f)
                {
                    var toRot = Quaternion.LookRotation(to);
                    laserPivot.rotation = Quaternion.RotateTowards(
                        laserPivot.rotation, toRot, laserTurnSpeed * Time.deltaTime);
                }
            }

            // Raycast + daño y ajustar punto final del LineRenderer
            Vector3 p0 = laserPivot.position;
            Vector3 dir = laserPivot.forward;
            Vector3 p1 = p0 + dir * laserLength;

            if (Physics.Raycast(p0, dir, out RaycastHit hit, laserLength, laserHitMask, QueryTriggerInteraction.Ignore))
            {
                p1 = hit.point;

                if (hit.collider.TryGetComponent(out Health hp))
                    hp.TakeDamage(laserDPS * Time.deltaTime);
                else
                {
                    var hpParent = hit.collider.GetComponentInParent<Health>();
                    if (hpParent) hpParent.TakeDamage(laserDPS * Time.deltaTime);
                }
            }

            laserLine.SetPosition(0, p0);
            laserLine.SetPosition(1, p1);

            yield return null;
        }

        laserLine.enabled = false;
        busy = false;
    }

    // 4) Summon aleatorio a partir de una lista de paths
    IEnumerator CoSummon()
    {
        if (minionPaths == null || minionPaths.Length == 0 || summonCount <= 0) yield break;

        int total = Mathf.Clamp(summonCount, 1, 32);
        for (int i = 0; i < total; i++)
        {
            string pick = minionPaths[Random.Range(0, minionPaths.Length)];
            if (string.IsNullOrEmpty(pick)) continue;

            Vector2 r = Random.insideUnitCircle * summonRadius;
            Vector3 pos = transform.position + new Vector3(r.x, 0, r.y);
            PhotonNetwork.Instantiate(pick, pos, Quaternion.identity);

            yield return new WaitForSeconds(0.08f);
        }
    }

    // ================== HELPERS ==================
    void SpawnBullet(Vector3 pos, Vector3 dir, float damage, float speed)
    {
        object[] data = new object[] { dir.normalized, damage, speed, 0f };
        PhotonNetwork.Instantiate(enemyBulletPath, pos, Quaternion.LookRotation(dir), 0, data);
    }
}
