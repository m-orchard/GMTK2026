using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SFXAudioButton : MonoBehaviour {

    [SerializeField]
    private Image _offImage;

    private bool _isOff = false;

    private void Awake() {
        _isOff = AudioManager.Instance.IsSoundMuted;
        UpdateOffSprite();
    }

    public void ToggleSFX() {
        AudioManager.Instance.ToggleSFX();
        _isOff = !_isOff;
        UpdateOffSprite();
    }

    public void UpdateOffSprite() {
        _offImage.enabled = _isOff;
    }
}