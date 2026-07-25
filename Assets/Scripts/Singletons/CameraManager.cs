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
    private Transform buildTopTarget;

    [SerializeField]
    private int activeCameraPriority = 20;

    [SerializeField]
    private int inactiveCameraPriority = 0;

    [SerializeField]
    private float pieceFramingRadius = 0.75f;

    [SerializeField]
    private float padAnchorFramingRadius = 0.5f;

    [SerializeField]
    private float buildFloorHeight = -8f;

    [SerializeField]
    private float buildTopMargin = 1f;

    [SerializeField]
    private float buildFloorFramingRadius = 0.5f;

    [SerializeField]
    private float buildTopFramingRadius = 0.5f;

    private bool following;
    private bool buildFraming;
    private Transform padAnchor;
    private Transform buildFloorAnchor;
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

    private void LateUpdate() {
        if (following) {
            UpdatePadAnchor();
            SyncTrackedPiecesWithRocket();
            return;
        }

        if (buildFraming) {
            UpdateBuildAnchors();
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

        UpdateBuildAnchors();

        buildTargetGroup.AddMember(GetBuildFloorAnchor(), 1f, buildFloorFramingRadius);
        buildTargetGroup.AddMember(GetBuildTopAnchor(), 1f, buildTopFramingRadius);
    }

    private void UpdatePadAnchor() {
        GetPadAnchor().position = new Vector3(rocket.transform.position.x, rocket.PadY, 0f);
    }

    private void UpdateBuildAnchors() {
        float horizontalCenter = rocket.transform.position.x;

        GetBuildFloorAnchor().position = new Vector3(horizontalCenter, buildFloorHeight, 0f);

        float craneTop = buildTopTarget != null ? buildTopTarget.position.y : rocket.HighestPointY();
        GetBuildTopAnchor().position = new Vector3(horizontalCenter, craneTop + buildTopMargin, 0f);
    }

    private Transform GetPadAnchor() {
        if (padAnchor == null) {
            padAnchor = new GameObject("Camera Pad Anchor").transform;
            padAnchor.SetParent(transform, false);
        }

        return padAnchor;
    }

    private Transform GetBuildFloorAnchor() {
        if (buildFloorAnchor == null) {
            buildFloorAnchor = new GameObject("Camera Build Floor Anchor").transform;
            buildFloorAnchor.SetParent(transform, false);
        }

        return buildFloorAnchor;
    }

    private Transform GetBuildTopAnchor() {
        if (buildTopAnchor == null) {
            buildTopAnchor = new GameObject("Camera Build Top Anchor").transform;
            buildTopAnchor.SetParent(transform, false);
        }

        return buildTopAnchor;
    }

    private void SyncTrackedPiecesWithRocket() {
        HashSet<Piece> connectedPieces = rocket.Rocket.Pieces;

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
