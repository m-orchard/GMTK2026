using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : Singleton<GameManager>
{
    private enum State { Building, Launching, Result }

    [SerializeField] private Timer buildTimer;
    [SerializeField] private PieceSpawner spawner;
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private LaunchController launchController;
    [SerializeField] private HeightTracker heightTracker;

    [SerializeField] private float baseBurnDuration = 2.5f;
    [SerializeField] private float settleTime = 2f;
    [SerializeField] private float conveyorExitAtSecondsRemaining = 1f;
    [SerializeField] private float conveyorOffScreenAtSecondsRemaining = 0f;

    private State state;
    private bool conveyorExitTriggered;
    private bool lastRoundSucceeded;

    public event System.Action<float, float, bool> OnRoundResult;
    public event System.Action OnBuildingStarted;
    public event System.Action OnLaunchStarted;
    public event System.Action<float> OnTargetHeightChanged;

    private void Awake()
    {
        buildTimer.OnTimerComplete += HandleBuildTimerComplete;
    }

    private void OnDestroy()
    {
        buildTimer.OnTimerComplete -= HandleBuildTimerComplete;
    }

    private void Start()
    {
        LevelManager.Instance.ResetToFirstLevel();
        EnterBuilding();
    }

    private void Update()
    {
        if (state == State.Building && !conveyorExitTriggered &&
            buildTimer.TimeRemaining <= conveyorExitAtSecondsRemaining)
        {
            conveyorExitTriggered = true;
            spawner.BeginBuildEndExit(conveyorExitAtSecondsRemaining - conveyorOffScreenAtSecondsRemaining);
        }

        if (Keyboard.current == null) return;

        if (state == State.Result && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            Continue();
        }

        if (state == State.Building && Keyboard.current.cKey.wasPressedThisFrame)
        {
            spawner.SpawnCargo();
        }

        if (Keyboard.current.kKey.wasPressedThisFrame)
        {
            ScreenShake.Instance?.Shake();
        }
    }

    private void EnterBuilding()
    {
        state = State.Building;
        conveyorExitTriggered = false;
        rocket.ClearAll();
        spawner.ResetCargo();
        CameraManager.Instance.ResetToBuildFraming();
        spawner.SetPool(LevelManager.Instance.CurrentPool);
        spawner.StartBelt();
        buildTimer.SetDuration(LevelManager.Instance.BuildDuration);
        buildTimer.StartTimer();
        OnBuildingStarted?.Invoke();
        OnTargetHeightChanged?.Invoke(LevelManager.Instance.TargetHeight);
    }

    private void HandleBuildTimerComplete()
    {
        if (state != State.Building) return;
        EnterLaunching();
    }

    private void EnterLaunching()
    {
        state = State.Launching;
        spawner.ForceLockActive();
        rocket.LockSettledPieces();
        rocket.ReleaseFoundation();
        heightTracker.BeginTracking();
        CameraManager.Instance.StartFollowing();
        OnLaunchStarted?.Invoke();
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        yield return launchController.Launch(rocket.Rocket, baseBurnDuration, settleTime);
        yield return new WaitUntil(() => !heightTracker.IsTracking);
        EnterResult();
    }

    private void EnterResult()
    {
        state = State.Result;
        heightTracker.StopTracking();

        float apex = heightTracker.ApexHeight;
        float targetHeight = LevelManager.Instance.TargetHeight;
        lastRoundSucceeded = apex >= targetHeight;
        OnRoundResult?.Invoke(apex, targetHeight, lastRoundSucceeded);
    }

    public void Continue()
    {
        if (state != State.Result) return;

        if (lastRoundSucceeded)
            GoToNextLevel();
        else
            EnterBuilding();
    }

    public void GoToNextLevel()
    {
        LevelManager.Instance.AdvanceLevel();
        EnterBuilding();
    }
}
