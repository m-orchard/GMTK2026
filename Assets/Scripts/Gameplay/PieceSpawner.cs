using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct AvailablePiece
{
    public GameObject prefab;

    [Range(0f, 1f)]
    public float chance;
}

public class PieceSpawner : Singleton<PieceSpawner> {
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private Conveyor conveyor;
    [SerializeField] private OffScreenSlider conveyorRig;
    [SerializeField] private CargoCrane crane;
    [SerializeField] private OffScreenSlider craneRig;
    [SerializeField] private List<GameObject> cargoPrefabs;

    [SerializeField] private int bagSize = 20;
    [SerializeField] private float wellMinX = -4.5f;
    [SerializeField] private float wellMaxX = 4.5f;

    private readonly List<GameObject> bag = new List<GameObject>();
    private readonly List<AvailablePiece> pool = new List<AvailablePiece>();

    private bool conveyorDispensingStopped;
    private bool craneBlocked;
    private int nextCargoIndex;

    public FallingPieceController Active { get; private set; }

    public IEnumerable<GameObject> PiecePrefabs {
        get {
            foreach (AvailablePiece availablePiece in pool)
                yield return availablePiece.prefab;
        }
    }

    public void SetPool(IReadOnlyList<AvailablePiece> pieces) {
        pool.Clear();
        for (int i = 0; i < pieces.Count; i++)
            pool.Add(pieces[i]);
    }

    public void ReplaceFrontConveyorPiece(GameObject prefab) {
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        var controller = instance.GetComponent<FallingPieceController>();
        if (controller != null)
            controller.enabled = false;
        conveyor.ReplaceFront(instance);
    }

    public void StartBelt() {
        conveyorDispensingStopped = false;
        conveyorRig.ResetPosition();
        conveyorRig.BeginFollowingRocketTop();
        conveyor.OnPieceReachedDrop -= HandlePieceReachedDrop;
        conveyor.OnPieceReachedDrop += HandlePieceReachedDrop;
        conveyor.Clear();
        for (int i = 0; i < conveyor.SlotCount; i++)
            AddPieceToConveyor(NextFromBag());
    }

    public void ReleaseFirstPiece() {
        conveyor.ReleaseFront();
    }

    public void BeginBuildEndExit(float exitDuration) {
        conveyorDispensingStopped = true;
        conveyorRig.ExitOffScreen(exitDuration);

        craneBlocked = true;
        crane.StopFetching();
        craneRig.ExitOffScreen(exitDuration);
    }

    public void SpawnCargo(bool controllable = true) {
        if (craneBlocked || !crane.IsReady)
            return;
        if (Active != null)
            DiscardActive();

        GameObject cargo = crane.ReleaseHeld();
        BecomeActiveFallingPiece(cargo, controllable);
        rocket.SetCargoPiece(cargo.GetComponent<Piece>());

        FetchNextCargo();
    }

    private void DiscardActive() {
        Active.OnReleased -= HandleReleased;
        Destroy(Active.gameObject);
        Active = null;
    }

    public void ResetCargo() {
        nextCargoIndex = 0;
        craneBlocked = false;
        craneRig.ResetPosition();
        craneRig.BeginFollowingRocketTop();
        crane.ResetCrane();
        FetchNextCargo();
    }

    private void FetchNextCargo() {
        if (nextCargoIndex >= cargoPrefabs.Count)
            return;

        GameObject prefab = cargoPrefabs[nextCargoIndex];
        nextCargoIndex++;

        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.GetComponent<FallingPieceController>().enabled = false;
        crane.Fetch(instance);
    }

    private void AddPieceToConveyor(GameObject prefab) {
        GameObject instance = Instantiate(prefab, Vector3.zero, Quaternion.identity);
        instance.GetComponent<FallingPieceController>().enabled = false;
        conveyor.Enqueue(instance);
    }

    private void BecomeActiveFallingPiece(GameObject instance, bool controllable = true) {
        instance.transform.SetParent(rocket.transform, worldPositionStays: true);

        var controller = instance.GetComponent<FallingPieceController>();
        controller.enabled = true;
        controller.SetControllable(controllable);
        controller.SetBounds(wellMinX, wellMaxX);
        controller.SetLockCeiling(instance.transform.position.y);
        controller.SnapToMovementStep();
        controller.OnReleased += HandleReleased;
        Active = controller;
    }

    private void HandlePieceReachedDrop(GameObject instance) {
        BecomeActiveFallingPiece(instance);

        if (!conveyorDispensingStopped)
            AddPieceToConveyor(NextFromBag());
    }

    private void HandleReleased() {
        Active.OnReleased -= HandleReleased;
        Active = null;
        if (!conveyorDispensingStopped)
            conveyor.ReleaseFront();
    }

    public void ForceLockActive() {
        if (Active == null)
            return;
        Active.OnReleased -= HandleReleased;
        Active.ForceLock();
        Active = null;
    }

    private GameObject NextFromBag() {
        if (bag.Count == 0)
            RefillBag();

        int lastIndex = bag.Count - 1;
        GameObject prefab = bag[lastIndex];
        bag.RemoveAt(lastIndex);
        return prefab;
    }

    private void RefillBag() {
        bag.Clear();

        float totalChance = 0f;
        foreach (var entry in pool)
            totalChance += entry.chance;

        Debug.Log($"PieceSpawner: Refilling bag (bag size={bagSize}, total chance={totalChance}");

        int remaining = bagSize;
        for (int i = 0; i < pool.Count; i++) {
            int count;
            if (i == pool.Count - 1) {
                // last entry takes whatever's left, avoiding rounding shortfalls
                count = remaining;
            } else {
                float normalizedChance = totalChance > 0f ? pool[i].chance / totalChance : 0f;
                count = Mathf.Clamp(Mathf.RoundToInt(bagSize * normalizedChance), 0, remaining);
            }

            for (int j = 0; j < count; j++)
                bag.Add(pool[i].prefab);

            remaining -= count;
        }

        for (int i = bag.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }
    }
}