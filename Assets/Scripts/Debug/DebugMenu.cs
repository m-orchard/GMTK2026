using UnityEngine;
#if UNITY_EDITOR
using System.Collections.Generic;
#endif

public class DebugMenu : Singleton<DebugMenu>
{
#if UNITY_EDITOR
    private struct DebugAction
    {
        public string label;
        public System.Action action;
    }

    private readonly List<DebugAction> actions = new List<DebugAction>();

    private void Start()
    {
        Register("Start Game", StartGame);
        Register("Force Next Spawn Timer", ForceNextSpawnTimer);
    }

    private void StartGame()
    {
        if (GameSession.Instance != null)
        {
            GameSession.Instance.StartGame();
        }
    }

    public void Register(string label, System.Action action)
    {
        actions.Add(new DebugAction { label = label, action = action });
    }

    private void ForceNextSpawnTimer()
    {
        if (WaveManager.Instance != null)
        {
            WaveManager.Instance.ForceNextBreach();
        }
    }

    private void OnGUI()
    {
        GUILayout.BeginArea(new Rect(10f, 10f, 220f, Screen.height - 20f), GUI.skin.box);
        GUILayout.Label("Debug Menu");

        foreach (DebugAction debugAction in actions)
        {
            if (GUILayout.Button(debugAction.label))
            {
                debugAction.action?.Invoke();
            }
        }

        GUILayout.EndArea();
    }
#endif
}
