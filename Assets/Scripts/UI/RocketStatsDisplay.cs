using TMPro;
using UnityEngine;

public class RocketStatsDisplay : MonoBehaviour
{
    public TMP_Text MassValue;
    public TMP_Text WeightValue;
    public TMP_Text FuelValue;
    public TMP_Text ThrustValue;

    // Update is called once per frame
    void LateUpdate()
    {
        MassValue.text = $"{RocketAssembly.Instance.Rocket.TotalMass:N2}kg";
        WeightValue.text = $"{RocketAssembly.Instance.Rocket.TotalWeight:N2}N";
        FuelValue.text = $"{RocketAssembly.Instance.Rocket.AvailableFuel:N2} gallons";
        ThrustValue.text = $"{RocketAssembly.Instance.Rocket.PotentialThrust:N2}N";
    }
}
