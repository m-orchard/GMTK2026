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
    private Sequence fetchSequence;

    public bool IsReady { get; private set; }

    public void Fetch(GameObject cargoInstance)
    {
        fetchSequence?.Kill();
        IsReady = false;
        carriedCargo = cargoInstance;
        cargoInstance.transform.position = pickupPoint.position;

        fetchSequence = DOTween.Sequence();
        fetchSequence.Append(transform.DOMove(pickupPoint.position, travelDuration).SetEase(travelEase));
        fetchSequence.AppendInterval(grabPause);
        fetchSequence.AppendCallback(() => cargoInstance.transform.SetParent(transform, worldPositionStays: true));
        fetchSequence.Append(transform.DOMove(holdPoint.position, travelDuration).SetEase(travelEase));
        fetchSequence.OnComplete(() => IsReady = true);
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
        carriedCargo = null;
        IsReady = false;
        return released;
    }

    public void ResetCrane()
    {
        fetchSequence?.Kill();
        fetchSequence = null;
        IsReady = false;

        if (carriedCargo != null)
        {
            Destroy(carriedCargo);
            carriedCargo = null;
        }

        if (holdPoint != null)
            transform.position = holdPoint.position;
    }
}
