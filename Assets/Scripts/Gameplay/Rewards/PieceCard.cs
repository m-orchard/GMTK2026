using UnityEngine;

public class PieceCard
{
    public GameObject PiecePrefab { get; }
    public Sprite Icon { get; }
    public string DisplayName { get; }
    public string HowItWorks { get; }

    private PieceCard(GameObject piecePrefab, Sprite icon, string displayName, string howItWorks)
    {
        PiecePrefab = piecePrefab;
        Icon = icon;
        DisplayName = displayName;
        HowItWorks = howItWorks;
    }

    public static PieceCard FromPrefab(GameObject piecePrefab)
    {
        if (piecePrefab == null)
            return null;

        var description = piecePrefab.GetComponentInChildren<PieceDescription>(true);

        Sprite icon = description != null ? description.Icon : null;
        string displayName = description != null && !string.IsNullOrEmpty(description.DisplayName)
            ? description.DisplayName
            : piecePrefab.name;
        string howItWorks = description != null ? description.HowItWorks : string.Empty;

        return new PieceCard(piecePrefab, icon, displayName, howItWorks);
    }
}
