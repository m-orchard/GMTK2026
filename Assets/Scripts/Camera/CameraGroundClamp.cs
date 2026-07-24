using Unity.Cinemachine;
using UnityEngine;

[SaveDuringPlay]
[AddComponentMenu("Cinemachine/Extensions/Camera Ground Clamp")]
public class CameraGroundClamp : CinemachineExtension {

    [SerializeField]
    private float groundHeight;

    protected override void PostPipelineStageCallback(
        CinemachineVirtualCameraBase virtualCamera,
        CinemachineCore.Stage stage,
        ref CameraState state,
        float deltaTime) {

        if (stage != CinemachineCore.Stage.Finalize) {
            return;
        }

        float distanceFromCenterToBottomEdge = state.Lens.OrthographicSize;
        float lowestVisibleHeight = state.RawPosition.y - distanceFromCenterToBottomEdge;

        if (lowestVisibleHeight >= groundHeight) {
            return;
        }

        Vector3 clampedPosition = state.RawPosition;
        clampedPosition.y = groundHeight + distanceFromCenterToBottomEdge;
        state.RawPosition = clampedPosition;
    }
}
