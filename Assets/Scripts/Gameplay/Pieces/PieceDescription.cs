using UnityEngine;

public class PieceDescription : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField] private Sprite icon;
    [SerializeField, TextArea(3, 8)] private string howItWorks;

    public string DisplayName => displayName;
    public Sprite Icon => icon;
    public string HowItWorks => howItWorks;
}
