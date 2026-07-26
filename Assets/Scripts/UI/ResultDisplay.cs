using UnityEngine;
using TMPro;

public class ResultDisplay : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private GameObject panel;

    private void OnEnable()
    {
        gameManager.OnRoundResult += UpdateLabel;
        gameManager.OnBuildingStarted += HidePanel;
        if (panel != null) panel.SetActive(false);
    }

    private void OnDisable()
    {
        gameManager.OnRoundResult -= UpdateLabel;
        gameManager.OnBuildingStarted -= HidePanel;
    }

    private void UpdateLabel(float apex, float target, bool success)
    {
        if (success)
        {
            HidePanel();
            return;
        }

        if (panel != null) panel.SetActive(true);

        label.text = $"Reached {apex:0.0}m / target {target:0.0}m — FAIL\nPress Space to continue";
    }

    private void HidePanel()
    {
        if (panel != null) panel.SetActive(false);
    }
}
