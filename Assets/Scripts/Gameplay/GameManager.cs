using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

    [Header("Build Start")]
    [Tooltip("Seconds after the conveyor is fed before the build timer starts and the first piece is released. Gives the conveyor time to slide in and the scene to fade in.")]
    [SerializeField, Min(0f)] private float buildStartDelay = 1f;
    [Tooltip("Seconds to wait for the camera to finish blending back to build framing before showing that level's tutorial (if any). Should roughly match the Cinemachine blend duration.")]
    [SerializeField, Min(0f)] private float cameraResetDelay = 1f;

    [Header("Crane Auto Drop")]
    [SerializeField] private bool autoDropCraneCargoEnabled;
    [SerializeField, Range(1, 5)] private int autoDropCraneCargoAtSecondsRemaining = 3;
    [SerializeField] private bool autoDroppedCargoIsControllable = false;

    private State state;
    private bool conveyorExitTriggered;
    private bool craneCargoAutoDropTriggered;
    private bool lastRoundSucceeded;

    private readonly List<IPreBuildGate> preBuildGates = new();

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

    public void RegisterPreBuildGate(IPreBuildGate gate)
    {
        if (!preBuildGates.Contains(gate))
            preBuildGates.Add(gate);
    }

    public void UnregisterPreBuildGate(IPreBuildGate gate)
    {
        preBuildGates.Remove(gate);
    }

    private void Update()
    {
        if (state == State.Building && buildTimer.IsRunning && !conveyorExitTriggered &&
            buildTimer.TimeRemaining <= conveyorExitAtSecondsRemaining)
        {
            conveyorExitTriggered = true;
            spawner.BeginBuildEndExit(conveyorExitAtSecondsRemaining - conveyorOffScreenAtSecondsRemaining);
        }

        if (state == State.Building && buildTimer.IsRunning && autoDropCraneCargoEnabled && !craneCargoAutoDropTriggered &&
            buildTimer.TimeRemaining <= autoDropCraneCargoAtSecondsRemaining)
        {
            craneCargoAutoDropTriggered = true;
            spawner.SpawnCargo(autoDroppedCargoIsControllable);
        }

        if (Keyboard.current == null) return;

        if (state == State.Result && !lastRoundSucceeded && Keyboard.current.spaceKey.wasPressedThisFrame)
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
        craneCargoAutoDropTriggered = false;
        rocket.ClearAll();
        spawner.ResetCargo();
        CameraManager.Instance.ResetToBuildFraming();
        spawner.SetPool(LevelManager.Instance.CurrentPool);
        spawner.StartBelt();
        OnBuildingStarted?.Invoke();
        OnTargetHeightChanged?.Invoke(LevelManager.Instance.TargetHeight);
        StartCoroutine(StartBuildSequence());
    }

    private IEnumerator StartBuildSequence()
    {
        if (cameraResetDelay > 0f)
            yield return new WaitForSeconds(cameraResetDelay);

        if (state != State.Building)
            yield break;

        yield return RunPreBuildGates();

        if (state != State.Building)
            yield break;

        if (buildStartDelay > 0f)
            yield return new WaitForSeconds(buildStartDelay);

        if (state != State.Building)
            yield break;

        spawner.ReleaseFirstPiece();
        buildTimer.SetDuration(LevelManager.Instance.BuildDuration);
        buildTimer.StartTimer();
    }

    private IEnumerator RunPreBuildGates()
    {
        int level = LevelManager.Instance.CurrentLevel;

        foreach (var gate in preBuildGates.OrderBy(g => g.Order).ToList())
        {
            yield return gate.WaitUntilReady(level);

            if (state != State.Building)
                yield break;
        }
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

    public void DebugCompleteLevel()
    {
        if (state == State.Result) return;

        buildTimer.StopTimer();
        heightTracker.StopTracking();

        state = State.Result;
        lastRoundSucceeded = true;

        float targetHeight = LevelManager.Instance.TargetHeight;
        OnRoundResult?.Invoke(targetHeight, targetHeight, true);
    }
}
