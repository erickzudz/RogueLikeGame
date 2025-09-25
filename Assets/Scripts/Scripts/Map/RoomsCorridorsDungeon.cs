using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RoomsCorridorsDungeon : MonoBehaviour
{
    [Header("Grid (cells)")]
    public int width = 32;
    public int length = 32;
    public int seed = 0;

    [Header("Rooms")]
    public int minRooms = 6;
    public int maxRooms = 12;
    public int roomMinW = 4;
    public int roomMaxW = 8;
    public int roomMinH = 4;
    public int roomMaxH = 8;
    public int roomPadding = 1;

    [Header("Corridors")]
    public int extraConnections = 2;

    DungeonGrid grid;
    Dungeon3DView view;

    // NUEVO: datos expuestos
    public Vector2Int StartCell { get; private set; }
    public Vector2Int EndCell { get; private set; }
    public List<Vector2Int> RoomCenters { get; private set; }

    // Si prefieres controlar desde Bootstrap, comenta esto:
    // void Start() => Generate();

    public void Generate()
    {
        if (!view) view = GetComponent<Dungeon3DView>();
        if (!view) view = gameObject.AddComponent<Dungeon3DView>();

        var clamp = new RectInt(-width / 2, -length / 2, width, length);
        grid = new DungeonGrid(clamp);

        var rnd = (seed == 0) ? new System.Random() : new System.Random(seed);

        // 1) Rooms
        var rooms = PlaceRooms(rnd, clamp);

        // 2) Conectar (MST + extras)
        ConnectRooms(rnd, rooms);

        // 3) Centros/inicio/fin
        var centers = rooms.Select(Center).ToList();
        RoomCenters = centers;

        if (centers.Count > 0)
        {
            StartCell = centers[0];
            EndCell = centers.OrderBy(c => Manhattan(c, StartCell)).Last();

            // coloreo opcional de debug (si tu grid soporta color)
            grid.SetColor(StartCell, 1);
            grid.SetColor(EndCell, 2);
        }

        // 4) Render 3D
        view.Build(grid);
    }

    // ---------- ROOMS ----------
    List<RectInt> PlaceRooms(System.Random rnd, RectInt clamp)
    {
        int target = rnd.Next(minRooms, maxRooms + 1);
        var placed = new List<RectInt>();
        int attempts = target * 12;

        while (placed.Count < target && attempts-- > 0)
        {
            int w = rnd.Next(roomMinW, roomMaxW + 1);
            int h = rnd.Next(roomMinH, roomMaxH + 1);

            int x = rnd.Next(clamp.xMin, clamp.xMax - w);
            int y = rnd.Next(clamp.yMin, clamp.yMax - h);
            var r = new RectInt(x, y, w, h);

            // respetar padding
            var padded = Inflate(r, roomPadding);

            bool overlaps = placed.Any(pr => pr.Overlaps(padded));
            if (overlaps) continue;

            placed.Add(r);
            CarveRoom(r);
        }

        return placed;
    }

    void CarveRoom(RectInt r)
    {
        // Abre todas las celdas del rect y conexiones internas
        for (int x = r.xMin; x < r.xMax; x++)
            for (int y = r.yMin; y < r.yMax; y++)
            {
                var p = new Vector2Int(x, y);
                var right = new Vector2Int(x + 1, y);
                if (x + 1 < r.xMax) grid.OpenBetween(p, right);
                var up = new Vector2Int(x, y + 1);
                if (y + 1 < r.yMax) grid.OpenBetween(p, up);
            }
    }

    // ---------- CONNECT (MST + extras) ----------
    void ConnectRooms(System.Random rnd, List<RectInt> rooms)
    {
        if (rooms.Count <= 1) return;

        var centers = rooms.Select(Center).ToList();

        // Kruskal simple
        var edges = new List<Edge>();
        for (int i = 0; i < centers.Count; i++)
            for (int j = i + 1; j < centers.Count; j++)
            {
                int w = Manhattan(centers[i], centers[j]) + rnd.Next(5);
                edges.Add(new Edge(i, j, w));
            }
        edges.Sort((a, b) => a.w.CompareTo(b.w));

        var dsu = new DSU(centers.Count);
        var mst = new List<Edge>();

        foreach (var e in edges)
        {
            if (dsu.Find(e.a) != dsu.Find(e.b))
            {
                dsu.Union(e.a, e.b);
                mst.Add(e);
                CarveCorridor(centers[e.a], centers[e.b]);
            }
        }

        // Extras para loops
        var others = edges.Except(mst).ToList();
        for (int i = 0; i < extraConnections && others.Count > 0; i++)
        {
            int k = rnd.Next(others.Count);
            var e = others[k]; others.RemoveAt(k);
            CarveCorridor(centers[e.a], centers[e.b]);
        }
    }

    void CarveCorridor(Vector2Int a, Vector2Int b)
    {
        // L-corridor (primero X, luego Y)
        var cur = a;
        int sx = Math.Sign(b.x - cur.x);
        while (cur.x != b.x)
        {
            var next = new Vector2Int(cur.x + sx, cur.y);
            grid.OpenBetween(cur, next);
            cur = next;
        }
        int sy = Math.Sign(b.y - cur.y);
        while (cur.y != b.y)
        {
            var next = new Vector2Int(cur.x, cur.y + sy);
            grid.OpenBetween(cur, next);
            cur = next;
        }
    }

    // ---------- Utils ----------
    static int Manhattan(Vector2Int a, Vector2Int b)
        => Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    static Vector2Int Center(RectInt r)
        => new Vector2Int(r.x + r.width / 2, r.y + r.height / 2);
    static RectInt Inflate(RectInt r, int pad)
        => new RectInt(r.x - pad, r.y - pad, r.width + pad * 2, r.height + pad * 2);

    struct Edge { public int a, b, w; public Edge(int a, int b, int w) { this.a = a; this.b = b; this.w = w; } }
    class DSU
    {
        int[] p, r;
        public DSU(int n) { p = new int[n]; r = new int[n]; for (int i = 0; i < n; i++) p[i] = i; }
        public int Find(int x) => p[x] == x ? x : (p[x] = Find(p[x]));
        public void Union(int a, int b)
        {
            a = Find(a); b = Find(b);
            if (a == b) return;
            if (r[a] < r[b]) (a, b) = (b, a);
            p[b] = a; if (r[a] == r[b]) r[a]++;
        }
    }
}
