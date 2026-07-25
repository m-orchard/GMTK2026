using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public struct FuelPiece
{
    public Fuel fuel;
    public Piece piece;
}

public class Rocket
{
    public readonly HashSet<Piece> Pieces = new();

    public float TotalMass { get; private set; }

    public float TotalWeight { get; private set; }

    public readonly List<List<Fuel>> FuelClusters = new();

    public readonly List<List<EngineThrustEffect>> EngineGroups = new();

    public float AvailableFuel { get; private set; }

    public float PotentialThrust { get; private set; }

    public System.Action OnAddPiece;

    public bool requireEngineBracing = false;

    public void Update(Piece root)
    {
        var result = new HashSet<Piece>();

        if (root != null)
        {
            result.Add(root);
            AddNeighbours(result, root);
        }

        Pieces.Clear();
        Pieces.UnionWith(result);

        UpdateTotalMass();
        UpdateFuel();
        UpdateEngines();
    }

    private List<List<Fuel>> GetFuelClusters()
    {
        List<FuelPiece> fuelPieces = new();
        foreach (Piece piece in Pieces)
        {
            if (piece.TryGetComponent<Fuel>(out var fuel))
            {
                fuelPieces.Add(new FuelPiece { fuel = fuel, piece = piece });
            }
        }

        var fuelPieceGroups = fuelPieces.GroupBy(fp => fp.fuel.Group);
        List<List<Fuel>> fuelClusters = new();

        foreach (var fuelPieceGroup in fuelPieceGroups)
        {
            var remaining = fuelPieceGroup.ToDictionary(fp => fp.piece);

            while (remaining.Count > 0)
            {
                var cluster = new List<Fuel>();
                var queue = new Queue<FuelPiece>();

                var start = remaining.Values.First();
                queue.Enqueue(start);
                remaining.Remove(start.piece);

                while (queue.Count > 0)
                {
                    var current = queue.Dequeue();
                    cluster.Add(current.fuel);

                    var neighbors = current.piece.WeldedNeighbors;
                    foreach (var neighbor in neighbors)
                    {
                        if (remaining.TryGetValue(neighbor, out var neighborFp))
                        {
                            queue.Enqueue(neighborFp);
                            remaining.Remove(neighbor);
                        }
                    }
                }

                fuelClusters.Add(cluster);
            }
        }
        return fuelClusters;
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

    private void UpdateTotalMass()
    {
        var rididBodies = Pieces.Select(piece => piece.Body2D);
        float gravity = Mathf.Abs(Physics2D.gravity.y);
        TotalMass = rididBodies.Sum(rb => rb.mass);
        TotalWeight = rididBodies.Sum(rb => rb.mass * gravity * rb.gravityScale);
    }

    private void UpdateFuel()
    {
        FuelClusters.Clear();
        FuelClusters.AddRange(GetFuelClusters());
        AvailableFuel = CalculateTotalFuel(FuelClusters);
    }

    private float CalculateTotalFuel(List<List<Fuel>> fuelClusters)
    {
        float total = 0f;
        foreach (var cluster in fuelClusters)
        {
            float clusterSum = cluster.Sum(fp => fp.Value);
            float clusterSize = cluster.Count;
            float clusterTotal = clusterSum * (1 + ((clusterSize - 1) / 2));
            Debug.Log($"[LaunchController] Calculated cluster fuel for cluster: group={cluster[0].Group}, sum={clusterSum}, size={clusterSize}, total={clusterTotal}");
            total += clusterTotal;
        }
        Debug.Log($"[LaunchController] Calculated total fuel for {fuelClusters.Count()} clusters: {total}");
        return total;
    }

    private void UpdateEngines()
    {
        var engines = new HashSet<EngineThrustEffect>();
        foreach (Piece p in Pieces)
        {
            if (p.TryGetComponent<EngineThrustEffect>(out var engine))
            {
                engines.Add(engine);
            }
        }

        if (requireEngineBracing)
        {
            engines = GetBracedEngines(engines);
        }

        var priorityGroups = engines
            .GroupBy(x => x.Group)
            .Select(g => g
                .GroupBy(x => x.PhasePriority)
                .OrderBy(pg => pg.Key)
                .Select(pg => pg.ToList())
                .ToList())
            .ToList();

        int numPhases = priorityGroups.Count() > 0 ? priorityGroups.Max(g => g.Count) : 0;
        var engineGroups = Enumerable.Range(0, numPhases)
            .Select(depth => priorityGroups
                .Where(g => depth < g.Count)
                .SelectMany(g => g[depth])
                .ToList())
            .ToList();

        EngineGroups.Clear();
        EngineGroups.AddRange(engineGroups);

        PotentialThrust = engines.Sum(engine => engine.Thrust);
    }

    public HashSet<EngineThrustEffect> GetBracedEngines(IEnumerable<EngineThrustEffect> engines)
    {
        var braced = new HashSet<EngineThrustEffect>();
        var remainingCapacity = new Dictionary<Piece, int>();

        foreach (var p in Pieces)
        {
            if (p.TryGetComponent<EngineThrustEffect>(out _)) continue;
            remainingCapacity[p] = p.EngineSupportCapacity;
        }

        foreach (var engine in engines)
        {
            var neighbours = engine.GetComponent<Piece>().WeldedNeighbors;
            foreach (var neighbor in neighbours)
            {
                if (!remainingCapacity.TryGetValue(neighbor, out int capacity) || capacity <= 0) continue;
                remainingCapacity[neighbor] = capacity - 1;
                braced.Add(engine);
                break;
            }
        }

        return braced;
    }
}
