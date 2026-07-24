using DG.Tweening;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class ChromaticAberrationPulse : Singleton<ChromaticAberrationPulse> {

    [SerializeField]
    private Volume volume;

    [SerializeField]
    private float pulseIntensity = 1f;

    [SerializeField]
    private float pulseDuration = 0.2f;

    private ChromaticAberration chromaticAberration;
    private Tween activePulse;

    private void Start() {
        chromaticAberration = FindOrAddChromaticAberration();
    }

    private ChromaticAberration FindOrAddChromaticAberration() {
        if (volume == null) {
            return null;
        }

        if (volume.profile.TryGet(out ChromaticAberration existing)) {
            return existing;
        }

        ChromaticAberration added = volume.profile.Add<ChromaticAberration>();
        added.active = true;
        added.intensity.overrideState = true;
        return added;
    }

    public void Pulse() {
        if (chromaticAberration == null) {
            return;
        }

        activePulse?.Kill();
        chromaticAberration.intensity.value = pulseIntensity;
        activePulse = DOTween.To(
            () => chromaticAberration.intensity.value,
            newIntensity => chromaticAberration.intensity.value = newIntensity,
            0f,
            pulseDuration
        ).SetEase(Ease.OutCubic).SetUpdate(true);
    }
}
