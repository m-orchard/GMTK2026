using UnityEngine;

public class NuclearReactor : MonoBehaviour
{
    [SerializeField] private float BoostValue = 0;
    [SerializeField] private float BoostMultiplier = 2;

    public void OnWeld(Piece target)
    {
        if (target.TryGetComponent<EngineThrustEffect>(out var thrustEffect))
        {
            thrustEffect.Thrust += BoostValue;
            thrustEffect.Thrust *= BoostMultiplier;
        }
    }
}
