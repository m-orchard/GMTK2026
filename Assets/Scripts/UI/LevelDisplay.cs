using UnityEngine;
using TMPro;

public class LevelDisplay : MonoBehaviour {
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private string format = "Level {0}";

    private void OnEnable() {
        LevelManager.Instance.OnLevelChanged += UpdateLabel;
        UpdateLabel(LevelManager.Instance.CurrentLevel);
    }

    private void OnDisable() {
        if (!LevelManager.Instance) {
            return;
        }

        LevelManager.Instance.OnLevelChanged -= UpdateLabel;
    }

    private void UpdateLabel(int level) {
        label.text = string.Format(format, level);
    }
}