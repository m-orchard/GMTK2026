using UnityEngine;

public class ModuleRepairInteractable : MonoBehaviour, IInteractable, IHoldInteractable {
    [SerializeField] private string interactionPrompt = "Hold F to repair";
    [SerializeField] private Sprite interactionIcon;

    [Tooltip("The module this repairs. Falls back to the nearest ShipModule up the hierarchy.")]
    [SerializeField] private ShipModule module;

    [Header("Repair")]
    [SerializeField, Min(0.01f)] private float secondsPerRepairTick = 1f;

    [SerializeField, Min(0f)] private float healPerTick = 10f;
    [SerializeField, Min(0)] private int resourceCostPerTick = 5;

    [Header("Audio")]
    [SerializeField] private AudioClip repairSound;

    [Tooltip("Volume for the repair sound. Above 1 amplifies past the clip's recorded level.")]
    [SerializeField, Min(0f)] private float repairSoundVolume = 2f;

    private float tickTimer;

    public string InteractionPrompt => interactionPrompt;
    public Sprite InteractionIcon => interactionIcon;

    private void Awake() {
        if (module == null) {
            module = GetComponentInParent<ShipModule>();
        }
    }

    public bool Interact(Interactor interactor) {
        return false;
    }

    public void HoldTick(float deltaTime) {
        if (module == null || module.IsFullyRepaired) {
            tickTimer = 0f;
            return;
        }

        tickTimer += deltaTime;

        while (tickTimer >= secondsPerRepairTick) {
            tickTimer -= secondsPerRepairTick;

            if (!TryRepairTick()) {
                tickTimer = 0f;
                break;
            }
        }
    }

    public void HoldReleased() {
        tickTimer = 0f;
    }

    private bool TryRepairTick() {
        if (module.IsFullyRepaired) {
            return false;
        }

        if (ResourceBank.Instance == null || !ResourceBank.Instance.TrySpend(resourceCostPerTick)) {
            return false;
        }

        module.Repair(healPerTick);
        PlayRepairSound();
        return true;
    }

    private void PlayRepairSound() {
        AudioManager.Instance.PlaySound(repairSound, transform, new AudioClipOptions { Volume = repairSoundVolume });
    }
}