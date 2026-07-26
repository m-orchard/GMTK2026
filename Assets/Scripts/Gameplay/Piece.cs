using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public struct PieceWeld
{
    public Piece parent;
    public Piece child;

    public FixedJoint2D joint;

    public GameObject weldMarker;
}

[RequireComponent(typeof(Rigidbody2D))]
public class Piece : MonoBehaviour
{
    [SerializeField] private int engineSupportCapacity = 1;

    public bool IsLocked { get; private set; }
    public Rigidbody2D Body2D { get; private set; }
    public int EngineSupportCapacity => engineSupportCapacity;

    // Welds to child pieces (i.e. this is the parent)
    private readonly List<PieceWeld> childPieceWelds = new();

    // Welds to parent pieces (i.e. this is the child)
    private readonly List<PieceWeld> parentPieceWelds = new();

    public IEnumerable<Piece> WeldedNeighbors
    {
        get
        {
            foreach (var weld in childPieceWelds) yield return weld.child;
            foreach (var weld in parentPieceWelds) yield return weld.parent;
        }
    }

    public UnityEvent<Piece> OnWeld;

    private void Awake()
    {
        Body2D = GetComponent<Rigidbody2D>();
    }

    public void Lock()
    {
        IsLocked = true;
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
        var weldPosition = ResolveContactPoint(other);
        var weldMarker = CreateWeldMarker(weldPosition);
        PlayWeldSound(weldPosition);
        var pieceWeld = new PieceWeld { parent = this, child = other, joint = weld, weldMarker = weldMarker };
        WeldAsParent(pieceWeld);
        other.WeldAsChild(pieceWeld);
        return weld;
    }

    private GameObject CreateWeldMarker(Vector3 weldPosition)
    {
        var weldMarkerPrefab = RocketAssembly.Instance.WeldMarkerPrefab;
        if (weldMarkerPrefab == null) return null;

        return Instantiate(weldMarkerPrefab, weldPosition, Quaternion.identity, transform);
    }

    private void PlayWeldSound(Vector3 weldPosition)
    {
        if (AudioManager.Instance == null) return;

        AudioManager.Instance.PlaySound(RocketAssembly.Instance.RandomPieceWeldedSound(), weldPosition);
    }

    private Vector3 ResolveContactPoint(Piece other)
    {
        var ownCollider = GetComponentInChildren<Collider2D>();
        var otherCollider = other.GetComponentInChildren<Collider2D>();
        if (ownCollider == null || otherCollider == null)
        {
            return (transform.position + other.transform.position) * 0.5f;
        }

        var distance = Physics2D.Distance(ownCollider, otherCollider);
        return (Vector3)(distance.pointA + distance.pointB) * 0.5f;
    }

    public void WeldAsParent(PieceWeld weld)
    {
        childPieceWelds.Add(weld);
        OnWeld?.Invoke(weld.child);
        RocketAssembly.Instance.UpdateRocket();
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
        if (weld.weldMarker != null) Destroy(weld.weldMarker);
        RocketAssembly.Instance.UpdateRocket();
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
            if (weld.weldMarker != null) Destroy(weld.weldMarker);
        }

        childPieceWelds.Clear();

        foreach (var weld in parentPieceWelds)
        {
            weld.parent.DetachAsParent(weld);
        }

        parentPieceWelds.Clear();
    }
}
