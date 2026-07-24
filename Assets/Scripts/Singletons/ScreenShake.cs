using Unity.Cinemachine;
using UnityEngine;

public class ScreenShake : Singleton<ScreenShake> {

    [SerializeField]
    private CinemachineImpulseSource impulseSource;

    [SerializeField]
    private float defaultForce = 1f;

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
}
