using UnityEngine;
using UnityEngine.Serialization;

public class LaunchCountdownManager : Singleton<LaunchCountdownManager>
{
    [System.Serializable]
    private class CountdownStep
    {
        public int number;
        public AudioClip clip;
        [FormerlySerializedAs("numberColor")]
        public Color topColor = Color.white;
        public Color bottomColor = Color.white;
    }

    [SerializeField] private Timer launchTimer;
    [SerializeField] private GameObject smallDisplay;
    [SerializeField] private SlammingNumber slammingNumberPrefab;
    [SerializeField] private RectTransform slammingNumberParent;
    [SerializeField] private int countdownStartSecond = 5;
    [SerializeField] private int emitThrusterPuffAtSecond = 3;
    [FormerlySerializedAs("countdownVoices")]
    [SerializeField] private CountdownStep[] countdownSteps;

    private int nextNumberToSlam;

    private void OnEnable()
    {
        launchTimer.OnTimerStarted += ResetCountdown;
        launchTimer.OnTimeUpdated += SlamNumbersAsTimeElapses;
    }

    private void OnDisable()
    {
        launchTimer.OnTimerStarted -= ResetCountdown;
        launchTimer.OnTimeUpdated -= SlamNumbersAsTimeElapses;
    }

    private void Start()
    {
        ResetCountdown();
    }

    private void ResetCountdown()
    {
        SetSmallDisplayVisible(true);
        nextNumberToSlam = countdownStartSecond;
    }

    private void SetSmallDisplayVisible(bool isVisible)
    {
        if (smallDisplay == null)
        {
            return;
        }

        smallDisplay.SetActive(isVisible);
    }

    private void SlamNumbersAsTimeElapses(float timeRemaining)
    {
        while (nextNumberToSlam >= 1 && timeRemaining <= nextNumberToSlam)
        {
            SetSmallDisplayVisible(false);
            SlamNumber(nextNumberToSlam);
            nextNumberToSlam--;
        }
    }

    private void SlamNumber(int numberToDisplay)
    {
        CountdownStep step = FindStepForNumber(numberToDisplay);
        AudioClip voiceClip = step != null ? step.clip : null;
        Color topColor = step != null ? step.topColor : Color.white;
        Color bottomColor = step != null ? step.bottomColor : Color.white;

        SlammingNumber slammingNumber = Instantiate(slammingNumberPrefab, slammingNumberParent);
        slammingNumber.Play(numberToDisplay, voiceClip, topColor, bottomColor);

        if (numberToDisplay == emitThrusterPuffAtSecond)
        {
            EmitThrusterPuffs();
        }
    }

    private void EmitThrusterPuffs()
    {
        if (RocketAssembly.Instance == null)
        {
            return;
        }

        EngineThrustEffect[] thrusters = RocketAssembly.Instance.GetComponentsInChildren<EngineThrustEffect>();
        foreach (EngineThrustEffect thruster in thrusters)
        {
            thruster.EmitPuff();
        }
    }

    private CountdownStep FindStepForNumber(int numberToDisplay)
    {
        foreach (CountdownStep countdownStep in countdownSteps)
        {
            if (countdownStep.number == numberToDisplay)
            {
                return countdownStep;
            }
        }

        return null;
    }
}
