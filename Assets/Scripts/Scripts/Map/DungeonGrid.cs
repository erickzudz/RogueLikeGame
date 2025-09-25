using System.Collections.Generic;
using UnityEngine;

public class DungeonGrid
{
    // bits 0..3: N,E,S,O  (1 = puerta abierta)
    // bits 4..7: color (0..15)
    const int COLOR_SHIFT = 4;
    Dictionary<Vector2Int, int> cells = new();
    public RectInt Clamp { get; private set; }

    public DungeonGrid(RectInt clamp) { Clamp = clamp; }

    public int Get(Vector2Int p) => cells.TryGetValue(p, out var v) ? v : 0;
    public void Set(Vector2Int p, int v) => cells[p] = v;
    public void Or(Vector2Int p, int mask) => cells[p] = Get(p) | mask;

    public void SetColor(Vector2Int p, int color)
    {
        int v = Get(p);
        v &= ~((0xF) << COLOR_SHIFT);
        v |= (Mathf.Clamp(color, 0, 15) << COLOR_SHIFT);
        Set(p, v);
    }

    public int GetColor(Vector2Int p) => (Get(p) >> COLOR_SHIFT) & 0xF;

    public void OpenBetween(Vector2Int a, Vector2Int b)
    {
        Vector2Int d = b - a;
        int dir = d == Vector2Int.up ? 0 :
                  d == Vector2Int.right ? 1 :
                  d == Vector2Int.down ? 2 : 3;
        int inv = (dir + 2) & 3;
        Or(a, 1 << dir);
        Or(b, 1 << inv);
    }

    public bool Contains(Vector2Int p) => cells.ContainsKey(p);

    public IEnumerable<KeyValuePair<Vector2Int, int>> AllCells() => cells;
}