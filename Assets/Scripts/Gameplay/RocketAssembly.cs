using System.Collections.Generic;
using UnityEngine;

public class RocketAssembly : MonoBehaviour
{
    [SerializeField] private GameObject rocketFoundationPrefab;

    public float PadY { get; private set; }
    public Piece PadPiece { get; private set; }
    public Piece CargoPiece { get; private set; }

    private void Awake()
    {
        PadY = transform.position.y;
    }

    public IEnumerable<Piece> Pieces => GetComponentsInChildren<Piece>();

    public float HighestPointY()
    {
        float highest = PadY;
        foreach (var piece in Pieces)
        {
            if (piece.transform.position.y > highest) highest = piece.transform.position.y;
        }
        return highest;
    }

    public Bounds GetBounds(IEnumerable<Piece> subset)
    {
        using var enumerator = subset.GetEnumerator();
        if (!enumerator.MoveNext())
        {
            return new Bounds(new Vector3(transform.position.x, PadY, 0f), Vector3.zero);
        }

        var bounds = new Bounds(enumerator.Current.transform.position, Vector3.zero);
        while (enumerator.MoveNext())
        {
            bounds.Encapsulate(enumerator.Current.transform.position);
        }
        return bounds;
    }

    public void SetCargoPiece(Piece piece)
    {
        CargoPiece = piece;
    }

    public HashSet<EngineThrustEffect> GetBracedEngines()
    {
        var braced = new HashSet<EngineThrustEffect>();
        var remainingCapacity = new Dictionary<Piece, int>();

        foreach (var p in Pieces)
        {
            if (p.TryGetComponent<EngineThrustEffect>(out _)) continue;
            remainingCapacity[p] = p.EngineSupportCapacity;
        }

        foreach (var p in Pieces)
        {
            if (!p.TryGetComponent<EngineThrustEffect>(out var engine)) continue;

            foreach (var neighbor in p.WeldedNeighbors)
            {
                if (!remainingCapacity.TryGetValue(neighbor, out int capacity) || capacity <= 0) continue;
                remainingCapacity[neighbor] = capacity - 1;
                braced.Add(engine);
                break;
            }
        }

        return braced;
    }

    private void Update()
    {
        var braced = GetBracedEngines();
        foreach (var engine in GetComponentsInChildren<EngineThrustEffect>())
        {
            engine.SetPowered(braced.Contains(engine));
        }
    }

    public void LockSettledPieces()
    {
        foreach (var p in Pieces)
        {
            if (p.IsLocked) continue;
            if (p.TryGetComponent<FallingPieceController>(out var controller)) controller.LockIfAtRest();
        }
    }

    public HashSet<Piece> GetConnectedPieces()
    {
        var result = new HashSet<Piece>();

        Piece root = PadPiece;
        if (root == null)
        {
            // No foundation piece was spawned this round (prefab not assigned yet) -
            // fall back to any locked piece so connectivity/camera framing still works.
            foreach (var p in Pieces)
            {
                if (!p.IsLocked) continue;
                root = p;
                break;
            }
        }
        if (root == null) return result;

        var adjacency = new Dictionary<Rigidbody2D, List<Rigidbody2D>>();
        foreach (var p in Pieces)
        {
            foreach (var joint in p.GetComponents<Joint2D>())
            {
                if (joint.connectedBody == null) continue;
                AddEdge(adjacency, p.Body2D, joint.connectedBody);
                AddEdge(adjacency, joint.connectedBody, p.Body2D);
            }
        }

        var visited = new HashSet<Rigidbody2D> { root.Body2D };
        var queue = new Queue<Rigidbody2D>();
        queue.Enqueue(root.Body2D);

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (!adjacency.TryGetValue(current, out var neighbors)) continue;

            foreach (var neighbor in neighbors)
            {
                if (visited.Add(neighbor)) queue.Enqueue(neighbor);
            }
        }

        foreach (var p in Pieces)
        {
            if (visited.Contains(p.Body2D)) result.Add(p);
        }
        return result;
    }

    private static void AddEdge(Dictionary<Rigidbody2D, List<Rigidbody2D>> adjacency, Rigidbody2D a, Rigidbody2D b)
    {
        if (!adjacency.TryGetValue(a, out var list))
        {
            list = new List<Rigidbody2D>();
            adjacency[a] = list;
        }
        list.Add(b);
    }

    public void ClearAll()
    {
        PadPiece = null;
        CargoPiece = null;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
        SpawnRocketFoundation();
    }

    private void SpawnRocketFoundation()
    {
        if (rocketFoundationPrefab == null) return;

        var instance = Instantiate(rocketFoundationPrefab, new Vector3(transform.position.x, PadY, 0f), Quaternion.identity, transform);

        int lockedLayer = LayerMask.NameToLayer("Locked");
        if (lockedLayer >= 0) instance.layer = lockedLayer;

        if (instance.TryGetComponent<FallingPieceController>(out var controller)) controller.ForceLock();

        PadPiece = instance.GetComponent<Piece>();
        if (PadPiece != null) PadPiece.Body2D.bodyType = RigidbodyType2D.Kinematic;
    }

    public void ReleaseFoundation()
    {
        if (PadPiece != null) PadPiece.Body2D.bodyType = RigidbodyType2D.Dynamic;
    }
}
