using System;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour {
    [SerializeField] private Timer timer;
    [SerializeField] private TextMeshProUGUI label;

    [Tooltip("TimeSpan custom format string, e.g. mm\\:ss or mm\\:ss\\.f\n" +
             "Docs: https://learn.microsoft.com/dotnet/standard/base-types/custom-timespan-format-strings")]
    [SerializeField] private string timeSpanFormat = @"mm\:ss\.fff";

    [SerializeField] private bool useDynamicDisplay = true;

    private void OnEnable() {
        timer.OnTimeUpdated += UpdateLabel;
        UpdateLabel(timer.TimeRemaining);
    }

    private void OnDisable() {
        timer.OnTimeUpdated -= UpdateLabel;
    }

    private void UpdateLabel(float timeRemaining) {
        var span = TimeSpan.FromSeconds(Math.Max(0f, timeRemaining));

        var format = timeSpanFormat;
        if (useDynamicDisplay) {
            if (span.TotalSeconds > 60f) {
                format = @"mm\:ss";
            } else {
                format = @"ss";
            }
        }

        label.text = span.ToString(format);
    }
}