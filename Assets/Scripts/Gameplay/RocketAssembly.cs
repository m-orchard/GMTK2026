using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class RocketAssembly : Singleton<RocketAssembly>
{
    [SerializeField] private GameObject rocketFoundationPrefab;
    [SerializeField] private bool requireEngineBracing = false;

    public float PadY { get; private set; }
    public readonly List<Piece> PadPieces = new();

    public readonly Rocket Rocket = new();

    public Piece CargoPiece { get; private set; }

    public System.Action OnAddPiece;

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
            if (IsUnderPlayerControl(piece)) continue;
            if (piece.transform.position.y > highest) highest = piece.transform.position.y;
        }
        return highest;
    }

    private static bool IsUnderPlayerControl(Piece piece)
    {
        return piece.TryGetComponent<FallingPieceController>(out var controller) && !controller.Released;
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

    public void LockSettledPieces()
    {
        foreach (var p in Pieces)
        {
            if (p.IsLocked) continue;
            if (p.TryGetComponent<FallingPieceController>(out var controller)) controller.LockIfAtRest();
        }
    }

    private void AddNeighbours(HashSet<Piece> collection, Piece piece)
    {
        var neighbours = piece.WeldedNeighbors;
        foreach (var neighbour in neighbours)
        {
            if (collection.Contains(neighbour))
            {
                continue;
            }

            collection.Add(neighbour);
            AddNeighbours(collection, neighbour);
        }
    }

    public void UpdateRocket()
    {
        Rocket.requireEngineBracing = requireEngineBracing;

        Piece root = PadPieces.Count > 0
            ? PadPieces[0]
            // No foundation piece was spawned this round (prefab not assigned yet) -
            // fall back to any locked piece so connectivity/camera framing still works.
            : Pieces.FirstOrDefault(p => p.IsLocked);

        int previousCount = Rocket.Pieces.Count();

        Rocket.Update(root);

        int newCount = Rocket.Pieces.Count();
        int change = newCount - previousCount;

        Debug.Log($"[RocketAssembly] Updated rocket: had {previousCount} pieces, now {newCount} pieces");

        for (var i = 0; i < change; i++)
        {
            OnAddPiece?.Invoke();
        }
    }

    public void ClearAll()
    {
        PadPieces.Clear();
        UpdateRocket();
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

        var instance = Instantiate(rocketFoundationPrefab, new Vector3(transform.position.x, PadY + 1f, 0f), Quaternion.identity, transform);

        PadPieces.AddRange(instance.GetComponentsInChildren<Piece>());
        Debug.Log($"[RocketAssembly] Adding {PadPieces.Count()} pad pieces");

        var controllers = instance.GetComponentsInChildren<FallingPieceController>();
        foreach (var controller in controllers)
        {
            controller.Release();
        }

        for (var i = 0; i < PadPieces.Count(); i++)
        {
            var piece = PadPieces[i];
            piece.Body2D.bodyType = RigidbodyType2D.Kinematic;
            if (i > 0)
            {
                piece.WeldTo(PadPieces[i - 1]);
            }
            piece.Lock();
        }
    }

    public void ReleaseFoundation()
    {
        foreach (var piece in PadPieces)
        {
            piece.Body2D.bodyType = RigidbodyType2D.Dynamic;
        }
    }
}
