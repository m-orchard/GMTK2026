using DG.Tweening;
using UnityEngine;

public class CargoCrane : MonoBehaviour
{
    [SerializeField] private Transform holdPoint;
    [SerializeField] private Transform pickupPoint;
    [SerializeField] private float travelDuration = 1f;
    [SerializeField] private Ease travelEase = Ease.InOutQuad;
    [SerializeField] private float grabPause = 0.2f;

    private GameObject carriedCargo;
    private Rigidbody2D carriedBody;
    private Vector3 carriedLocalOffset;
    private bool holdingCargo;
    private Sequence fetchSequence;

    public bool IsReady { get; private set; }

    public void Fetch(GameObject cargoInstance)
    {
        fetchSequence?.Kill();
        IsReady = false;
        holdingCargo = false;
        carriedCargo = cargoInstance;
        carriedBody = cargoInstance.GetComponent<Rigidbody2D>();
        if (carriedBody != null)
            carriedBody.simulated = false;
        cargoInstance.transform.position = pickupPoint.position;

        Vector3 pickupLocalPosition = ToParentLocalPosition(pickupPoint.position);
        Vector3 holdLocalPosition = ToParentLocalPosition(holdPoint.position);

        fetchSequence = DOTween.Sequence();
        fetchSequence.Append(transform.DOLocalMove(pickupLocalPosition, travelDuration).SetEase(travelEase));
        fetchSequence.AppendInterval(grabPause);
        fetchSequence.AppendCallback(() => GrabCargo(cargoInstance));
        fetchSequence.Append(transform.DOLocalMove(holdLocalPosition, travelDuration).SetEase(travelEase));
        fetchSequence.OnComplete(() => IsReady = true);
    }

    private void GrabCargo(GameObject cargoInstance)
    {
        cargoInstance.transform.SetParent(transform, worldPositionStays: true);
        carriedLocalOffset = transform.InverseTransformPoint(cargoInstance.transform.position);
        holdingCargo = true;
    }

    private void LateUpdate()
    {
        if (carriedCargo == null)
            return;

        carriedCargo.transform.position = holdingCargo
            ? transform.TransformPoint(carriedLocalOffset)
            : pickupPoint.position;
    }

    private Vector3 ToParentLocalPosition(Vector3 worldPosition)
    {
        return transform.parent != null
            ? transform.parent.InverseTransformPoint(worldPosition)
            : worldPosition;
    }

    public void StopFetching()
    {
        fetchSequence?.Kill();
        fetchSequence = null;
        IsReady = false;
    }

    public GameObject ReleaseHeld()
    {
        GameObject released = carriedCargo;

        if (carriedBody != null)
            carriedBody.simulated = true;

        carriedCargo = null;
        carriedBody = null;
        holdingCargo = false;
        IsReady = false;
        return released;
    }

    public void ResetCrane()
    {
        fetchSequence?.Kill();
        fetchSequence = null;
        IsReady = false;
        holdingCargo = false;

        if (carriedCargo != null)
        {
            Destroy(carriedCargo);
            carriedCargo = null;
        }

        carriedBody = null;

        if (holdPoint != null)
            transform.position = holdPoint.position;
    }
}
