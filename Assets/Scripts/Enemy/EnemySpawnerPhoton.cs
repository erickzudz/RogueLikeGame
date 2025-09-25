using Photon.Pun;
using UnityEngine;

public class EnemySpawnerPhoton : MonoBehaviour
{
    [System.Serializable] public struct Entry { public string prefabPath; public int weight; } // Resources/Enemies/...
    public Entry[] enemies;
    public Transform[] spawnPoints;

    [Header("Waves")]
    public float timeBetweenSpawns = 1f;
    public int enemiesPerWave = 10;
    public float timeBetweenWaves = 2f;

    void Start()
    {
        if (PhotonNetwork.IsMasterClient)
            StartCoroutine(Loop());
    }

    System.Collections.IEnumerator Loop()
    {
        while (true)
        {
            for (int i = 0; i < enemiesPerWave; i++)
            {
                var p = spawnPoints[Random.Range(0, spawnPoints.Length)];
                PhotonNetwork.Instantiate(PickByWeight(), p.position, p.rotation);
                yield return new WaitForSeconds(timeBetweenSpawns);
            }
            yield return new WaitForSeconds(timeBetweenWaves);
        }
    }

    string PickByWeight()
    {
        int total = 0; foreach (var e in enemies) total += e.weight;
        int r = Random.Range(0, Mathf.Max(1, total));
        foreach (var e in enemies)
        { if (r < e.weight) return e.prefabPath; r -= e.weight; }
        return enemies[0].prefabPath;
    }
}
