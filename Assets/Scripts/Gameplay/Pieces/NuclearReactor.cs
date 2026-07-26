using UnityEngine;

[RequireComponent(typeof(Piece))]
public class NuclearReactor : MonoBehaviour
{
    [SerializeField] private float BoostValue = 0;
    [SerializeField] private float BoostMultiplier = 1.2f;

    private Piece piece;

    public void Awake()
    {
        piece = GetComponent<Piece>();
    }

    public void OnWeld(PieceWeld weld)
    {
        var target = piece == weld.parent ? weld.child : weld.parent;

        if (target.TryGetComponent<EngineThrustEffect>(out var thrustEffect))
        {
            var currentThrust = thrustEffect.Thrust;
            var newThrust = (currentThrust * BoostMultiplier) + BoostValue;
            Debug.Log($"[NuclearReactor] Applying thruster boost (({currentThrust} * {BoostMultiplier}) + {BoostValue}) = {newThrust}");
            thrustEffect.Thrust = newThrust;
            thrustEffect.Empower();
        }
    }
}
