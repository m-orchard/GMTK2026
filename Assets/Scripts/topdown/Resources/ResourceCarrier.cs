using UnityEngine;

public class ResourceCarrier : MonoBehaviour
{
    private int carriedAmount;

    public int CarriedAmount => carriedAmount;
    public bool HasResources => carriedAmount > 0;

    public void Collect(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        carriedAmount += amount;
    }
}
