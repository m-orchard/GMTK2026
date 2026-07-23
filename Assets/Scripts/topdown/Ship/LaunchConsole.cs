using UnityEngine;

public class LaunchConsole : MonoBehaviour, IInteractable {
    [SerializeField] private string interactionPrompt = "Press F to launch";
    [SerializeField] private Sprite interactionIcon;
    [SerializeField] private AudioClip launchSfx;

    public string InteractionPrompt => interactionPrompt;
    public Sprite InteractionIcon => interactionIcon;

    public AudioClip LaunchSfx => launchSfx;

    public bool Interact(Interactor interactor) {
        if (GameSession.Instance == null || GameSession.Instance.HasStarted) {
            return false;
        }

        GameSession.Instance.StartGame();
        StopBeingInteractable();
        AudioManager.Instance.PlaySound(LaunchSfx);
        return true;
    }

    private void StopBeingInteractable() {
        foreach (Collider2D interactionCollider in GetComponents<Collider2D>()) {
            interactionCollider.enabled = false;
        }
    }
}