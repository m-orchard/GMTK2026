using UnityEngine;

[RequireComponent(typeof(Piece))]
public class AntiMatterInfuser : MonoBehaviour
{
    [SerializeField] private float MassAdjustmentValue = 0;
    [SerializeField] private float MassAdjustmentMultiplier = 0.8f;

    private Piece piece;

    public void Awake()
    {
        piece = GetComponent<Piece>();
    }

    public void OnWeld(PieceWeld weld)
    {
        var target = piece == weld.parent ? weld.child : weld.parent;
        var body = target.GetComponent<Rigidbody2D>();
        var currentMass = body.mass;
        var newMass = (currentMass * MassAdjustmentMultiplier) + MassAdjustmentValue;
        Debug.Log($"[AntiMatterInfuser] Applying mass adjustment (({currentMass} * {MassAdjustmentMultiplier}) + {MassAdjustmentValue} = {newMass}");
        body.mass = newMass;
    }
}
