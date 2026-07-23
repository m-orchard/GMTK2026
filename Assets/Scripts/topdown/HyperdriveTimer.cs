using UnityEngine;

public class HyperdriveTimer : Singleton<HyperdriveTimer>
{
    [SerializeField] private Timer timer;

    [Tooltip("Parent of the countdown text. Hidden until launch, shown once the countdown starts.")]
    [SerializeField] private GameObject timerDisplayRoot;

    [Tooltip("Message telling the player to pick a destination. Shown before launch, hidden once the countdown starts.")]
    [SerializeField] private GameObject pickDestinationPrompt;

    public bool IsRunning => timer.IsRunning;
    public float TimeRemaining => timer.TimeRemaining;

    private void Start()
    {
        ShowPickDestinationPrompt(true);
        ShowTimerDisplay(false);

        if (GameSession.Instance == null)
        {
            return;
        }

        GameSession.Instance.OnGameStarted += HandleGameStarted;

        if (GameSession.Instance.HasStarted)
        {
            HandleGameStarted();
        }
    }

    private void OnDestroy()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameStarted -= HandleGameStarted;
        }
    }

    public void Pause() => timer.Pause();
    public void Resume() => timer.Resume();
    public void TogglePause() => timer.TogglePause();
    public void SetSpeed(float speed) => timer.SetTimeScale(speed);
    public void AddTime(float seconds) => timer.AddTime(seconds);
    public void RemoveTime(float seconds) => timer.AddTime(-seconds);

    private void HandleGameStarted()
    {
        ShowPickDestinationPrompt(false);
        ShowTimerDisplay(true);
        timer.StartTimer();
    }

    private void ShowPickDestinationPrompt(bool visible)
    {
        if (pickDestinationPrompt != null)
        {
            pickDestinationPrompt.SetActive(visible);
        }
    }

    private void ShowTimerDisplay(bool visible)
    {
        if (timerDisplayRoot != null)
        {
            timerDisplayRoot.SetActive(visible);
        }
    }
}
