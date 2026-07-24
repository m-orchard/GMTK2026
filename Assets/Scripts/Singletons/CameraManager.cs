using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;

public class CameraManager : Singleton<CameraManager> {

    [SerializeField]
    private RocketAssembly rocket;

    [SerializeField]
    private CinemachineCamera buildCamera;

    [SerializeField]
    private CinemachineCamera launchCamera;

    [SerializeField]
    private CinemachineTargetGroup launchTargetGroup;

    [SerializeField]
    private int activeCameraPriority = 20;

    [SerializeField]
    private int inactiveCameraPriority = 0;

    [SerializeField]
    private float pieceFramingRadius = 0.75f;

    [SerializeField]
    private float padAnchorFramingRadius = 0.5f;

    private bool following;
    private Transform padAnchor;
    private readonly HashSet<Piece> trackedPieces = new HashSet<Piece>();
    private readonly List<Piece> piecesToRemove = new List<Piece>();

    public void StartFollowing() {
        following = true;
        ResetLaunchTargetGroup();
        ActivateCamera(launchCamera);
    }

    public void ResetToBuildFraming() {
        following = false;
        ResetLaunchTargetGroup();
        ActivateCamera(buildCamera);
    }

    private void LateUpdate() {
        if (!following) {
            return;
        }

        UpdatePadAnchor();
        SyncTrackedPiecesWithRocket();
    }

    private void ActivateCamera(CinemachineCamera cameraToActivate) {
        buildCamera.Priority = cameraToActivate == buildCamera ? activeCameraPriority : inactiveCameraPriority;
        launchCamera.Priority = cameraToActivate == launchCamera ? activeCameraPriority : inactiveCameraPriority;
    }

    private void ResetLaunchTargetGroup() {
        launchTargetGroup.Targets.Clear();
        trackedPieces.Clear();

        UpdatePadAnchor();
        launchTargetGroup.AddMember(GetPadAnchor(), 1f, padAnchorFramingRadius);
    }

    private void UpdatePadAnchor() {
        GetPadAnchor().position = new Vector3(rocket.transform.position.x, rocket.PadY, 0f);
    }

    private Transform GetPadAnchor() {
        if (padAnchor == null) {
            padAnchor = new GameObject("Camera Pad Anchor").transform;
            padAnchor.SetParent(transform, false);
        }

        return padAnchor;
    }

    private void SyncTrackedPiecesWithRocket() {
        HashSet<Piece> connectedPieces = rocket.GetConnectedPieces();

        piecesToRemove.Clear();
        foreach (Piece trackedPiece in trackedPieces) {
            if (!connectedPieces.Contains(trackedPiece)) {
                piecesToRemove.Add(trackedPiece);
            }
        }

        foreach (Piece stalePiece in piecesToRemove) {
            if (stalePiece != null) {
                launchTargetGroup.RemoveMember(stalePiece.transform);
            }
            trackedPieces.Remove(stalePiece);
        }

        foreach (Piece connectedPiece in connectedPieces) {
            if (trackedPieces.Add(connectedPiece)) {
                launchTargetGroup.AddMember(connectedPiece.transform, 1f, pieceFramingRadius);
            }
        }
    }
}
