using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Rocket
{
    public readonly HashSet<Piece> Pieces = new();

    public float TotalMass { get; private set; }

    public float TotalWeight { get; private set; }

    public readonly List<List<EngineThrustEffect>> EngineGroups = new();

    public readonly List<Fuel> FuelCells = new();

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

    private List<Fuel> GetFuelCells()
    {
        List<Fuel> fuelCells = new();
        foreach (Piece piece in Pieces)
        {
            if (piece.TryGetComponent<Fuel>(out var fuel))
            {
                fuelCells.Add(fuel);
            }
        }

        return fuelCells;
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
        FuelCells.Clear();
        FuelCells.AddRange(GetFuelCells());
        AvailableFuel = FuelCells.Where(fuelCell => fuelCell.Effect == Effect.add).Sum(fuelCell => fuelCell.Value);
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
