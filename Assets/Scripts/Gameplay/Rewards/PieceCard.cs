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

        Sprite icon = ResolveIcon(piecePrefab);

        var description = piecePrefab.GetComponentInChildren<PieceDescription>(true);
        string displayName = description != null && !string.IsNullOrEmpty(description.DisplayName)
            ? description.DisplayName
            : piecePrefab.name;
        string howItWorks = description != null ? description.HowItWorks : string.Empty;

        return new PieceCard(piecePrefab, icon, displayName, howItWorks);
    }

    private static Sprite ResolveIcon(GameObject piecePrefab)
    {
        Sprite fallback = null;

        foreach (var spriteRenderer in piecePrefab.GetComponentsInChildren<SpriteRenderer>(true))
        {
            if (spriteRenderer.sprite == null)
                continue;

            if (spriteRenderer.enabled)
                return spriteRenderer.sprite;

            if (fallback == null)
                fallback = spriteRenderer.sprite;
        }

        return fallback;
    }
}
