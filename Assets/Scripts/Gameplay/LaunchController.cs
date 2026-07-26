using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaunchController : Singleton<LaunchController> {
    private Coroutine launchRoutine;

    public System.Action<int> OnBurnStart;

    public System.Action<int> OnBurnEnd;

    public IEnumerator Launch(Rocket rocket, float settleTime) {
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        launchRoutine = StartCoroutine(BurnEngines(rocket, settleTime));
        yield return launchRoutine;
        launchRoutine = null;
    }

    private IEnumerator BurnEngines(
        Rocket rocket,
        float settleTime
    ) {
        var engineGroups = rocket.EngineGroups;

        Debug.Log($"[LaunchController] Burning {engineGroups.Count()} engine groups for {rocket.BurnDuration}s");

        ScreenShake.Instance?.Shake(2f);
        for (var i = 0; i < engineGroups.Count(); i++)
        {
            var activeEngines = engineGroups[i];
            LogThrustVsWeight(rocket, i, activeEngines);
            yield return Burn(i, activeEngines, rocket.BurnDuration, settleTime);
        }
    }

    private void LogThrustVsWeight(Rocket rocket, int phase, IEnumerable<EngineThrustEffect> activeEngines) {
        float groupThrust = 0f;

        foreach (var engine in activeEngines)
        {
            groupThrust += engine.Thrust;
        }

        var message = phase == 1 ? " (need thrust > weight to lift off)" : "";
        Debug.Log($"[LaunchController] Phase {phase}: groupThrust={groupThrust:0.0} totalWeight={rocket.TotalWeight:0.0}{message}");
    }

    private IEnumerator Burn(int phase, IEnumerable<EngineThrustEffect> activeEngines, float baseBurnDuration, float settleDuration) {
        OnBurnStart?.Invoke(phase);

        var pieces = new Dictionary<EngineThrustEffect, Piece>();
        foreach (var engine in activeEngines) {
            var piece = engine.GetComponent<Piece>();
            pieces.Add(engine, piece);
            engine.SetFiring(piece.IsLocked);
        }

        Debug.Log($"[LaunchController] Phase {phase}: Burning {activeEngines.Count()} engines");
        float elapsedBurn = 0f;
        while (elapsedBurn < baseBurnDuration) {
            foreach (var engine in activeEngines) {
                var piece = pieces[engine];
                if (!piece.IsLocked)
                    continue;
                piece.Body2D.AddForce((Vector2)engine.transform.up * engine.Thrust, ForceMode2D.Force);
            }
            elapsedBurn += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        foreach (var engine in activeEngines) {
            engine.SetFiring(false);
        }

        OnBurnEnd?.Invoke(phase);

        Debug.Log($"[LaunchController] Phase {phase}: Settling");
        float elapsedSettle = 0f;
        while (elapsedSettle < settleDuration) {
            elapsedSettle += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[LaunchController] Phase {phase}: Complete");
    }
}