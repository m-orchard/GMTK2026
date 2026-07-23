using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LaunchController : MonoBehaviour
{
    private Coroutine burnRoutine;

    public void Launch(RocketAssembly rocket, float burnDuration)
    {
        if (burnRoutine != null) StopCoroutine(burnRoutine);
        HashSet<Piece> connected = rocket.GetConnectedPieces();
        LogThrustVsWeight(rocket, connected);
        burnRoutine = StartCoroutine(Burn(rocket, burnDuration, connected));
    }

    private void LogThrustVsWeight(RocketAssembly rocket, HashSet<Piece> connected)
    {
        float totalThrust = 0f;
        float totalWeight = 0f;
        float gravity = Mathf.Abs(Physics2D.gravity.y);

        foreach (var p in rocket.Pieces)
        {
            if (!connected.Contains(p)) continue;
            totalWeight += p.Body2D.mass * gravity * p.Body2D.gravityScale;
            if (p.Type == Piece.PieceType.Engine) totalThrust += p.Thrust;
        }

        Debug.Log($"[Launch] totalThrust={totalThrust:0.0} totalWeight={totalWeight:0.0} (need thrust > weight to lift off)");
    }

    private IEnumerator Burn(RocketAssembly rocket, float burnDuration, HashSet<Piece> connected)
    {
        var engines = new List<Piece>(rocket.GetPiecesOfType(Piece.PieceType.Engine));
        foreach (var engine in engines)
        {
            engine.ThrustEffect?.SetFiring(connected.Contains(engine));
        }

        float elapsed = 0f;
        while (elapsed < burnDuration)
        {
            foreach (var engine in engines)
            {
                if (!connected.Contains(engine)) continue;
                engine.Body2D.AddForce((Vector2)engine.transform.up * engine.Thrust, ForceMode2D.Force);
            }
            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        foreach (var engine in engines)
        {
            engine.ThrustEffect?.SetFiring(false);
        }
        burnRoutine = null;
    }
}
