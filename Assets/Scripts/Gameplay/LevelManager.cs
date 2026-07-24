using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {
    [SerializeField] private List<AvailablePiece> startingPieces = new List<AvailablePiece>();
    [SerializeField] private List<AvailablePiece> addablePieces = new List<AvailablePiece>();

    private readonly List<AvailablePiece> currentPool = new List<AvailablePiece>();
    private readonly List<AvailablePiece> unusedAddablePieces = new List<AvailablePiece>();

    public int CurrentLevel { get; private set; }
    public IReadOnlyList<AvailablePiece> CurrentPool => currentPool;

    public event Action<int> OnLevelChanged;

    public void ResetToFirstLevel() {
        CurrentLevel = 1;

        currentPool.Clear();
        currentPool.AddRange(startingPieces);

        unusedAddablePieces.Clear();
        unusedAddablePieces.AddRange(addablePieces);

        OnLevelChanged?.Invoke(CurrentLevel);
    }

    public void AdvanceLevel() {
        CurrentLevel++;
        AddRandomUnusedPieceToPool();
        OnLevelChanged?.Invoke(CurrentLevel);
    }

    private void AddRandomUnusedPieceToPool() {
        while (unusedAddablePieces.Count > 0) {
            int index = UnityEngine.Random.Range(0, unusedAddablePieces.Count);
            AvailablePiece candidate = unusedAddablePieces[index];
            unusedAddablePieces.RemoveAt(index);

            if (PoolContainsPrefab(candidate.prefab))
                continue;

            currentPool.Add(candidate);
            return;
        }
    }

    private bool PoolContainsPrefab(GameObject prefab) {
        foreach (AvailablePiece pooledPiece in currentPool) {
            if (pooledPiece.prefab == prefab)
                return true;
        }

        return false;
    }
}
