using System;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    private enum DisplayStyle { Precise, MinutesThenSeconds }

    [SerializeField] private Timer timer;
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private DisplayStyle displayStyle = DisplayStyle.Precise;
    [SerializeField] private string timeSpanFormat = @"mm\:ss\.fff";

    private void OnEnable()
    {
        timer.OnTimeUpdated += UpdateLabel;
        UpdateLabel(timer.TimeRemaining);
    }

    private void OnDisable()
    {
        timer.OnTimeUpdated -= UpdateLabel;
    }

    private void UpdateLabel(float timeRemaining)
    {
        label.text = displayStyle == DisplayStyle.MinutesThenSeconds
            ? FormatMinutesThenSeconds(timeRemaining)
            : FormatPrecise(timeRemaining);
    }

    private string FormatPrecise(float timeRemaining)
    {
        var span = TimeSpan.FromSeconds(Math.Max(0f, timeRemaining));
        return span.ToString(timeSpanFormat);
    }

    private string FormatMinutesThenSeconds(float timeRemaining)
    {
        int totalSeconds = Mathf.RoundToInt(Mathf.Max(0f, timeRemaining));
        if (totalSeconds >= 60)
        {
            return $"~{totalSeconds / 60} min";
        }

        return $"{totalSeconds}s";
    }
}
