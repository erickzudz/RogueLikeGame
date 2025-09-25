using Photon.Pun;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(PhotonView))]
public class PlayerRespawn : MonoBehaviourPun
{
    public float respawnDelay = 2f;

    Vector3 spawnPos;
    Quaternion spawnRot;

    CharacterController cc;
    Rigidbody rb;
    Renderer[] rends;
    Collider[] cols;
    Health health; // <-- NUEVO

    void Awake()
    {
        cc = GetComponent<CharacterController>();
        rb = GetComponent<Rigidbody>();
        health = GetComponent<Health>(); // <-- NUEVO
        rends = GetComponentsInChildren<Renderer>(true);
        cols = GetComponentsInChildren<Collider>(true);
    }

    void Start()
    {
        if (photonView.IsMine)
        {
            spawnPos = transform.position;
            spawnRot = transform.rotation;
        }
    }

    public void Die()
    {
        if (!photonView.IsMine) return;
        StartCoroutine(CoRespawn());
    }

    IEnumerator CoRespawn()
    {
        photonView.RPC(nameof(RPC_SetAlive), RpcTarget.All, false);

        yield return new WaitForSeconds(respawnDelay);

        if (cc) cc.enabled = false;
        transform.SetPositionAndRotation(spawnPos, spawnRot);
        if (rb) { rb.velocity = Vector3.zero; rb.angularVelocity = Vector3.zero; }
        if (cc) cc.enabled = true;

        // *** RESET VIDA Y ESTADO ***
        health?.ResetHPFull();   // hp = maxHP y isDead = false (sincroniza a todos)

        photonView.RPC(nameof(RPC_SetAlive), RpcTarget.All, true);
    }

    [PunRPC]
    void RPC_SetAlive(bool alive)
    {
        foreach (var r in rends) r.enabled = alive;
        foreach (var c in cols) c.enabled = alive;
    }
}
