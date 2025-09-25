using UnityEngine;

public enum SpawnPointType { Player, Exit, Enemy }

public class SpawnPoint : MonoBehaviour
{
    public SpawnPointType type;
    public float radius = 0.5f;

    void OnDrawGizmos()
    {
        Gizmos.color = type switch
        {
            SpawnPointType.Player => Color.green,
            SpawnPointType.Exit => Color.cyan,
            _ => Color.red
        };
        Gizmos.DrawWireSphere(transform.position, radius);
    }
}