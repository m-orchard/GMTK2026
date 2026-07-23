using UnityEngine;
using TMPro;

public class ResourceCountDisplay : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string format = "{0}";

    private void OnEnable()
    {
        if (ResourceBank.Instance == null)
        {
            return;
        }

        ResourceBank.Instance.OnChanged += UpdateLabel;
        UpdateLabel(ResourceBank.Instance.Total);
    }

    private void OnDisable()
    {
        if (ResourceBank.Instance == null)
        {
            return;
        }

        ResourceBank.Instance.OnChanged -= UpdateLabel;
    }

    private void UpdateLabel(int total)
    {
        label.text = string.Format(format, total);
    }
}
