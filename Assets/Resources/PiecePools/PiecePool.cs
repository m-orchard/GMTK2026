using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// A pool of pieces used to refill the bag, organized into groups.
///
/// - Groups compete for space in the bag, weighted by each group's total
///   chance (sum of its members' chance values).
/// - Members within a group compete for that group's share, weighted by
///   their individual chance.
/// - Chance values are relative, not percentages — only the ratio between
///   them matters.
/// - A small bagSize can make a low-weight group "exclusive" (only one
///   member spawns per refill, picked randomly); a larger bagSize allows
///   more of each group to appear.
/// </summary>
[CreateAssetMenu(fileName = "NewPiecePool", menuName = "Piece Spawner/Piece Pool")]
public class PiecePool : ScriptableObject {
    public string poolName; // editor/debug label only
    public int bagSize;
    public List<AvailablePiece> members;
    public List<GameObject> overrides;
}

[System.Serializable]
public struct AvailablePiece
{
    public GameObject prefab;

    public float chance;

    public int group;
}