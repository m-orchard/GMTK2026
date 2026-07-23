using UnityEngine;

public class Timer : MonoBehaviour
{
    public enum Direction { CountDown, CountUp }

    [SerializeField] private float duration = 60f;
    [SerializeField] private bool autoStart = false;
    [SerializeField] private Direction direction = Direction.CountDown;

    private float timeRemaining; // for CountUp, this represents "elapsed"
    private bool isRunning;
    private bool isPaused;

    [SerializeField] private float reportInterval = 1f;
    private float nextReportTime;

    [SerializeField, Min(0f)] private float timeScale = 1f;

    public event System.Action<float> OnTimeUpdated;
    public event System.Action OnTimerComplete;
    public event System.Action OnTimerPaused;
    public event System.Action OnTimerResumed;
    public event System.Action OnTimerStarted;
    public event System.Action OnTimerStopped;

    public bool IsRunning => isRunning;
    public bool IsPaused => isPaused;
    public float TimeRemaining => timeRemaining;

    private void Awake()
    {
        ResetState();
    }

    private void Start()
    {
        if (autoStart) StartTimer();
    }

    private void Update()
    {
        if (!isRunning || isPaused) return;

        float dt = Time.unscaledDeltaTime * timeScale;
        timeRemaining += (direction == Direction.CountDown) ? -dt : dt;

        bool intervalPassed = direction == Direction.CountDown
            ? timeRemaining <= nextReportTime
            : timeRemaining >= nextReportTime;

        if (intervalPassed)
        {
            OnTimeUpdated?.Invoke(Mathf.Clamp(timeRemaining, 0f, duration));
            nextReportTime += (direction == Direction.CountDown) ? -reportInterval : reportInterval;
        }

        bool finished = direction == Direction.CountDown
            ? timeRemaining <= 0f
            : timeRemaining >= duration;

        if (finished)
        {
            timeRemaining = direction == Direction.CountDown ? 0f : duration;
            isRunning = false;
            OnTimerComplete?.Invoke();
        }
    }

    public void StartTimer()
    {
        if (isRunning) return;
        ResetState();
        isRunning = true;
        isPaused = false;
        OnTimerStarted?.Invoke();
        OnTimeUpdated?.Invoke(timeRemaining);
    }

    public void StopTimer()
    {
        if (!isRunning && !isPaused) return;
        isRunning = false;
        isPaused = false;
        ResetState();
        OnTimerStopped?.Invoke();
    }

    public void Complete()
    {
        if (!isRunning)
        {
            return;
        }

        timeRemaining = direction == Direction.CountDown ? 0f : duration;
        isRunning = false;
        isPaused = false;
        OnTimerComplete?.Invoke();
    }

    public void SetTimeScale(float scale) => timeScale = Mathf.Max(0f, scale);

    public void SetDuration(float newDuration)
    {
        duration = Mathf.Max(0f, newDuration);
        ResetState();
    }

    public void AddTime(float seconds)
    {
        timeRemaining = Mathf.Clamp(timeRemaining + seconds, 0f, duration);
        OnTimeUpdated?.Invoke(timeRemaining);
    }

    public void Pause()
    {
        if (!isRunning || isPaused) return;
        isPaused = true;
        OnTimerPaused?.Invoke();
    }

    public void Resume()
    {
        if (!isRunning || !isPaused) return;
        isPaused = false;
        OnTimerResumed?.Invoke();
    }

    public void TogglePause()
    {
        if (isPaused) Resume();
        else Pause();
    }

    private void ResetState()
    {
        timeRemaining = direction == Direction.CountDown ? duration : 0f;
        nextReportTime = direction == Direction.CountDown
            ? duration - reportInterval
            : reportInterval;
    }
}