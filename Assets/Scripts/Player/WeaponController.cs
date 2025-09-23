using UnityEngine;
using Photon.Pun;

public class WeaponController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] Transform firePoint;
    [SerializeField] string bulletResourcePath = "Prefabs/Bullet"; // ruta en Resources

    [Header("Stats")]
    [SerializeField] float baseDamage = 1f;
    [SerializeField] float bulletSpeed = 14f;
    [SerializeField] float fireRate = 4f;     // disparos/seg
    [SerializeField] float critChance = 0.1f; // 10%

    StunReceiver stun;
    PhotonView pv;
    float nextFire;

    void Awake()
    {
        pv = GetComponent<PhotonView>();
        stun = GetComponent<StunReceiver>();
        if (!firePoint) firePoint = transform;
    }

    void Update()
    {
        // solo el dueño procesa input
        if (pv && !pv.IsMine) return;

        if (stun != null && stun.IsStunned) return;
        if (Time.time < nextFire) return;

        // LEFT CLICK (Mouse0). Si prefieres InputActionsWrapper, puedes OR con él.
        bool firePressed = Input.GetMouseButton(0);
        if (firePressed)
        {
            nextFire = Time.time + 1f / Mathf.Max(0.01f, fireRate);

            Vector3 pos = firePoint ? firePoint.position : transform.position + transform.forward * 0.6f;
            Vector3 dir = firePoint ? firePoint.forward : transform.forward;

            // Calcula daño (crítico) en el owner y lo manda a todos
            float dmg = (Random.value <= critChance) ? baseDamage * 2f : baseDamage;

            // PUN: pasa datos en InstantiationData para que TODOS los clientes inicialicen la bala igual
            object[] data = new object[] { dir, dmg, bulletSpeed };

            PhotonNetwork.Instantiate(bulletResourcePath, pos, Quaternion.LookRotation(dir), 0, data);
        }
    }
}
