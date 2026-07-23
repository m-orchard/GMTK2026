using UnityEngine;

public class HyperdriveModulePower : MonoBehaviour
{
    private ShipModule[] modules;

    private void Start()
    {
        modules = FindObjectsByType<ShipModule>(FindObjectsSortMode.None);

        foreach (ShipModule module in modules)
        {
            module.OnBroken += UpdateHyperdriveSpeed;
            module.OnRestored += UpdateHyperdriveSpeed;
        }

        UpdateHyperdriveSpeed();
    }

    private void OnDestroy()
    {
        if (modules == null)
        {
            return;
        }

        foreach (ShipModule module in modules)
        {
            if (module != null)
            {
                module.OnBroken -= UpdateHyperdriveSpeed;
                module.OnRestored -= UpdateHyperdriveSpeed;
            }
        }
    }

    private void UpdateHyperdriveSpeed()
    {
        if (HyperdriveTimer.Instance == null || modules.Length == 0)
        {
            return;
        }

        HyperdriveTimer.Instance.SetSpeed(OnlineFraction());
    }

    private float OnlineFraction()
    {
        int onlineCount = 0;
        foreach (ShipModule module in modules)
        {
            if (module != null && module.IsOperational)
            {
                onlineCount++;
            }
        }

        return (float)onlineCount / modules.Length;
    }
}
