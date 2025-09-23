using Photon.Pun;
using UnityEngine;

public class SimpleTopDownCamPUN : MonoBehaviour
{
    [Header("Offset respecto al jugador")]
    public Vector3 offset = new Vector3(0f, 12f, -8f);

    [Header("Auto-buscar a mi propio player")]
    public bool autoFindLocalPlayer = true;

    [Tooltip("Si lo dejas vac�o y autoFind est� ON, se asigna solo")]
    public Transform target;

    void LateUpdate()
    {
        // 1) Encontrar TU jugador local una vez
        if (autoFindLocalPlayer && !target)
        {
            foreach (var pv in FindObjectsOfType<PhotonView>())
            {
                // Asume que el prefab del player tiene Tag = "Player"
                if (pv.IsMine && pv.CompareTag("Player"))
                {
                    target = pv.transform;
                    break;
                }
            }
            if (!target) return; // a�n no existe tu player
        }

        // 2) Colocar y orientar la c�mara igual que en tu script Mirror
        Vector3 camPos = target.position + offset;
        transform.position = camPos;

        // Mira directamente al jugador (top-down cl�sico)
        transform.rotation = Quaternion.LookRotation(target.position - transform.position, Vector3.up);
    }
}
