using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaunchController : MonoBehaviour {
    private Coroutine launchRoutine;

    public float fuelBurnRatio = 1.5f;

    public IEnumerator Launch(Rocket rocket, float baseBurnDuration, float settleTime) {
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        launchRoutine = StartCoroutine(BurnEngines(rocket, baseBurnDuration, settleTime));
        yield return launchRoutine;
        launchRoutine = null;
    }

    private IEnumerator BurnEngines(
        Rocket rocket,
        float baseBurnDuration,
        float settleTime
    ) {
        var engineGroups = rocket.EngineGroups;
        var totalEngines = engineGroups.Sum(group => group.Count());
        float burnDuration = baseBurnDuration + (rocket.AvailableFuel * fuelBurnRatio / (1 + totalEngines));

        Debug.Log($"[LaunchController] Calculated burn duration: base={baseBurnDuration}, fuel={rocket.AvailableFuel}, burnRatio={fuelBurnRatio}, totalEngines={totalEngines}, burnDuration={burnDuration}");
        Debug.Log($"[LaunchController] Burning {engineGroups.Count()} engine groups");

        ScreenShake.Instance?.Shake(2f);
        for (var i = 0; i < engineGroups.Count(); i++)
        {
            var activeEngines = engineGroups[i];
            LogThrustVsWeight(rocket, i, activeEngines);
            yield return Burn(i, activeEngines, burnDuration, settleTime);
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

        Debug.Log($"[LaunchController] Phase {phase}: Settling");
        float elapsedSettle = 0f;
        while (elapsedSettle < settleDuration) {
            elapsedSettle += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[LaunchController] Phase {phase}: Complete");
    }
}