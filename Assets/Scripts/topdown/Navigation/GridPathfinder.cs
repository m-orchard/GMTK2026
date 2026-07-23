using System.Collections.Generic;
using UnityEngine;

public class GridPathfinder
{
    private const float DiagonalCost = 1.41421356f;

    private static readonly Vector2Int[] Neighbours =
    {
        new Vector2Int(1, 0), new Vector2Int(-1, 0),
        new Vector2Int(0, 1), new Vector2Int(0, -1),
        new Vector2Int(1, 1), new Vector2Int(1, -1),
        new Vector2Int(-1, 1), new Vector2Int(-1, -1),
    };

    private readonly NavigationGrid grid;
    private readonly List<Vector2Int> cellPath = new List<Vector2Int>();
    private readonly List<Vector2Int> smoothBuffer = new List<Vector2Int>();

    public GridPathfinder(NavigationGrid grid)
    {
        this.grid = grid;
    }

    public bool TryFindPath(Vector3 startWorld, Vector3 goalWorld, List<Vector3> waypoints)
    {
        waypoints.Clear();

        Vector2Int start = grid.FindNearestWalkable(grid.WorldToCell(startWorld));
        Vector2Int goal = grid.FindNearestWalkable(grid.WorldToCell(goalWorld));

        if (!SearchCells(start, goal))
        {
            return false;
        }

        SmoothPath();
        BuildWaypoints(goalWorld, waypoints);
        return true;
    }

    private bool SearchCells(Vector2Int start, Vector2Int goal)
    {
        cellPath.Clear();

        int columns = grid.Columns;
        int cellCount = columns * grid.Rows;

        float[] gScore = new float[cellCount];
        float[] fScore = new float[cellCount];
        int[] cameFrom = new int[cellCount];
        bool[] closed = new bool[cellCount];

        for (int i = 0; i < cellCount; i++)
        {
            gScore[i] = float.PositiveInfinity;
            cameFrom[i] = -1;
        }

        int startIndex = Index(start, columns);
        int goalIndex = Index(goal, columns);

        gScore[startIndex] = 0f;
        fScore[startIndex] = Heuristic(start, goal);

        OpenSet open = new OpenSet(fScore);
        open.Push(startIndex);

        while (open.Count > 0)
        {
            int current = open.Pop();
            if (closed[current])
            {
                continue;
            }

            if (current == goalIndex)
            {
                Reconstruct(cameFrom, goalIndex, startIndex, columns);
                return true;
            }

            closed[current] = true;
            Vector2Int currentCell = Cell(current, columns);

            foreach (Vector2Int step in Neighbours)
            {
                Vector2Int nextCell = currentCell + step;
                if (!grid.IsWalkable(nextCell.x, nextCell.y))
                {
                    continue;
                }

                if (IsDiagonal(step) && !CanMoveDiagonally(currentCell, step))
                {
                    continue;
                }

                int nextIndex = Index(nextCell, columns);
                if (closed[nextIndex])
                {
                    continue;
                }

                float tentativeScore = gScore[current] + StepCost(step);
                if (tentativeScore < gScore[nextIndex])
                {
                    cameFrom[nextIndex] = current;
                    gScore[nextIndex] = tentativeScore;
                    fScore[nextIndex] = tentativeScore + Heuristic(nextCell, goal);
                    open.Push(nextIndex);
                }
            }
        }

        return false;
    }

    private void Reconstruct(int[] cameFrom, int goalIndex, int startIndex, int columns)
    {
        cellPath.Clear();
        int current = goalIndex;

        while (current != -1)
        {
            cellPath.Add(Cell(current, columns));
            if (current == startIndex)
            {
                break;
            }

            current = cameFrom[current];
        }

        cellPath.Reverse();
    }

    private void SmoothPath()
    {
        if (cellPath.Count <= 2)
        {
            return;
        }

        smoothBuffer.Clear();
        smoothBuffer.Add(cellPath[0]);

        int anchor = 0;
        while (anchor < cellPath.Count - 1)
        {
            int furthest = anchor + 1;
            for (int test = cellPath.Count - 1; test > anchor + 1; test--)
            {
                if (HasLineOfSight(cellPath[anchor], cellPath[test]))
                {
                    furthest = test;
                    break;
                }
            }

            smoothBuffer.Add(cellPath[furthest]);
            anchor = furthest;
        }

        cellPath.Clear();
        cellPath.AddRange(smoothBuffer);
    }

    private bool HasLineOfSight(Vector2Int from, Vector2Int to)
    {
        int deltaX = Mathf.Abs(to.x - from.x);
        int deltaY = Mathf.Abs(to.y - from.y);
        int stepX = to.x >= from.x ? 1 : -1;
        int stepY = to.y >= from.y ? 1 : -1;
        int error = deltaX - deltaY;
        int x = from.x;
        int y = from.y;

        while (true)
        {
            if (!grid.IsWalkable(x, y))
            {
                return false;
            }

            if (x == to.x && y == to.y)
            {
                return true;
            }

            int doubleError = 2 * error;
            bool stepHorizontal = doubleError > -deltaY;
            bool stepVertical = doubleError < deltaX;

            if (stepHorizontal && stepVertical)
            {
                if (!grid.IsWalkable(x + stepX, y) || !grid.IsWalkable(x, y + stepY))
                {
                    return false;
                }
            }

            if (stepHorizontal)
            {
                error -= deltaY;
                x += stepX;
            }

            if (stepVertical)
            {
                error += deltaX;
                y += stepY;
            }
        }
    }

    private void BuildWaypoints(Vector3 goalWorld, List<Vector3> waypoints)
    {
        for (int i = 1; i < cellPath.Count - 1; i++)
        {
            waypoints.Add(grid.CellToWorld(cellPath[i]));
        }

        waypoints.Add(goalWorld);
    }

    private bool CanMoveDiagonally(Vector2Int from, Vector2Int step)
    {
        return grid.IsWalkable(from.x + step.x, from.y) && grid.IsWalkable(from.x, from.y + step.y);
    }

    private static bool IsDiagonal(Vector2Int step)
    {
        return step.x != 0 && step.y != 0;
    }

    private static float StepCost(Vector2Int step)
    {
        return IsDiagonal(step) ? DiagonalCost : 1f;
    }

    private static float Heuristic(Vector2Int a, Vector2Int b)
    {
        int deltaX = Mathf.Abs(a.x - b.x);
        int deltaY = Mathf.Abs(a.y - b.y);
        int shorter = Mathf.Min(deltaX, deltaY);
        int longer = Mathf.Max(deltaX, deltaY);
        return DiagonalCost * shorter + (longer - shorter);
    }

    private static int Index(Vector2Int cell, int columns)
    {
        return cell.x + cell.y * columns;
    }

    private static Vector2Int Cell(int index, int columns)
    {
        return new Vector2Int(index % columns, index / columns);
    }

    private class OpenSet
    {
        private readonly List<int> cells = new List<int>();
        private readonly float[] priorities;

        public OpenSet(float[] priorities)
        {
            this.priorities = priorities;
        }

        public int Count => cells.Count;

        public void Push(int cell)
        {
            cells.Add(cell);
            SiftUp(cells.Count - 1);
        }

        public int Pop()
        {
            int root = cells[0];
            int last = cells.Count - 1;
            cells[0] = cells[last];
            cells.RemoveAt(last);

            if (cells.Count > 0)
            {
                SiftDown(0);
            }

            return root;
        }

        private void SiftUp(int index)
        {
            while (index > 0)
            {
                int parent = (index - 1) / 2;
                if (priorities[cells[index]] >= priorities[cells[parent]])
                {
                    break;
                }

                Swap(index, parent);
                index = parent;
            }
        }

        private void SiftDown(int index)
        {
            int count = cells.Count;
            while (true)
            {
                int smallest = index;
                int left = index * 2 + 1;
                int right = index * 2 + 2;

                if (left < count && priorities[cells[left]] < priorities[cells[smallest]])
                {
                    smallest = left;
                }

                if (right < count && priorities[cells[right]] < priorities[cells[smallest]])
                {
                    smallest = right;
                }

                if (smallest == index)
                {
                    break;
                }

                Swap(index, smallest);
                index = smallest;
            }
        }

        private void Swap(int a, int b)
        {
            (cells[a], cells[b]) = (cells[b], cells[a]);
        }
    }
}
