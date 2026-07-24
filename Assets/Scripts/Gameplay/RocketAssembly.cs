using System.Collections.Generic;
using UnityEngine;

public class RocketAssembly : MonoBehaviour
{
    public float PadY { get; private set; }
    public Piece PadPiece { get; private set; }
    public Piece CargoPiece { get; private set; }

    private FixedJoint2D padJoint;

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

    public void SetPadPiece(Piece piece, FixedJoint2D joint)
    {
        PadPiece = piece;
        padJoint = joint;
    }

    public void SetCargoPiece(Piece piece)
    {
        CargoPiece = piece;
    }

    public void ReleasePad()
    {
        if (padJoint != null) Destroy(padJoint);
        padJoint = null;
    }

    public HashSet<Piece> GetConnectedPieces()
    {
        var result = new HashSet<Piece>();
        if (PadPiece == null) return result;

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

        var visited = new HashSet<Rigidbody2D> { PadPiece.Body2D };
        var queue = new Queue<Rigidbody2D>();
        queue.Enqueue(PadPiece.Body2D);

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
        padJoint = null;
        CargoPiece = null;
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Destroy(transform.GetChild(i).gameObject);
        }
    }
}
