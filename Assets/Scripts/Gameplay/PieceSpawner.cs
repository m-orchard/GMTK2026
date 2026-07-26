using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PieceSpawner : Singleton<PieceSpawner>
{
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private Conveyor conveyor;
    [SerializeField] private OffScreenSlider conveyorRig;
    [SerializeField] private CargoCrane crane;
    [SerializeField] private OffScreenSlider craneRig;
    [SerializeField] private List<GameObject> cargoPrefabs;

    [SerializeField] private float wellMinX = -4.5f;
    [SerializeField] private float wellMaxX = 4.5f;

    [Header("Cargo Drop")]
    [Tooltip("When on, dropping cargo takes over the active slot so the conveyor pauses until the cargo is placed. When off, the cargo falls on its own and the conveyor keeps delivering pieces uninterrupted.")]
    [SerializeField] private bool cargoDropStopsConveyor = true;

    private readonly List<GameObject> bag = new List<GameObject>();
    private PiecePool pool;
    private List<GameObject> pendingPoolOverrides;

    private bool conveyorDispensingStopped;
    private bool controlDisabled;
    private bool craneBlocked;
    private int nextCargoIndex;

    public FallingPieceController Active { get; private set; }

    public void SetPool(PiecePool pool)
    {
        this.pool = pool;
        pendingPoolOverrides = new(pool.overrides);
        pendingPoolOverrides.AddRange(LevelManager.Instance.AcquiredPieces);
    }

    public void ReplaceFrontConveyorPiece(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var controller = instance.GetComponent<FallingPieceController>();
        if (controller != null)
            controller.enabled = false;
        conveyor.ReplaceFront(instance);
    }

    public void StartBelt()
    {
        conveyorDispensingStopped = false;
        controlDisabled = false;
        conveyorRig.ResetPosition();
        conveyorRig.BeginFollowingRocketTop();
        conveyor.OnPieceReachedDrop -= HandlePieceReachedDrop;
        conveyor.OnPieceReachedDrop += HandlePieceReachedDrop;
        conveyor.Clear();
        for (int i = 0; i < conveyor.SlotCount; i++)
            AddPieceToConveyor(NextFromBag());
    }

    public void ReleaseFirstPiece()
    {
        conveyor.ReleaseFront();
    }

    public void BeginBuildEndExit(float exitDuration)
    {
        conveyorDispensingStopped = true;
        conveyor.StopDispensing();
        conveyorRig.ExitOffScreen(exitDuration);

        craneBlocked = true;
        crane.StopFetching();
        craneRig.ExitOffScreen(exitDuration);
    }

    public void SpawnCargo(bool controllable = true)
    {
        if (craneBlocked || !crane.IsReady)
            return;

        GameObject cargo = crane.ReleaseHeld();

        if (cargoDropStopsConveyor)
        {
            if (Active != null)
                DropActive();
            BecomeActiveFallingPiece(cargo, controllable);
        }
        else
        {
            ReleaseAsFreeFallingPiece(cargo);
        }

        rocket.SetCargoPiece(cargo.GetComponent<Piece>());

        FetchNextCargo();
    }

    private void DropActive()
    {
        Active.OnReleased -= HandleReleased;
        Active.Release();
        Active = null;
    }

    private void ReleaseAsFreeFallingPiece(GameObject instance)
    {
        instance.transform.SetParent(rocket.transform, worldPositionStays: true);

        var controller = instance.GetComponent<FallingPieceController>();
        controller.enabled = true;
        controller.SetControllable(false);
        controller.SetBounds(wellMinX, wellMaxX);
        controller.Release();
    }

    public void ResetCargo()
    {
        nextCargoIndex = 0;
        craneBlocked = false;
        craneRig.ResetPosition();
        craneRig.BeginFollowingRocketTop();
        crane.ResetCrane();
        FetchNextCargo();
    }

    private void FetchNextCargo()
    {
        if (nextCargoIndex >= cargoPrefabs.Count)
            return;

        GameObject prefab = cargoPrefabs[nextCargoIndex];
        nextCargoIndex++;

        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.GetComponent<FallingPieceController>().enabled = false;
        crane.Fetch(instance);
    }

    private void AddPieceToConveyor(GameObject prefab)
    {
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.GetComponent<FallingPieceController>().enabled = false;
        conveyor.Enqueue(instance);
    }

    public void DisableControl()
    {
        controlDisabled = true;
    }

    private void BecomeActiveFallingPiece(GameObject instance, bool controllable = true)
    {
        instance.transform.SetParent(rocket.transform, worldPositionStays: true);

        var controller = instance.GetComponent<FallingPieceController>();
        controller.enabled = true;
        controller.SetControllable(controllable && !controlDisabled);
        controller.SetBounds(wellMinX, wellMaxX);
        controller.SetLockCeiling(instance.transform.position.y);
        controller.SnapToMovementStep();

        if (controlDisabled)
        {
            controller.Release();
            return;
        }

        controller.OnReleased += HandleReleased;
        Active = controller;
    }

    private void HandlePieceReachedDrop(GameObject instance)
    {
        BecomeActiveFallingPiece(instance);

        if (!conveyorDispensingStopped)
            AddPieceToConveyor(NextFromBag());
    }

    private void HandleReleased()
    {
        Active.OnReleased -= HandleReleased;
        Active = null;
        if (!conveyorDispensingStopped)
            conveyor.ReleaseFront();
    }

    public void ForceLockActive()
    {
        if (Active == null)
            return;
        Active.OnReleased -= HandleReleased;
        Active.ForceLock();
        Active = null;
    }

    private GameObject NextFromBag()
    {
        if (bag.Count == 0)
            RefillBag();

        int lastIndex = bag.Count - 1;
        GameObject prefab = bag[lastIndex];
        bag.RemoveAt(lastIndex);
        return prefab;
    }

    private void RefillBag()
    {
        bag.Clear();

        var bagSize = pool.bagSize;
        var overridesToAdd = Math.Min(pendingPoolOverrides.Count(), bagSize);

        if (overridesToAdd > 0)
        {
            Debug.Log($"[PieceSpawner]: Refilling bag with overrides (count={overridesToAdd})");

            var overrides = pendingPoolOverrides.GetRange(0, overridesToAdd);
            overrides.Reverse();
            bag.AddRange(overrides);
            pendingPoolOverrides.RemoveRange(0, overridesToAdd);
            bagSize -= overridesToAdd;
        }

        var groups = new List<(int group, List<AvailablePiece> members)>();
        var groupIndex = new Dictionary<int, int>();

        foreach (var entry in pool.members)
        {
            if (!groupIndex.TryGetValue(entry.group, out int idx))
            {
                idx = groups.Count;
                groupIndex[entry.group] = idx;
                groups.Add((entry.group, new List<AvailablePiece>()));
            }
            groups[idx].members.Add(entry);
        }

        var groupWeights = groups.Select(g => g.members.Sum(m => m.chance)).ToList();
        var groupCounts = AllocateCounts(bagSize, groupWeights);

        Debug.Log($"[PieceSpawner]: Adding random pieces to bag (count={bagSize}, groups={groups.Count})");

        for (int g = 0; g < groups.Count; g++)
        {
            int groupCount = groupCounts[g];
            if (groupCount <= 0) continue;

            var members = groups[g].members;
            var memberWeights = members.Select(m => m.chance).ToList();
            var memberCounts = AllocateCounts(groupCount, memberWeights);

            for (int m = 0; m < members.Count; m++)
            {
                for (int j = 0; j < memberCounts[m]; j++)
                    bag.Add(members[m].prefab);
            }
        }

        for (int i = bag.Count - 1; i > overridesToAdd; i--)
        {
            int j = UnityEngine.Random.Range(overridesToAdd, i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }

        Debug.Log($"[PieceSpawner]: Refilled bag (count={bag.Count()})");
    }

    // Guarantees every positive-weight entry at least one slot (when the bag has
    // room for it), then splits whatever remains across `weights` using exact
    // floors plus a randomly-distributed remainder (weighted by fractional
    // share), so proportional splits stay exact but rounding ties resolve
    // differently each call. The upfront guarantee is what makes "you'll always
    // get at least one engine per bag" an actual guarantee rather than a
    // coincidence of today's exact weight values.
    private int[] AllocateCounts(int total, List<float> weights)
    {
        int n = weights.Count;
        var counts = new int[n];
        float totalWeight = weights.Sum();

        if (totalWeight <= 0f)
        {
            counts[n - 1] = total;
            return counts;
        }

        int positiveWeightCount = weights.Count(w => w > 0f);
        int remaining = total;
        if (positiveWeightCount <= total)
        {
            for (int i = 0; i < n; i++)
            {
                if (weights[i] <= 0f) continue;
                counts[i] = 1;
                remaining--;
            }
        }

        if (remaining <= 0) return counts;

        var fractions = new float[n];
        int assigned = 0;

        for (int i = 0; i < n; i++)
        {
            float exact = remaining * (weights[i] / totalWeight);
            int extra = Mathf.FloorToInt(exact);
            counts[i] += extra;
            fractions[i] = exact - extra;
            assigned += extra;
        }

        int remainder = remaining - assigned;
        var indices = Enumerable.Range(0, n).ToList();

        for (int r = 0; r < remainder; r++)
        {
            float totalFrac = indices.Sum(i => fractions[i]);
            int chosen;
            if (totalFrac <= 0f)
            {
                chosen = indices[UnityEngine.Random.Range(0, indices.Count)];
            }
            else
            {
                float roll = UnityEngine.Random.Range(0f, totalFrac);
                float cumulative = 0f;
                chosen = indices[indices.Count - 1];
                foreach (int i in indices)
                {
                    cumulative += fractions[i];
                    if (roll <= cumulative) { chosen = i; break; }
                }
            }
            counts[chosen]++;
            fractions[chosen] = 0f;
            indices.Remove(chosen);
        }

        return counts;
    }
}