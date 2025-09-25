using System;
using UnityEngine;

public class TurtleModel
{
    public static readonly Vector2Int[] DIRS = {
        new Vector2Int(0, 1),  // 0=N
        new Vector2Int(1, 0),  // 1=E
        new Vector2Int(0,-1),  // 2=S
        new Vector2Int(-1,0)   // 3=O
    };

    public Vector2Int Pos { get; private set; }
    public Vector2Int Prev { get; private set; }
    public int Dir { get; private set; }          // 0..3
    public int InvDir => (Dir + 2) & 3;
    public RectInt Clamp { get; private set; }

    public Action<Vector2Int, Vector2Int, int, int> OnStep;   // (from,to,dir,invDir)
    public Action<int, int> OnDir;                           // (dir,invDir)

    public TurtleModel(Vector2Int start, int dir, RectInt clamp)
    {
        Pos = Prev = start;
        Dir = dir & 3;
        Clamp = clamp;
    }
    public void SetDir(int dir)
    {
        Dir = dir & 3;
        OnDir?.Invoke(Dir, InvDir);
    }

    public void SetPos(Vector2Int p) { Prev = Pos; Pos = ClampPos(p); }
    public void SetClamp(RectInt r) { Clamp = r; }
    public void Turn(int delta) { Dir = (Dir + delta) & 3; OnDir?.Invoke(Dir, InvDir); }

    public void Forward(int steps = 1)
    {
        for (int i = 0; i < steps; i++)
        {
            Prev = Pos;
            Pos = ClampPos(Pos + DIRS[Dir]);
            if (Prev != Pos) OnStep?.Invoke(Prev, Pos, Dir, InvDir);
        }
    }

    Vector2Int ClampPos(Vector2Int p)
    {
        p.x = Mathf.Clamp(p.x, Clamp.xMin, Clamp.xMax - 1);
        p.y = Mathf.Clamp(p.y, Clamp.yMin, Clamp.yMax - 1);
        return p;
    }
}
