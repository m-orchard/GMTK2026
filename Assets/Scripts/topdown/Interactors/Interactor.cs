using UnityEngine;
#if ENABLE_INPUT_SYSTEM
using UnityEngine.InputSystem;
#endif

public class Interactor : MonoBehaviour {

    [SerializeField]
    private Transform _interactionPoint;

    [SerializeField]
    private float _interactionPointRadius = 0.75f;

    [SerializeField]
    private LayerMask _interactableMask;

    [SerializeField]
    private InteractionPromptController _promptController;

    private readonly Collider2D[] _colliderResults = new Collider2D[3];

    private IInteractable _interactable;
    private IHoldInteractable _activeHold;

    private void Awake() {
        if (_interactionPoint == null) {
            _interactionPoint = transform;
        }
    }

    private void Update() {
        RefreshInteractable();
        UpdateInteraction();
    }

    private void UpdateInteraction() {
        IHoldInteractable holdTarget = _interactable as IHoldInteractable;

        if (holdTarget != null && IsInteractHeld()) {
            ContinueHold(holdTarget);
            return;
        }

        ReleaseActiveHold();

        if (holdTarget == null && IsInteractPressed()) {
            Interact();
        }
    }

    private void ContinueHold(IHoldInteractable holdTarget) {
        if (_activeHold != null && !ReferenceEquals(_activeHold, holdTarget)) {
            _activeHold.HoldReleased();
        }

        holdTarget.HoldTick(Time.deltaTime);
        _activeHold = holdTarget;
    }

    private void ReleaseActiveHold() {
        if (_activeHold != null) {
            _activeHold.HoldReleased();
            _activeHold = null;
        }
    }

    public bool HasInteractionTarget() {
        return _interactable != null;
    }

    public void Interact() {
        if (_interactable == null) {
            return;
        }

        _interactable.Interact(this);
    }

    private void RefreshInteractable() {
        IInteractable newInteractable = FindInteractable();

        if (newInteractable == _interactable) {
            return;
        }

        _interactable = newInteractable;

        if (_interactable != null) {
            HandleNewInteractable();
        } else {
            HandleRemovedInteractable();
        }
    }

    private IInteractable FindInteractable() {
        ContactFilter2D contactFilter = new ContactFilter2D();
        contactFilter.useTriggers = true;
        contactFilter.SetLayerMask(_interactableMask);

        int numberOfInteractable = Physics2D.OverlapCircle(_interactionPoint.position, _interactionPointRadius, contactFilter, _colliderResults);

        return numberOfInteractable > 0 ?
            _colliderResults[0].GetComponent<IInteractable>() :
            null;
    }

    private void HandleNewInteractable() {
        if (_promptController != null) {
            _promptController.Show(_interactable.InteractionIcon, _interactable.InteractionPrompt);
        }
    }

    private void HandleRemovedInteractable() {
        if (_promptController != null) {
            _promptController.Hide();
        }
    }

    private bool IsInteractHeld() {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) {
            return Keyboard.current.fKey.isPressed;
        }
#endif
        return Input.GetKey(KeyCode.F);
    }

    private bool IsInteractPressed() {
#if ENABLE_INPUT_SYSTEM
        if (Keyboard.current != null) {
            return Keyboard.current.fKey.wasPressedThisFrame;
        }
#endif
        return Input.GetKeyDown(KeyCode.F);
    }
}
