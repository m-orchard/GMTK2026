using System.Collections;
using UnityEngine;

public interface IInteractable {
    public string InteractionPrompt { get; }

    public Sprite InteractionIcon { get; }

    public bool Interact(Interactor interactor);
}