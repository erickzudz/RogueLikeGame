using System.Collections;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public EnemyType type;

    [Header("Stats")]
    public float maxLife = 3f;
    public float speed = 2f;
    public float damage = 1f;

    [Header("Shooting")]
    public float timeBtwShoot = 1.5f;
    public int burstCount = 1;
    public float burstInterval = 0.08f;
    public float spreadAngle = 0f;
    public bool shootWhileChasing = false;
    public bool aimAtTarget = true;

    [Header("Range & Target")]
    public float range = 4f;
    bool targetInRange;
    Transform target;

    [Header("Refs")]
    public Transform firePoint;
    public GameObject bulletPrefab;
    public GameObject explosionEffect;

    float life, timer;

    void Start()
    {
        var p = GameObject.FindGameObjectWithTag("Player");
        if (p) target = p.transform;
        life = maxLife;
    }

    void Update()
    {
        switch (type)
        {
            case EnemyType.Normal: MoveForward(); break;
            case EnemyType.NormalShoot: MoveForward(); Shoot(); break;
            case EnemyType.Kamikaze:
                if (targetInRange) { RotateToTarget(); MoveForward(2f); if (shootWhileChasing) Shoot(); }
                else { MoveForward(); SearchTarget(); }
                break;
            case EnemyType.Sniper:
                if (targetInRange) { RotateToTarget(); Shoot(); }
                else { MoveForward(); SearchTarget(); }
                break;
        }
    }

    public void TakeDamage(float dmg)
    {
        life -= dmg;
        if (life <= 0f)
        {
            if (explosionEffect) Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }

    void MoveForward(float mult = 1f) => transform.Translate(Vector3.forward * speed * mult * Time.deltaTime);

    void RotateToTarget()
    {
        if (!target) return;
        Vector3 dir = target.position - transform.position;
        float y = Mathf.Atan2(dir.x, dir.z) * Mathf.Rad2Deg;
        transform.rotation = Quaternion.Euler(0, y, 0);
    }

    void SearchTarget() { if (target) targetInRange = Vector3.Distance(transform.position, target.position) <= range; }

    void Shoot()
    {
        timer += Time.deltaTime;
        if (timer < timeBtwShoot) return;
        timer = 0f;
        StartCoroutine(ShootBurst());
    }

    IEnumerator ShootBurst()
    {
        int shots = Mathf.Max(1, burstCount);
        for (int i = 0; i < shots; i++)
        {
            if (aimAtTarget) RotateToTarget();
            float offset = (shots > 1) ? Mathf.Lerp(-spreadAngle, spreadAngle, i / (float)(shots - 1)) : 0f;
            Quaternion rot = transform.rotation * Quaternion.Euler(0, offset, 0);
            if (bulletPrefab && firePoint) Instantiate(bulletPrefab, firePoint.position, rot);
            if (i < shots - 1) yield return new WaitForSeconds(burstInterval);
        }
    }

    void OnCollisionEnter(Collision c)
    {
        if (c.gameObject.CompareTag("Player"))
        {
            var h = c.gameObject.GetComponent<Health>();
            if (h) h.TakeDamage(damage);
            if (explosionEffect) Instantiate(explosionEffect, transform.position, transform.rotation);
            Destroy(gameObject);
        }
    }
}

public enum EnemyType { Normal, NormalShoot, Kamikaze, Sniper }
