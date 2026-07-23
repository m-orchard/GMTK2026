using System;
using System.Collections;
using UnityEngine;

public class InteractableEvent : MonoBehaviour, IInteractable {

    [SerializeField]
    private string _prompt;

    [SerializeField]
    private Sprite _icon;

    public string InteractionPrompt => _prompt;

    public Sprite InteractionIcon => _icon;

    public event Action OnInteract;

    [SerializeField]
    private AudioClip _onInteractSFX;

    public bool Interact(Interactor interactor) {
        AudioClipOptions audioClipOptions = new AudioClipOptions();
        audioClipOptions.RandomPitch = true;
        audioClipOptions.Pitch = 1.0f;
        audioClipOptions.PitchRange = 0.1f;

        AudioManager.Instance.PlaySound(_onInteractSFX, transform, audioClipOptions);

        OnInteract?.Invoke();
        return true;
    }
}