using UnityEngine;

public class PieceDescription : MonoBehaviour
{
    [SerializeField] private string displayName;
    [SerializeField, TextArea(3, 8)] private string howItWorks;

    public string DisplayName => displayName;
    public string HowItWorks => howItWorks;
}
