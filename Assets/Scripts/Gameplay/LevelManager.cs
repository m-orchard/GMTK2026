using System;
using System.Collections.Generic;
using UnityEngine;

public enum TargetHeightProgression
{
    Add,
    Multiply
}

public class LevelManager : Singleton<LevelManager> {
    [SerializeField] private int initialLevel = 1;
    [SerializeField] private List<PiecePool> levelPools = new();

    [SerializeField] private float startingTargetHeight = 8f;
    [Tooltip("How the target height grows each level. Add: increases by the increment. Multiply: scales by the multiplier (for example 1.5x each level).")]
    [SerializeField] private TargetHeightProgression targetHeightProgression = TargetHeightProgression.Add;
    [Tooltip("Amount added to the target height per level when the progression is set to Add.")]
    [SerializeField] private float targetHeightIncrement = 4f;
    [Tooltip("Factor the target height is multiplied by per level when the progression is set to Multiply (for example 1.5).")]
    [SerializeField] private float targetHeightMultiplier = 1.5f;

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
        TargetHeight = CalculateTargetHeight(level);
        BuildDuration = startingBuildDuration + ((level - 1) * buildDurationIncrement);
        int poolIndex = Math.Min(CurrentLevel - 1, levelPools.Count - 1);
        currentPool = levelPools[poolIndex];
        Debug.Log($"[LevelManager] Setting level to {CurrentLevel} (target height={TargetHeight}, build duration={BuildDuration}, pool index={poolIndex}, pool={CurrentPool.poolName})");
        OnLevelChanged?.Invoke(CurrentLevel);
    }

    private float CalculateTargetHeight(int level)
    {
        int levelsCompleted = level - 1;

        if (targetHeightProgression == TargetHeightProgression.Multiply)
            return startingTargetHeight * Mathf.Pow(targetHeightMultiplier, levelsCompleted);

        return startingTargetHeight + (levelsCompleted * targetHeightIncrement);
    }
}
