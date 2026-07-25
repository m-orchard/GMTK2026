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
    private CinemachineTargetGroup buildTargetGroup;

    [SerializeField]
    private Transform craneRig;

    [SerializeField]
    private Transform conveyorRig;

    [SerializeField]
    private int activeCameraPriority = 20;

    [SerializeField]
    private int inactiveCameraPriority = 0;

    [SerializeField]
    private float pieceFramingRadius = 0.75f;

    [SerializeField]
    private float padAnchorFramingRadius = 0.5f;

    [SerializeField]
    private float rigFramingRadius = 2f;

    [SerializeField]
    private float buildTopHeadroom = 7f;

    [SerializeField]
    private float buildTopFramingRadius = 1f;

    private bool following;
    private bool buildFraming;
    private Transform padAnchor;
    private Transform buildTopAnchor;
    private readonly HashSet<Piece> trackedPieces = new HashSet<Piece>();
    private readonly List<Piece> piecesToRemove = new List<Piece>();

    public void StartFollowing() {
        following = true;
        buildFraming = false;
        ResetLaunchTargetGroup();
        ActivateCamera(launchCamera);
    }

    public void ResetToBuildFraming() {
        following = false;
        ResetLaunchTargetGroup();
        ResetBuildTargetGroup();
        buildFraming = true;
        ActivateCamera(buildCamera);
    }

    public void FreezeBuildFraming() {
        if (craneRig != null) {
            buildTargetGroup.RemoveMember(craneRig);
        }

        if (conveyorRig != null) {
            buildTargetGroup.RemoveMember(conveyorRig);
        }
    }

    private void LateUpdate() {
        if (following) {
            UpdatePadAnchor();
            SyncTrackedPiecesWithRocket();
            return;
        }

        if (buildFraming) {
            UpdateBuildTopAnchor();
        }
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

    private void ResetBuildTargetGroup() {
        buildTargetGroup.Targets.Clear();

        UpdatePadAnchor();
        UpdateBuildTopAnchor();

        buildTargetGroup.AddMember(GetPadAnchor(), 1f, padAnchorFramingRadius);
        buildTargetGroup.AddMember(GetBuildTopAnchor(), 1f, buildTopFramingRadius);

        if (craneRig != null) {
            buildTargetGroup.AddMember(craneRig, 1f, rigFramingRadius);
        }

        if (conveyorRig != null) {
            buildTargetGroup.AddMember(conveyorRig, 1f, rigFramingRadius);
        }
    }

    private void UpdatePadAnchor() {
        GetPadAnchor().position = new Vector3(rocket.transform.position.x, rocket.PadY, 0f);
    }

    private void UpdateBuildTopAnchor() {
        GetBuildTopAnchor().position = new Vector3(rocket.transform.position.x, rocket.HighestPointY() + buildTopHeadroom, 0f);
    }

    private Transform GetPadAnchor() {
        if (padAnchor == null) {
            padAnchor = new GameObject("Camera Pad Anchor").transform;
            padAnchor.SetParent(transform, false);
        }

        return padAnchor;
    }

    private Transform GetBuildTopAnchor() {
        if (buildTopAnchor == null) {
            buildTopAnchor = new GameObject("Camera Build Top Anchor").transform;
            buildTopAnchor.SetParent(transform, false);
        }

        return buildTopAnchor;
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
