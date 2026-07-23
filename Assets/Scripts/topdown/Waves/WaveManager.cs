using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaveManager : Singleton<WaveManager>
{
    [System.Serializable]
    private class BreachPoint
    {
        public ShipLocation location;
        public Transform spawnPoint;
    }

    [Header("Prefabs")]
    [SerializeField] private EnemyAI enemyPrefab;
    [SerializeField] private InvasionTimerView invasionTimerPrefab;

    [Header("UI")]
    [Tooltip("The right-side Invasion Timers container (has the VerticalLayoutGroup).")]
    [SerializeField] private Transform invasionTimersParent;

    [Header("Ship Locations")]
    [SerializeField] private List<BreachPoint> breachPoints = new List<BreachPoint>();

    [Header("Spawning")]
    [SerializeField, Min(0f)] private float firstWaveDelay = 1f;

    [Tooltip("Fixed countdown for the opening wave so the first breach lands at a predictable time instead of a random roll.")]
    [SerializeField, Min(0f)] private float firstWaveCountdown = 30f;

    [Tooltip("How many upcoming waves to keep queued on screen. A new wave is added whenever one breaches.")]
    [SerializeField, Min(1)] private int queuedWaveCount = 5;

    [SerializeField, Min(0f)] private float spawnScatterRadius = 1f;

    [Tooltip("Shared heartbeat all waves are aligned to. Wave starts and countdown durations snap to this tick so every timer counts down in sync.")]
    [SerializeField, Min(0.01f)] private float spawnTickInterval = 1f;

    [Header("Escalation (X = seconds since waves began)")]
    [Tooltip("Number of enemies spawned by a wave.")]
    [SerializeField] private AnimationCurve enemiesPerWaveCurve = AnimationCurve.Linear(0f, 1f, 300f, 8f);

    [Tooltip("Shortest gap added after the previous wave's breach when scheduling a new one. Each new wave always breaches later than every queued wave.")]
    [SerializeField] private AnimationCurve breachGapMinCurve = AnimationCurve.Linear(0f, 40f, 600f, 60f);

    [Tooltip("Longest gap added after the previous wave's breach. A random gap between min and max gives the queue a spread from seconds to minutes.")]
    [SerializeField] private AnimationCurve breachGapMaxCurve = AnimationCurve.Linear(0f, 95f, 600f, 150f);

    private float wavesStartTime;
    private float lastBreachElapsed;
    private int spawnedWaveCount;
    private int queuedWaves;
    private bool hasBegun;

    public event System.Action<ShipLocation> OnWaveBreached;

    private void Start()
    {
        if (GameSession.Instance == null)
        {
            BeginWaves();
            return;
        }

        GameSession.Instance.OnGameStarted += BeginWaves;

        if (GameSession.Instance.HasStarted)
        {
            BeginWaves();
        }
    }

    private void OnDestroy()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.OnGameStarted -= BeginWaves;
        }
    }

    public void BeginWaves()
    {
        if (hasBegun)
        {
            return;
        }

        if (!HasRequiredReferences())
        {
            Debug.LogWarning("WaveManager is missing references or breach points; not starting waves.", this);
            return;
        }

        hasBegun = true;
        StartCoroutine(RunWaveLoop());
    }

    private bool HasRequiredReferences()
    {
        return enemyPrefab != null
            && invasionTimerPrefab != null
            && invasionTimersParent != null
            && breachPoints.Count > 0;
    }

    private IEnumerator RunWaveLoop()
    {
        yield return new WaitForSecondsRealtime(firstWaveDelay);
        wavesStartTime = Time.unscaledTime;

        for (int tick = 0; ; tick++)
        {
            float elapsed = tick * spawnTickInterval;

            while (queuedWaves < queuedWaveCount)
            {
                QueueWave(elapsed);
            }

            yield return WaitForTick(tick + 1);
        }
    }

    private WaitForSecondsRealtime WaitForTick(int tick)
    {
        float remaining = wavesStartTime + tick * spawnTickInterval - Time.unscaledTime;
        return new WaitForSecondsRealtime(Mathf.Max(0f, remaining));
    }

    private float QuantizeToTick(float seconds)
    {
        return Mathf.Max(spawnTickInterval, Mathf.Round(seconds / spawnTickInterval) * spawnTickInterval);
    }

    public void ForceNextBreach()
    {
        Timer soonest = FindSoonestRunningTimer();
        if (soonest != null)
        {
            soonest.Complete();
        }
    }

    public bool TryGetTimeUntilNextBreach(out float timeRemaining)
    {
        Timer soonest = FindSoonestRunningTimer();
        if (soonest == null)
        {
            timeRemaining = 0f;
            return false;
        }

        timeRemaining = soonest.TimeRemaining;
        return true;
    }

    private Timer FindSoonestRunningTimer()
    {
        if (invasionTimersParent == null)
        {
            return null;
        }

        Timer soonest = null;
        foreach (Timer timer in invasionTimersParent.GetComponentsInChildren<Timer>())
        {
            if (!timer.IsRunning)
            {
                continue;
            }

            if (soonest == null || timer.TimeRemaining < soonest.TimeRemaining)
            {
                soonest = timer;
            }
        }

        return soonest;
    }

    private void QueueWave(float elapsed)
    {
        BreachPoint breachPoint = PickRandomBreachPoint();
        int enemyCount = Mathf.Max(1, Mathf.RoundToInt(enemiesPerWaveCurve.Evaluate(elapsed)));

        lastBreachElapsed = NextBreachElapsed(elapsed);
        float countdown = Mathf.Max(spawnTickInterval, lastBreachElapsed - elapsed);

        InvasionTimerView timerView = Instantiate(invasionTimerPrefab, invasionTimersParent);
        timerView.SetLabel($"Breach at {breachPoint.location}:");

        Timer timer = timerView.Timer;
        timer.SetDuration(countdown);
        timer.OnTimerComplete += () => HandleWaveBreach(breachPoint, enemyCount, timerView);
        timer.StartTimer();

        spawnedWaveCount++;
        queuedWaves++;
    }

    private float NextBreachElapsed(float elapsed)
    {
        if (spawnedWaveCount == 0)
        {
            return QuantizeToTick(elapsed + firstWaveCountdown);
        }

        return lastBreachElapsed + RollBreachGap(elapsed);
    }

    private float RollBreachGap(float elapsed)
    {
        float minGap = breachGapMinCurve.Evaluate(elapsed);
        float maxGap = breachGapMaxCurve.Evaluate(elapsed);
        float rolled = Random.Range(Mathf.Min(minGap, maxGap), Mathf.Max(minGap, maxGap));
        return QuantizeToTick(rolled);
    }

    private void HandleWaveBreach(BreachPoint breachPoint, int enemyCount, InvasionTimerView timerView)
    {
        queuedWaves--;
        SpawnWaveEnemies(breachPoint, enemyCount);
        OnWaveBreached?.Invoke(breachPoint.location);
        Destroy(timerView.gameObject);
    }

    private void SpawnWaveEnemies(BreachPoint breachPoint, int enemyCount)
    {
        for (int i = 0; i < enemyCount; i++)
        {
            Vector3 spawnPosition = breachPoint.spawnPoint.position + ScatterOffset();
            EnemyAI enemy = Instantiate(enemyPrefab, spawnPosition, Quaternion.identity);
            enemy.SetReturnPoint(breachPoint.spawnPoint);
        }
    }

    private Vector3 ScatterOffset()
    {
        Vector2 offset = Random.insideUnitCircle * spawnScatterRadius;
        return new Vector3(offset.x, offset.y, 0f);
    }

    private BreachPoint PickRandomBreachPoint()
    {
        return breachPoints[Random.Range(0, breachPoints.Count)];
    }
}
