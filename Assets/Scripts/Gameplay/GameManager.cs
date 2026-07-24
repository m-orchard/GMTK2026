using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class GameManager : MonoBehaviour
{
    private enum State { Building, Launching, Result }

    [SerializeField] private Timer buildTimer;
    [SerializeField] private PieceSpawner spawner;
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private LaunchController launchController;
    [SerializeField] private HeightTracker heightTracker;

    [SerializeField] private float startingTargetHeight = 8f;
    [SerializeField] private float targetHeightIncrement = 4f;
    [SerializeField] private float burnDuration = 2.5f;
    [SerializeField] private float settleTime = 2f;

    private State state;
    private float targetHeight;

    public event System.Action<float, float, bool> OnRoundResult;
    public event System.Action OnBuildingStarted;
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
        targetHeight = startingTargetHeight;
        EnterBuilding();
    }

    private void Update()
    {
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
        rocket.ClearAll();
        spawner.ResetCargo();
        CameraManager.Instance.ResetToBuildFraming();
        spawner.StartBelt();
        buildTimer.StartTimer();
        OnBuildingStarted?.Invoke();
        OnTargetHeightChanged?.Invoke(targetHeight);
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
        StartCoroutine(Launch());
    }

    private IEnumerator Launch()
    {
        yield return launchController.Launch(rocket, burnDuration, settleTime);
        EnterResult();
    }

    private void EnterResult()
    {
        state = State.Result;
        heightTracker.StopTracking();

        float apex = heightTracker.ApexHeight;
        bool success = apex >= targetHeight;
        OnRoundResult?.Invoke(apex, targetHeight, success);

        if (success) targetHeight += targetHeightIncrement;
    }

    public void Continue()
    {
        if (state != State.Result) return;
        EnterBuilding();
    }
}
