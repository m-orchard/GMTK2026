using UnityEngine;

public class PieceSpawner : MonoBehaviour
{
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private GameObject bodyPrefab;
    [SerializeField] private GameObject enginePrefab;
    [Range(0f, 1f)]
    [SerializeField] private float engineChance = 0.3f;
    [SerializeField] private float wellMinX = -2f;
    [SerializeField] private float wellMaxX = 2f;

    public FallingPieceController Active { get; private set; }

    public void SpawnNext()
    {
        GameObject prefab = Random.value < engineChance ? enginePrefab : bodyPrefab;
        GameObject instance = Instantiate(prefab, spawnPoint.position, Quaternion.identity, rocket.transform);

        var controller = instance.GetComponent<FallingPieceController>();
        controller.SetBounds(wellMinX, wellMaxX);
        controller.SetRocket(rocket);
        controller.OnLocked += HandleLocked;
        Active = controller;
    }

    private void HandleLocked()
    {
        Active.OnLocked -= HandleLocked;
        Active = null;
        SpawnNext();
    }

    public void ForceLockActive()
    {
        if (Active == null) return;
        Active.OnLocked -= HandleLocked;
        Active.ForceLock();
        Active = null;
    }
}
