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
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private List<AvailablePiece> availablePieces;
    [SerializeField] private GameObject cargoPrefab;

    [SerializeField] private int bagSize = 7;
    [SerializeField] private float wellMinX = -2f;
    [SerializeField] private float wellMaxX = 2f;

    private readonly List<GameObject> bag = new List<GameObject>();

    public FallingPieceController Active { get; private set; }
    public bool HasCargoBeenDropped { get; private set; }

    public void SpawnNext() {
        SpawnPiece(NextFromBag());
    }

    public void SpawnCargo() {
        if (HasCargoBeenDropped)
            return;
        if (Active != null)
            DiscardActive();

        HasCargoBeenDropped = true;
        var controller = SpawnPiece(cargoPrefab);
        rocket.SetCargoPiece(controller.GetComponent<Piece>());
    }

    private void DiscardActive() {
        Active.OnReleased -= HandleReleased;
        Destroy(Active.gameObject);
        Active = null;
    }

    public void ResetCargo() {
        HasCargoBeenDropped = false;
    }

    private FallingPieceController SpawnPiece(GameObject prefab) {
        GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity, rocket.transform);

        var controller = instance.GetComponent<FallingPieceController>();
        controller.SetBounds(wellMinX, wellMaxX);
        controller.SetLockCeiling(spawnPoint.position.y);
        controller.SetRocket(rocket);
        controller.OnReleased += HandleReleased;
        Active = controller;
        return controller;
    }

    private void HandleReleased() {
        Active.OnReleased -= HandleReleased;
        Active = null;
        SpawnNext();
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
        foreach (var entry in availablePieces)
            totalChance += entry.chance;

        int remaining = bagSize;
        for (int i = 0; i < availablePieces.Count; i++) {
            int count;
            if (i == availablePieces.Count - 1) {
                // last entry takes whatever's left, avoiding rounding shortfalls
                count = remaining;
            } else {
                float normalizedChance = totalChance > 0f ? availablePieces[i].chance / totalChance : 0f;
                count = Mathf.Clamp(Mathf.RoundToInt(bagSize * normalizedChance), 0, remaining);
            }

            for (int j = 0; j < count; j++)
                bag.Add(availablePieces[i].prefab);

            remaining -= count;
        }

        for (int i = bag.Count - 1; i > 0; i--) {
            int j = Random.Range(0, i + 1);
            (bag[i], bag[j]) = (bag[j], bag[i]);
        }
    }
}