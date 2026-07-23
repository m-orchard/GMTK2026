using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Health))]
public class FlashOnHit : MonoBehaviour {
    [SerializeField] private Color flashColor = Color.white;
    [SerializeField] private float flashDuration = 0.25f;
    [SerializeField] private AnimationCurve flashSpeedCurve;

    private readonly List<Material> materials = new();
    private Health health;
    private Coroutine flashRoutine;

    private void Awake() {
        health = GetComponent<Health>();
        CacheMaterials();
    }

    private void OnEnable() {
        health.OnDamaged += Flash;
    }

    private void OnDisable() {
        health.OnDamaged -= Flash;
    }

    private void CacheMaterials() {
        foreach (SpriteRenderer spriteRenderer in GetComponentsInChildren<SpriteRenderer>()) {
            materials.Add(spriteRenderer.material);
        }
    }

    private void Flash(float damageAmount) {
        if (flashRoutine != null) {
            StopCoroutine(flashRoutine);
        }

        flashRoutine = StartCoroutine(FlashRoutine());
    }

    private IEnumerator FlashRoutine() {
        SetFlashColor();

        float elapsedTime = 0f;
        while (elapsedTime < flashDuration) {
            elapsedTime += Time.deltaTime;
            float flashAmount = Mathf.Lerp(1f, flashSpeedCurve.Evaluate(elapsedTime), elapsedTime / flashDuration);
            SetFlashAmount(flashAmount);
            yield return null;
        }
    }

    private void SetFlashColor() {
        foreach (Material material in materials) {
            material.SetColor("_FlashColor", flashColor);
        }
    }

    private void SetFlashAmount(float amount) {
        foreach (Material material in materials) {
            material.SetFloat("_FlashAmount", amount);
        }
    }
}