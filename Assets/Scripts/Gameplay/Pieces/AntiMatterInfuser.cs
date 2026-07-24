using UnityEngine;

public class AntiMatterInfuser : MonoBehaviour
{
    [SerializeField] private float MassAdjustmentValue = 0;
    [SerializeField] private float MassAdjustmentMultiplier = 0.8f;

    public void OnWeld(Piece target)
    {
        var body = target.GetComponent<Rigidbody2D>();
        var currentMass = body.mass;
        var newMass = (currentMass * MassAdjustmentMultiplier) + MassAdjustmentValue;
        Debug.Log($"AntiMatterInfuser: Applying mass adjustment (({currentMass} * {MassAdjustmentMultiplier}) + {MassAdjustmentValue} = {newMass}");
        body.mass = newMass;
    }
}
