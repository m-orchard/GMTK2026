using UnityEngine;

public class NavigationGrid : MonoBehaviour
{
    public static NavigationGrid Instance { get; private set; }

    [Header("Grid")]
    [SerializeField, Min(0.05f)] private float cellSize = 0.5f;
    [SerializeField, Min(1)] private int columns = 40;
    [SerializeField, Min(1)] private int rows = 40;

    [Header("Walls")]
    [SerializeField] private LayerMask wallLayers;

    [Tooltip("Fraction of a cell tested for wall overlap. Below 1 leaves a little slack so enemies can pass through tight gaps.")]
    [SerializeField, Range(0.1f, 1f)] private float cellFillRatio = 0.9f;

    [Header("Debug")]
    [SerializeField] private bool drawBlockedCells = true;

    private bool[,] walkable;

    public int Columns => columns;
    public int Rows => rows;
    public float CellSize => cellSize;

    private Vector3 Origin => transform.position - new Vector3(columns * cellSize, rows * cellSize, 0f) * 0.5f;

    private void Awake()
    {
        Instance = this;
        Bake();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    public void Bake()
    {
        walkable = new bool[columns, rows];
        Vector2 overlapSize = Vector2.one * (cellSize * cellFillRatio);

        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                Vector3 center = CellToWorld(new Vector2Int(x, y));
                bool blocked = Physics2D.OverlapBox(center, overlapSize, 0f, wallLayers) != null;
                walkable[x, y] = !blocked;
            }
        }
    }

    public bool InBounds(int x, int y)
    {
        return x >= 0 && x < columns && y >= 0 && y < rows;
    }

    public bool IsWalkable(int x, int y)
    {
        return InBounds(x, y) && walkable[x, y];
    }

    public Vector3 CellToWorld(Vector2Int cell)
    {
        return Origin + new Vector3((cell.x + 0.5f) * cellSize, (cell.y + 0.5f) * cellSize, 0f);
    }

    public Vector2Int WorldToCell(Vector3 world)
    {
        Vector3 local = world - Origin;
        int x = Mathf.FloorToInt(local.x / cellSize);
        int y = Mathf.FloorToInt(local.y / cellSize);
        return new Vector2Int(Mathf.Clamp(x, 0, columns - 1), Mathf.Clamp(y, 0, rows - 1));
    }

    public Vector2Int FindNearestWalkable(Vector2Int cell)
    {
        if (IsWalkable(cell.x, cell.y))
        {
            return cell;
        }

        int maxRadius = Mathf.Max(columns, rows);
        for (int radius = 1; radius <= maxRadius; radius++)
        {
            for (int x = -radius; x <= radius; x++)
            {
                for (int y = -radius; y <= radius; y++)
                {
                    bool onRing = Mathf.Abs(x) == radius || Mathf.Abs(y) == radius;
                    if (!onRing)
                    {
                        continue;
                    }

                    if (IsWalkable(cell.x + x, cell.y + y))
                    {
                        return new Vector2Int(cell.x + x, cell.y + y);
                    }
                }
            }
        }

        return cell;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireCube(transform.position, new Vector3(columns * cellSize, rows * cellSize, 0f));

        if (!drawBlockedCells || walkable == null)
        {
            return;
        }

        Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
        for (int x = 0; x < columns; x++)
        {
            for (int y = 0; y < rows; y++)
            {
                if (!walkable[x, y])
                {
                    Gizmos.DrawCube(CellToWorld(new Vector2Int(x, y)), Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}
