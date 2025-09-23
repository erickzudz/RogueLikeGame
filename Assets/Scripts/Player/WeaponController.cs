using UnityEngine;
using Photon.Pun;

public class WeaponController : MonoBehaviourPun
{
    [SerializeField] Transform firePoint;
    [SerializeField] string bulletResourcePath = "Prefabs/Bullet";

    [Header("Stats")]
    [SerializeField] float baseDamage = 1f;
    [SerializeField] float bulletSpeed = 12f;
    [SerializeField] float fireRate = 6f;     // disparos por segundo
    [SerializeField] float critChance = 0.1f; // 10%
    [SerializeField] float stunOnHit = 0f;    // opcional

    float nextFire;

    void Update()
    {
        if (!photonView.IsMine) return;
        if (Time.time < nextFire) return;

        // lee EN UPDATE (siempre actualizado)
        bool fireHeldMobile = MobileHUD.I && MobileHUD.I.fireBtn && MobileHUD.I.fireBtn.IsHeld;
        bool fireHeldMouse = Input.GetMouseButton(0);

        if (fireHeldMobile || fireHeldMouse)
        {
            nextFire = Time.time + (1f / fireRate);

            Vector3 dir = (firePoint ? firePoint.forward : transform.forward).normalized;
            Vector3 pos = firePoint ? firePoint.position : (transform.position + transform.forward * 0.6f);
            float dmg = (Random.value <= critChance) ? baseDamage * 2f : baseDamage;

            PhotonNetwork.Instantiate(
                bulletResourcePath,
                pos,
                Quaternion.LookRotation(dir),
                0,
                new object[] { dir, dmg, bulletSpeed, stunOnHit }
            );
        }
    }
}
