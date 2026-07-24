using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : Singleton<ScreenShake> {

    [SerializeField]
    private CinemachineImpulseSource impulseSource;

    [SerializeField]
    private CinemachineImpulseSource recoilImpulseSource;

    [SerializeField]
    private float defaultForce = 1f;

    [SerializeField]
    private float defaultRecoilForce = 0.5f;

    public void Shake() {
        Shake(defaultForce);
    }

    public void Shake(float force) {
        if (impulseSource == null) {
            impulseSource = GetComponent<CinemachineImpulseSource>();
        }

        if (impulseSource == null) {
            return;
        }

        impulseSource.GenerateImpulseWithForce(force);
    }

    public void Recoil() {
        Recoil(defaultRecoilForce);
    }

    public void Recoil(float force) {
        if (recoilImpulseSource == null) {
            return;
        }

        recoilImpulseSource.GenerateImpulseWithForce(force);
    }
}
