using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class LaunchController : MonoBehaviour {
    private Coroutine launchRoutine;

    public float fuelBurnRatio = 1.5f;

    public IEnumerator Launch(RocketAssembly rocket, float baseBurnDuration, float settleTime) {
        if (launchRoutine != null)
            StopCoroutine(launchRoutine);

        HashSet<EngineThrustEffect> bracedEngines = rocket.GetBracedEngines();
        var fuel = CalculateTotalFuel(rocket.GetFuel());
        launchRoutine = StartCoroutine(BurnEngines(rocket, baseBurnDuration, settleTime, bracedEngines, fuel));
        yield return launchRoutine;
        launchRoutine = null;
    }

    private IEnumerator BurnEngines(
        RocketAssembly rocket,
        float baseBurnDuration,
        float settleTime,
        HashSet<EngineThrustEffect> bracedEngines,
        float fuel
    ) {
        var priorityGroups = bracedEngines
            .GroupBy(x => x.Group)
            .Select(g => g
                .GroupBy(x => x.PhasePriority)
                .OrderBy(pg => pg.Key)
                .Select(pg => pg.ToList())
                .ToList())
            .ToList();

        int numPhases = priorityGroups.Max(g => g.Count);
        var enginesByPhase = Enumerable.Range(0, numPhases)
            .Select(depth => priorityGroups
                .Where(g => depth < g.Count)
                .SelectMany(g => g[depth])
                .ToList())
            .ToList();

        float burnDuration = baseBurnDuration + (fuel * fuelBurnRatio / (1 + bracedEngines.Count()));

        Debug.Log($"[LaunchController] Calculated burn duration: base={baseBurnDuration}, fuel={fuel}, burnRatio={fuelBurnRatio}, engineCount={bracedEngines.Count()}, burnDuration={burnDuration}");

        ScreenShake.Instance?.Shake(2f);
        for (var i = 0; i < enginesByPhase.Count(); i++)
        {
            var activeEngines = enginesByPhase[i];
            LogThrustVsWeight(rocket, i, activeEngines);
            yield return Burn(i, activeEngines, burnDuration, settleTime);
        }
    }

    private float CalculateTotalFuel(List<List<Fuel>> fuelClusters)
    {
        float total = 0f;
        foreach (var cluster in fuelClusters)
        {
            float clusterSum = cluster.Sum(fp => fp.Value);
            float clusterSize = cluster.Count;
            float clusterTotal = clusterSum * clusterSize;
            Debug.Log($"[LaunchController] Calculated cluster fuel for cluster: group={cluster[0].Group}, sum={clusterSum}, size={clusterSize}, total={clusterTotal}");
            total += clusterTotal;
        }
        Debug.Log($"[LaunchController] Calculated total fuel for {fuelClusters.Count()} clusters: {total}");
        return total;
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
        Debug.Log($"[LaunchController] Phase {phase}: totalThrust={totalThrust:0.0} totalWeight={totalWeight:0.0}{message}");
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