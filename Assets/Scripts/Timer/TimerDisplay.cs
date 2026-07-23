using System;
using UnityEngine;
using TMPro;

public class TimerDisplay : MonoBehaviour
{
    [SerializeField] private Timer timer;
    [SerializeField] private TextMeshProUGUI label;
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
        var span = TimeSpan.FromSeconds(Math.Max(0f, timeRemaining));
        label.text = span.ToString(timeSpanFormat);
    }
}