using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchController : MonoBehaviour {
    private Coroutine burnRoutine;

    public void Launch(RocketAssembly rocket, float burnDuration) {
        if (burnRoutine != null)
            StopCoroutine(burnRoutine);
        LogThrustVsWeight(rocket);
        ScreenShake.Instance?.Shake(2f);
        burnRoutine = StartCoroutine(Burn(rocket, burnDuration));
    }

    private void LogThrustVsWeight(RocketAssembly rocket) {
        float totalThrust = 0f;
        float totalWeight = 0f;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        foreach (var p in rocket.Pieces) {
            if (!p.IsLocked)
                continue;
            totalWeight += p.Body2D.mass * gravity * p.Body2D.gravityScale;
            if (p.TryGetComponent<EngineThrustEffect>(out var engine))
                totalThrust += engine.Thrust;
        }

        Debug.Log($"[Launch] totalThrust={totalThrust:0.0} totalWeight={totalWeight:0.0} (need thrust > weight to lift off)");
    }

    private IEnumerator Burn(RocketAssembly rocket, float burnDuration) {
        var engines = rocket.GetComponentsInChildren<EngineThrustEffect>();
        var pieces = new Dictionary<EngineThrustEffect, Piece>();
        foreach (var engine in engines) {
            var piece = engine.GetComponent<Piece>();
            pieces.Add(engine, piece);
            engine.SetFiring(piece.IsLocked);
        }

        float elapsed = 0f;
        while (elapsed < burnDuration) {
            foreach (var engine in engines) {
                var piece = pieces[engine];
                if (!piece.IsLocked)
                    continue;
                piece.Body2D.AddForce((Vector2)engine.transform.up * engine.Thrust, ForceMode2D.Force);
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        foreach (var engine in engines) {
            engine.SetFiring(false);
        }
        burnRoutine = null;
    }
}