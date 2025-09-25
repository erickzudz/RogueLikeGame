using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Grid")]
    public int width = 1;
    public int length = 1;
    public float tileSize = 0.1f;

    [Header("Perlin")]
    public float noiseScale = 0.015f;
    public float heightMult = 0.05f;
    public float obstacleThreshold = 0.06f;

    [Header("Prefabs/Materials")]
    public Material groundMat;
    public Material obstacleMat;
    public bool useCubesAsFloor = true; // Para tu prueba unitaria

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        // Limpia hijos previos
        for (int i = transform.childCount - 1; i >= 0; i--)
            Destroy(transform.GetChild(i).gameObject);

        float cx = (width - 1) * 0.5f;
        float cz = (length - 1) * 0.5f;

        for (int x = 0; x < width; x++)
            for (int z = 0; z < length; z++)
            {
                float u = (float)x / width;
                float v = (float)z / length;
                float n = Mathf.PerlinNoise(u / noiseScale, v / noiseScale);

                bool obstacle = n > obstacleThreshold;
                float h = obstacle ? Mathf.Lerp(0.5f, 1.5f, (n - obstacleThreshold) / (1f - obstacleThreshold)) : 0.1f;

                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.SetParent(transform, false);
                go.transform.localScale = new Vector3(tileSize, obstacle ? h : 0.1f, tileSize);

                // ► centra cada tile alrededor del origen (el origen será el centro del plano AR)
                float y = go.transform.localScale.y * 0.5f;        // apoya en el “suelo”
                go.transform.localPosition = new Vector3((x - cx) * tileSize, y, (z - cz) * tileSize);

                var mr = go.GetComponent<MeshRenderer>();
                if (mr) mr.material = obstacle ? obstacleMat : groundMat;
            }

        // ¡No muevas transform.localPosition aquí!
    }
}