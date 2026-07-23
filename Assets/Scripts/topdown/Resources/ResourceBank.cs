public class ResourceBank : Singleton<ResourceBank>
{
    private int total;

    public int Total => total;

    public event System.Action<int> OnChanged;

    public void Add(int amount)
    {
        if (amount <= 0)
        {
            return;
        }

        total += amount;
        OnChanged?.Invoke(total);
    }

    public bool TrySpend(int amount)
    {
        if (amount <= 0 || total < amount)
        {
            return false;
        }

        total -= amount;
        OnChanged?.Invoke(total);
        return true;
    }
}
