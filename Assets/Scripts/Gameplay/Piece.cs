using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct PieceWeld
{
    public Piece parent;
    public Piece child;

    public FixedJoint2D joint;
}

[RequireComponent(typeof(Rigidbody2D))]
public class Piece : MonoBehaviour
{
    public bool IsLocked { get; private set; }
    public bool IsConnected { get; private set; }
    public Rigidbody2D Body2D { get; private set; }

    // Welds to child pieces (i.e. this is the parent)
    private readonly List<PieceWeld> childPieceWelds = new();

    // Welds to parent pieces (i.e. this is the child)
    private readonly List<PieceWeld> parentPieceWelds = new();

    public UnityEvent<Piece> OnWeld;

    private void Awake()
    {
        Body2D = GetComponent<Rigidbody2D>();
    }

    public void Lock()
    {
        IsLocked = true;
    }

    public void MarkConnected()
    {
        IsConnected = true;
    }

    public FixedJoint2D WeldTo(Rigidbody2D other)
    {
        var weld = gameObject.AddComponent<FixedJoint2D>();
        weld.autoConfigureConnectedAnchor = true;
        weld.connectedBody = other;
        weld.frequency = 0f;
        return weld;
    }

    public FixedJoint2D WeldTo(Piece other)
    {
        var weld = WeldTo(other.GetComponent<Rigidbody2D>());
        var pieceWeld = new PieceWeld { parent = this, child = other, joint = weld };
        WeldAsParent(pieceWeld);
        other.WeldAsChild(pieceWeld);
        return weld;
    }

    public void WeldAsParent(PieceWeld weld)
    {
        childPieceWelds.Add(weld);
        OnWeld?.Invoke(weld.child);
    }

    public void WeldAsChild(PieceWeld weld)
    {
        parentPieceWelds.Add(weld);
        OnWeld?.Invoke(weld.parent);
    }

    public void DetachAsParent(PieceWeld weld)
    {
        childPieceWelds.Remove(weld);
        Destroy(weld.joint);
    }

    public void DetachAsChild(PieceWeld weld)
    {
        parentPieceWelds.Remove(weld);
    }

    public void BreakWelds()
    {
        foreach (var weld in childPieceWelds)
        {
            weld.child.DetachAsChild(weld);
            Destroy(weld.joint);
        }

        childPieceWelds.Clear();

        foreach (var weld in parentPieceWelds)
        {
            weld.parent.DetachAsParent(weld);
        }

        parentPieceWelds.Clear();
    }
}
