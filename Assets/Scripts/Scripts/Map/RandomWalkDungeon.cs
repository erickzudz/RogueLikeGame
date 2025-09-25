using System.Collections.Generic;
using UnityEngine;

public class RandomWalkDungeon : MonoBehaviour
{
    [Header("Matriz (celdas)")]
    public int width = 16;
    public int length = 16;
    public int seed = 0;
    public Vector2Int EndCell { get; private set; }  // <-- NUEVO

    TurtleModel turtle;
    DungeonGrid grid;
    Dungeon3DView view;

    void Start()
    {
        Generate();
    }

    public void Generate()
    {
        if (!view) view = GetComponent<Dungeon3DView>();
        if (!view) view = gameObject.AddComponent<Dungeon3DView>();

        // Área centrada en (0,0)
        var clamp = new RectInt(-width / 2, -length / 2, width, length);
        grid = new DungeonGrid(clamp);

        // Tortuga y “delegado”: cada paso abre la puerta entre celdas
        turtle = new TurtleModel(Vector2Int.zero, 0, clamp);
        turtle.OnStep = (from, to, dir, inv) => grid.OpenBetween(from, to);

        DoDFS();
        view.Build(grid);
    }

    void DoDFS()
    {
        var rnd = (seed == 0) ? new System.Random() : new System.Random(seed);
        var stack = new Stack<Vector2Int>();
        var visited = new HashSet<Vector2Int>();

        Vector2Int cur = Vector2Int.zero;
        visited.Add(cur);
        grid.SetColor(cur, 1); // inicio rojo

        while (visited.Count < width * length)
        {
            var neighbors = UnvisitedNeighbors(cur, visited);
            if (neighbors.Count > 0)
            {
                var next = neighbors[rnd.Next(neighbors.Count)];
                // mover tortuga en la dirección adecuada
                // mover tortuga en la dirección adecuada
                var delta = next - cur;
                if (delta == Vector2Int.up) turtle.SetDir(0);
                else if (delta == Vector2Int.right) turtle.SetDir(1);
                else if (delta == Vector2Int.down) turtle.SetDir(2);
                else turtle.SetDir(3);

                stack.Push(cur);
                turtle.Forward(1);      // abre puerta (delegado)
                cur = next;
                visited.Add(cur);
            }
            else if (stack.Count > 0)
            {
                cur = stack.Pop();
                turtle.SetPos(cur);
            }
            else break;
        }

        grid.SetColor(cur, 2); // meta verde
        EndCell = cur;  // <--- guarda la celda final
    }

    List<Vector2Int> UnvisitedNeighbors(Vector2Int p, HashSet<Vector2Int> visited)
    {
        var list = new List<Vector2Int>();
        foreach (var d in TurtleModel.DIRS)
        {
            var n = p + d;
            if (grid.Clamp.Contains(n) && !visited.Contains(n))
                list.Add(n);
        }
        return list;
    }
}