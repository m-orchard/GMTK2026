using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

public class DebugMenu : Singleton<DebugMenu> {
    [SerializeField]
    private float menuWidth = 220f;

    [SerializeField]
    private float menuMargin = 10f;

    [SerializeField]
    private float menuTopOffset = 200f;

    private List<GameObject> piecePrefabs;

    private void OnGUI() {
        if (!IsAvailable()) {
            return;
        }

        DrawMenu();
    }

    private bool IsAvailable() {
        return Application.isEditor;
    }

    private void DrawMenu() {
        Rect menuArea = new Rect(menuMargin, menuTopOffset, menuWidth, Screen.height - menuTopOffset - menuMargin);
        GUILayout.BeginArea(menuArea, GUI.skin.box);

        GUILayout.Label("Debug Menu");

        DrawPieceTypeSection();

        GUILayout.EndArea();
    }

    private void DrawPieceTypeSection() {
        PieceSpawner spawner = PieceSpawner.Instance;
        if (spawner == null) {
            return;
        }

        GUILayout.Label("Pieces");

        foreach (GameObject piecePrefab in GetAllPiecePrefabs()) {
            if (piecePrefab == null) {
                continue;
            }

            if (GUILayout.Button(piecePrefab.name)) {
                spawner.ReplaceFrontConveyorPiece(piecePrefab);
            }
        }
    }

    private List<GameObject> GetAllPiecePrefabs() {
        if (piecePrefabs == null) {
            piecePrefabs = LoadAllPiecePrefabs();
        }

        return piecePrefabs;
    }

    private List<GameObject> LoadAllPiecePrefabs() {
        List<GameObject> loadedPrefabs = new List<GameObject>();

#if UNITY_EDITOR
        string[] prefabGuids = AssetDatabase.FindAssets("t:Prefab", new[] { "Assets/Prefabs/Pieces" });
        foreach (string prefabGuid in prefabGuids) {
            string prefabPath = AssetDatabase.GUIDToAssetPath(prefabGuid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab != null && prefab.GetComponent<Piece>() != null) {
                loadedPrefabs.Add(prefab);
            }
        }
#endif

        return loadedPrefabs;
    }
}
