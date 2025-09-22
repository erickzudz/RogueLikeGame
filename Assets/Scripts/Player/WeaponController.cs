using Photon.Pun;
using UnityEngine;

public class WeaponController : MonoBehaviour
{
    public Transform firePoint;
    public string bulletResourcePath = "Bullets/PlayerBullet"; // Resources/Bullets/PlayerBullet.prefab
    public float fireRate = 6f;
    public KeyCode altFireKey = KeyCode.Space;

    float cd;
    PhotonView pv;

    void Awake() { pv = GetComponent<PhotonView>(); }

    void Update()
    {
        if (pv && !pv.IsMine) return;

        cd -= Time.deltaTime;
        bool shoot = Input.GetMouseButton(0) || Input.GetKey(altFireKey);
        if (shoot && cd <= 0f)
        {
            cd = 1f / Mathf.Max(0.01f, fireRate);
            PhotonNetwork.Instantiate(bulletResourcePath, firePoint.position, firePoint.rotation);
        }
    }
}
