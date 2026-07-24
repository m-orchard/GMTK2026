using UnityEngine;

public class DebugMenu : Singleton<DebugMenu> {
    [SerializeField]
    private float menuWidth = 220f;

    [SerializeField]
    private float menuMargin = 10f;

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
        Rect menuArea = new Rect(menuMargin, menuMargin, menuWidth, Screen.height - menuMargin * 2f);
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

        foreach (GameObject piecePrefab in spawner.PiecePrefabs) {
            if (piecePrefab == null) {
                continue;
            }

            if (GUILayout.Button(piecePrefab.name)) {
                spawner.ReplaceFrontConveyorPiece(piecePrefab);
            }
        }
    }
}
