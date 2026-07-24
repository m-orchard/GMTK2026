using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaunchController : MonoBehaviour {
    private Coroutine launchRoutine;

    public IEnumerator Launch(RocketAssembly rocket, float burnDuration, float settleTime) {
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        HashSet<EngineThrustEffect> bracedEngines = rocket.GetBracedEngines();
        launchRoutine = StartCoroutine(BurnEngines(rocket, burnDuration, settleTime, bracedEngines));
        yield return launchRoutine;
        launchRoutine = null;
    }

    private IEnumerator BurnEngines(
        RocketAssembly rocket,
        float burnDuration,
        float settleTime,
        HashSet<EngineThrustEffect> bracedEngines
    ) {
        var engineGroups = bracedEngines
            .GroupBy(x => x.Group)
            .Select(g => g.OrderBy(x => x.PhasePriority).ToList())
            .ToList();

        int numPhases = engineGroups.Max(g => g.Count);
        var enginesByPhase = Enumerable.Range(0, numPhases)
            .Select(i => engineGroups
                .Where(g => i < g.Count)
                .Select(g => g[i])
                .ToList())
            .ToList();

        ScreenShake.Instance?.Shake(2f);
        for (var i = 0; i < enginesByPhase.Count(); i++)
        {
            var activeEngines = enginesByPhase[i];
            LogThrustVsWeight(rocket, i, activeEngines);
            yield return Burn(i, activeEngines, burnDuration, settleTime);
        }
    }

    private void LogThrustVsWeight(RocketAssembly rocket, int phase, IEnumerable<EngineThrustEffect> activeEngines) {
        float totalThrust = 0f;
        float totalWeight = 0f;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        foreach (var p in rocket.Pieces) {
            if (!p.IsLocked)
                continue;
            totalWeight += p.Body2D.mass * gravity * p.Body2D.gravityScale;
            if (p.TryGetComponent<EngineThrustEffect>(out var engine))
            {
                if (activeEngines.Contains(engine))
                {
                    totalThrust += engine.Thrust;
                }
            }
        }

        var message = phase == 1 ? " (need thrust > weight to lift off)" : "";
        Debug.Log($"[Launch Controller] Phase {phase}: totalThrust={totalThrust:0.0} totalWeight={totalWeight:0.0}{message}");
    }

    private IEnumerator Burn(int phase, IEnumerable<EngineThrustEffect> activeEngines, float burnDuration, float settleDuration) {
        var pieces = new Dictionary<EngineThrustEffect, Piece>();
        foreach (var engine in activeEngines) {
            var piece = engine.GetComponent<Piece>();
            pieces.Add(engine, piece);
            engine.SetFiring(piece.IsLocked);
        }

        Debug.Log($"[Launch Controller] Phase {phase}: Burning {activeEngines.Count()} engines");
        float elapsedBurn = 0f;
        while (elapsedBurn < burnDuration) {
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

        Debug.Log($"[Launch Controller] Phase {phase}: Settling");
        float elapsedSettle = 0f;
        while (elapsedSettle < settleDuration) {
            elapsedSettle += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        Debug.Log($"[Launch Controller] Phase {phase}: Complete");
    }
}