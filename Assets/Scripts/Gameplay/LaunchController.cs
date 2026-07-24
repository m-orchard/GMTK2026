using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaunchController : MonoBehaviour {
    private Coroutine launchRoutine;

    public IEnumerator Launch(RocketAssembly rocket, float burnDuration, float settleTime) {
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        HashSet<Piece> braced = rocket.GetBracedEngines();
        launchRoutine = StartCoroutine(BurnEngines(rocket, burnDuration, settleTime, braced));
        yield return launchRoutine;
        launchRoutine = null;
    }

    private IEnumerator BurnEngines(RocketAssembly rocket, float burnDuration, float settleTime, HashSet<Piece> braced)
    {
        var phases = rocket.GetComponentsInChildren<EngineThrustEffect>().Select(engine => engine.Phase).Distinct().OrderBy(v => v);
        ScreenShake.Instance?.Shake(2f);
        foreach (var phase in phases)
        {
            LogThrustVsWeight(rocket, phase, braced);
            yield return Burn(rocket, burnDuration, settleTime, phase, braced);
        }
    }

    private void LogThrustVsWeight(RocketAssembly rocket, int phase, HashSet<Piece> braced) {
        float totalThrust = 0f;
        float totalWeight = 0f;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        foreach (var p in rocket.Pieces) {
            if (!p.IsLocked)
                continue;
            totalWeight += p.Body2D.mass * gravity * p.Body2D.gravityScale;
            if (braced.Contains(p) && p.TryGetComponent<EngineThrustEffect>(out var engine))
            {
                if (engine.Phase == phase)
                {
                    totalThrust += engine.Thrust;
                }
            }
        }

        var message = phase == 1 ? " (need thrust > weight to lift off)" : "";
        Debug.Log($"[Launch (burn phase {phase})] totalThrust={totalThrust:0.0} totalWeight={totalWeight:0.0}{message}");
    }

    private IEnumerator Burn(RocketAssembly rocket, float burnDuration, float settleDuration, int phase, HashSet<Piece> braced) {
        var engines = rocket.GetComponentsInChildren<EngineThrustEffect>().Where(engine => engine.Phase == phase);
        var pieces = new Dictionary<EngineThrustEffect, Piece>();
        foreach (var engine in engines) {
            var piece = engine.GetComponent<Piece>();
            pieces.Add(engine, piece);
            engine.SetFiring(piece.IsLocked && braced.Contains(piece));
        }

        Debug.Log($"[Launch Controller] Phase {phase}: Burning {engines.Count()} engines");
        float elapsedBurn = 0f;
        while (elapsedBurn < burnDuration) {
            foreach (var engine in engines) {
                var piece = pieces[engine];
                if (!piece.IsLocked || !braced.Contains(piece))
                    continue;
                piece.Body2D.AddForce((Vector2)engine.transform.up * engine.Thrust, ForceMode2D.Force);
            }
            elapsedBurn += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        foreach (var engine in engines) {
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