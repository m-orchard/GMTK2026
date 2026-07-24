using UnityEngine;

public class LaunchCountdownManager : Singleton<LaunchCountdownManager>
{
    [System.Serializable]
    private class CountdownVoice
    {
        public int number;
        public AudioClip clip;
    }

    [SerializeField] private Timer launchTimer;
    [SerializeField] private GameObject smallDisplay;
    [SerializeField] private SlammingNumber slammingNumberPrefab;
    [SerializeField] private RectTransform slammingNumberParent;
    [SerializeField] private int countdownStartSecond = 5;
    [SerializeField] private CountdownVoice[] countdownVoices;

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
        SlammingNumber slammingNumber = Instantiate(slammingNumberPrefab, slammingNumberParent);
        slammingNumber.Play(numberToDisplay, FindVoiceForNumber(numberToDisplay));
    }

    private AudioClip FindVoiceForNumber(int numberToDisplay)
    {
        foreach (CountdownVoice countdownVoice in countdownVoices)
        {
            if (countdownVoice.number == numberToDisplay)
            {
                return countdownVoice.clip;
            }
        }

        return null;
    }
}
