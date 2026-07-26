using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum Effect
{
    add,
    multiply
}

[RequireComponent(typeof(Piece))]
public class Fuel : MonoBehaviour
{
    [SerializeField]
    private int value = 1;

    [SerializeField]
    private Effect effect = Effect.add;

    public Color addColour = new (53, 187, 22);

    public Color multiplyColour = new (187, 22, 175);

    private int currentValue;

    public int Value { get => currentValue; }

    public Effect Effect { get => effect; }

    public GameObject LabelGroup;

    public TextMeshProUGUI Label;

    private Piece piece;

    // Debug/inspection only: which directly-attached multipliers contributed,
    // and what effective value each one carried in the last rebuild.
    private readonly Dictionary<Fuel, int> incomingMultipliers = new();

    void Awake()
    {
        piece = GetComponent<Piece>();
        RecomputeValue();
        UpdateLabelText();
    }

    void LateUpdate()
    {
        LabelGroup.transform.rotation = Quaternion.identity;
    }

    private void UpdateLabelText()
    {
        var sign = effect == Effect.add ? '+' : 'x';
        Label.text = $"{sign}{Value}";
        Label.color = effect == Effect.add ? addColour : multiplyColour;
    }

    public void OnWeld(PieceWeld weld)
    {
        var porc = piece == weld.child ? "child" : "parent";
        Debug.Log($"[Fuel] Fuel being welded {effect} {value} (i am {porc})");

        // Any weld can change the multiplier topology for the whole connected
        // structure, so rebuild everything reachable from here in one pass.
        RebuildStructure();
    }

    /// <summary>
    /// Finds every Fuel in the connected structure and recomputes each one's
    /// value from scratch. Done as a single pass so no node is double-counted
    /// and no stale data lingers from a previous topology.
    /// </summary>
    private void RebuildStructure()
    {
        var visitedPieces = new HashSet<Piece>();
        var queue = new Queue<Piece>();
        queue.Enqueue(piece);
        visitedPieces.Add(piece);

        var structureFuels = new List<Fuel>();

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();
            if (current.TryGetComponent<Fuel>(out var fuel))
            {
                structureFuels.Add(fuel);
            }

            foreach (var neighbour in current.WeldedNeighbors)
            {
                if (visitedPieces.Add(neighbour))
                {
                    queue.Enqueue(neighbour);
                }
            }
        }

        foreach (var fuel in structureFuels)
        {
            fuel.RecomputeValue();
        }

        foreach (var fuel in structureFuels)
        {
            fuel.UpdateLabelText();
        }
    }

    /// <summary>
    /// Recomputes this fuel's value as: base value * effective value of every
    /// directly-attached multiplier neighbour.
    /// </summary>
    private void RecomputeValue()
    {
        incomingMultipliers.Clear();

        int total = value;

        foreach (var neighbour in piece.WeldedNeighbors)
        {
            if (!neighbour.TryGetComponent<Fuel>(out var neighbourFuel))
            {
                continue;
            }

            if (neighbourFuel.effect != Effect.multiply)
            {
                continue;
            }

            var visited = new HashSet<Fuel> { this };
            int effectiveValue = neighbourFuel.EffectiveValue(this, visited);

            incomingMultipliers[neighbourFuel] = effectiveValue;
            total *= effectiveValue;
        }

        currentValue = total;
    }

    /// <summary>
    /// This multiplier's own value, amplified by every OTHER multiplier
    /// chained onto it (excluding the direction we're being asked from, and
    /// anything already visited in this chain, to avoid double-counting and
    /// infinite loops).
    /// </summary>
    private int EffectiveValue(Fuel excludeDirection, HashSet<Fuel> visited)
    {
        visited.Add(this);

        int result = value; // raw base value, not currentValue

        foreach (var neighbour in piece.WeldedNeighbors)
        {
            if (!neighbour.TryGetComponent<Fuel>(out var neighbourFuel))
            {
                continue;
            }

            if (neighbourFuel == excludeDirection)
            {
                continue;
            }

            if (neighbourFuel.effect != Effect.multiply)
            {
                continue;
            }

            if (visited.Contains(neighbourFuel))
            {
                continue;
            }

            result *= neighbourFuel.EffectiveValue(this, visited);
        }

        return result;
    }
}