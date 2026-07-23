using UnityEngine;

public class ResourcePickup : MonoBehaviour
{
    [SerializeField] private int amount;

    public int Amount => amount;

    public void SetAmount(int newAmount)
    {
        amount = Mathf.Max(0, newAmount);
    }
}
