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
    [SerializeField, Min(0f)] private float firstWaveDelay = 3f;
    [SerializeField, Min(0f)] private float spawnScatterRadius = 1f;

    [Tooltip("Shared heartbeat all waves are aligned to. Wave starts and countdown durations snap to this tick so every timer counts down in sync.")]
    [SerializeField, Min(0.01f)] private float spawnTickInterval = 1f;

    [Header("Escalation (X = seconds since waves began)")]
    [Tooltip("Number of enemies spawned by a wave.")]
    [SerializeField] private AnimationCurve enemiesPerWaveCurve = AnimationCurve.Linear(0f, 1f, 120f, 6f);

    [Tooltip("Seconds between the start of one wave and the next.")]
    [SerializeField] private AnimationCurve waveIntervalCurve = AnimationCurve.Linear(0f, 10f, 120f, 6f);

    [Tooltip("Countdown length shown on a wave's timer before it breaches. Much longer than the interval so many timers stack up at once.")]
    [SerializeField] private AnimationCurve warningDurationCurve = AnimationCurve.Linear(0f, 90f, 120f, 75f);

    private float wavesStartTime;

    public event System.Action<ShipLocation> OnWaveBreached;

    private void Start()
    {
        if (!HasRequiredReferences())
        {
            Debug.LogWarning("WaveManager is missing references or breach points; not starting waves.", this);
            return;
        }

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

        float nextWaveElapsed = 0f;
        for (int tick = 0; ; tick++)
        {
            float elapsed = tick * spawnTickInterval;

            if (elapsed >= nextWaveElapsed)
            {
                BeginWave(elapsed);
                nextWaveElapsed = elapsed + QuantizeToTick(waveIntervalCurve.Evaluate(elapsed));
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

    private void BeginWave(float elapsed)
    {
        BreachPoint breachPoint = PickRandomBreachPoint();
        int enemyCount = Mathf.Max(1, Mathf.RoundToInt(enemiesPerWaveCurve.Evaluate(elapsed)));

        InvasionTimerView timerView = Instantiate(invasionTimerPrefab, invasionTimersParent);
        timerView.SetLabel($"Breach at {breachPoint.location}:");

        Timer timer = timerView.Timer;
        timer.SetDuration(QuantizeToTick(warningDurationCurve.Evaluate(elapsed)));
        timer.OnTimerComplete += () => HandleWaveBreach(breachPoint, enemyCount, timerView);
        timer.StartTimer();
    }

    private void HandleWaveBreach(BreachPoint breachPoint, int enemyCount, InvasionTimerView timerView)
    {
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
