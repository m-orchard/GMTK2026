using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InteractionPromptController : MonoBehaviour {

    [SerializeField]
    private Image _icon;

    [SerializeField]
    private TMP_Text _text;

    private void Awake() {
        Hide();
    }

    public void Show(Sprite icon, string text) {
        _icon.sprite = icon;
        _text.text = text;
        gameObject.SetActive(true);
    }

    public void Hide() {
        gameObject.SetActive(false);
    }
}
