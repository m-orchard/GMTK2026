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

    public int CurrentLevel { get; private set; }
    public float TargetHeight { get; private set; }
    public float BuildDuration { get; private set; }
    public PiecePool CurrentPool => currentPool;

    public event Action<int> OnLevelChanged;

    public void ResetToFirstLevel() {
        SetLevel(initialLevel);
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
        Debug.Log($"[LevelManager] {CurrentLevel-1}, {levelPools.Count - 1}, {Math.Min(CurrentLevel - 1, levelPools.Count - 1)}");
        Debug.Log($"[LevelManager] Setting level to {CurrentLevel} (target height={TargetHeight}, build duration={BuildDuration}, pool index={poolIndex}, pool={CurrentPool.poolName})");
        OnLevelChanged?.Invoke(CurrentLevel);
    }
}
