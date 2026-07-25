using UnityEngine;

public class NuclearReactor : MonoBehaviour
{
    [SerializeField] private float BoostValue = 0;
    [SerializeField] private float BoostMultiplier = 1.2f;

    public void OnWeld(Piece target)
    {
        if (target.TryGetComponent<EngineThrustEffect>(out var thrustEffect))
        {
            var currentThrust = thrustEffect.Thrust;
            var newThrust = (currentThrust * BoostMultiplier) + BoostValue;
            Debug.Log($"[NuclearReactor] Applying thruster boost (({currentThrust} * {BoostMultiplier}) + {BoostValue}) = {newThrust}");
            thrustEffect.Thrust = newThrust;
        }
    }
}
