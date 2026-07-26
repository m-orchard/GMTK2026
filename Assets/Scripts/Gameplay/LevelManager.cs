using System;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : Singleton<LevelManager> {
    [SerializeField] private int initialLevel = 1;
    [SerializeField] private List<PiecePool> levelPools = new();

    [SerializeField] private float startingTargetHeight = 8f;
    [SerializeField] private float targetHeightIncrement = 4f;

    [SerializeField] private float startingBuildDuration = 60f;
    [SerializeField] private float buildDurationIncrement = 5f;

    private PiecePool currentPool;

    private readonly List<GameObject> acquiredPieces = new();

    public int CurrentLevel { get; private set; }
    public float TargetHeight { get; private set; }
    public float BuildDuration { get; private set; }
    public PiecePool CurrentPool => currentPool;
    public IReadOnlyList<GameObject> AcquiredPieces => acquiredPieces;

    public event Action<int> OnLevelChanged;

    public void ResetToFirstLevel() {
        acquiredPieces.Clear();
        SetLevel(initialLevel);
    }

    public void AcquirePiece(GameObject piecePrefab) {
        if (piecePrefab == null) return;
        acquiredPieces.Add(piecePrefab);
    }

    public void AdvanceLevel() {
        SetLevel(CurrentLevel + 1);
    }

    private void SetLevel(int level)
    {
        CurrentLevel = level;
        TargetHeight = startingTargetHeight + ((level - 1) * targetHeightIncrement);
        BuildDuration = startingBuildDuration + ((level - 1) * buildDurationIncrement);
        int poolIndex = Math.Min(CurrentLevel - 1, levelPools.Count - 1);
        currentPool = levelPools[poolIndex];
        Debug.Log($"[LevelManager] Setting level to {CurrentLevel} (target height={TargetHeight}, build duration={BuildDuration}, pool index={poolIndex}, pool={CurrentPool.poolName})");
        OnLevelChanged?.Invoke(CurrentLevel);
    }
}
