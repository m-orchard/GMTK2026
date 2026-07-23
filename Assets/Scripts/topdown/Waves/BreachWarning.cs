using UnityEngine;
using UnityEngine.UI;

public class BreachWarning : MonoBehaviour
{
    [Header("Trigger")]
    [Tooltip("Warn when the next breach is within this many seconds.")]
    [SerializeField, Min(0f)] private float warningThreshold = 10f;

    [Header("Screen Flash")]
    [Tooltip("Full-screen red overlay. Its RGB is kept; only the alpha is driven.")]
    [SerializeField] private Image redOverlay;

    [Tooltip("Peak opacity of the red flash at the top of each pulse.")]
    [SerializeField, Range(0f, 1f)] private float maxAlpha = 0.5f;

    [Tooltip("Seconds for one fade in-and-out cycle.")]
    [SerializeField, Min(0.05f)] private float pulsePeriod = 1f;

    [Header("Audio")]
    [SerializeField] private AudioClip warningSfx;

    [SerializeField] private AudioClip secondaryWarningSfx;

    [Tooltip("Seconds after the alarm before the secondary sound plays. Skipped if the breach lands first.")]
    [SerializeField, Min(0f)] private float secondaryWarningDelay = 2f;

    [Tooltip("Volume for the secondary sound. Above 1 amplifies past the clip's recorded level.")]
    [SerializeField, Min(0f)] private float secondaryWarningVolume = 2f;

    private bool isWarning;
    private float pulseTime;
    private float secondaryWarningTime;
    private bool secondaryWarningPlayed;

    private void Awake()
    {
        StopWarning();
    }

    private void Update()
    {
        if (IsBreachImminent())
        {
            if (!isWarning)
            {
                StartWarning();
            }

            TickWarning();
        }
        else if (isWarning)
        {
            StopWarning();
        }
    }

    private bool IsBreachImminent()
    {
        WaveManager waveManager = WaveManager.Instance;
        if (waveManager == null)
        {
            return false;
        }

        return waveManager.TryGetTimeUntilNextBreach(out float timeRemaining)
            && timeRemaining <= warningThreshold;
    }

    private void StartWarning()
    {
        isWarning = true;
        pulseTime = 0f;
        secondaryWarningPlayed = false;
        secondaryWarningTime = Time.unscaledTime + secondaryWarningDelay;
        AudioManager.Instance.PlaySound(warningSfx);

        if (redOverlay != null)
        {
            redOverlay.enabled = true;
        }
    }

    private void TickWarning()
    {
        pulseTime += Time.unscaledDeltaTime;
        SetOverlayAlpha(PulseAlpha() * maxAlpha);

        if (!secondaryWarningPlayed && Time.unscaledTime >= secondaryWarningTime)
        {
            AudioManager.Instance.PlaySound(secondaryWarningSfx, new AudioClipOptions { Volume = secondaryWarningVolume });
            secondaryWarningPlayed = true;
        }
    }

    private float PulseAlpha()
    {
        return 0.5f - 0.5f * Mathf.Cos(pulseTime / pulsePeriod * 2f * Mathf.PI);
    }

    private void StopWarning()
    {
        isWarning = false;
        SetOverlayAlpha(0f);

        if (redOverlay != null)
        {
            redOverlay.enabled = false;
        }
    }

    private void SetOverlayAlpha(float alpha)
    {
        if (redOverlay == null)
        {
            return;
        }

        Color color = redOverlay.color;
        color.a = alpha;
        redOverlay.color = color;
    }
}
