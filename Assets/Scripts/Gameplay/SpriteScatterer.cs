using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpriteScatterer : MonoBehaviour
{
    [FormerlySerializedAs("cloudPrefab")]
    [SerializeField] private ScatteredSprite spritePrefab;
    [SerializeField] private float minX = -20f;
    [SerializeField] private float maxX = 20f;
    [SerializeField] private float minY = 10f;
    [SerializeField] private float maxY = 60f;

    [Header("Density")]
    [Tooltip("Instances per square world unit of the spawn area.")]
    [Range(0f, 0.2f)]
    [FormerlySerializedAs("cloudsPerUnitArea")]
    [SerializeField] private float spritesPerUnitArea = 0.02f;

    private readonly List<ScatteredSprite> spawnedSprites = new List<ScatteredSprite>();

    private void Start()
    {
        Scatter();
    }

    [ContextMenu("Scatter")]
    public void Scatter()
    {
        Clear();

        if (spritePrefab == null)
        {
            return;
        }

        int spawnCount = CalculateSpawnCount();
        for (int spawnIndex = 0; spawnIndex < spawnCount; spawnIndex++)
        {
            SpawnAtRandomPosition();
        }
    }

    private int CalculateSpawnCount()
    {
        float spawnArea = Mathf.Abs(maxX - minX) * Mathf.Abs(maxY - minY);
        return Mathf.RoundToInt(spawnArea * spritesPerUnitArea);
    }

    private void SpawnAtRandomPosition()
    {
        Vector3 spawnPosition = new Vector3(
            Random.Range(minX, maxX),
            Random.Range(minY, maxY),
            0f);

        ScatteredSprite scatteredSprite = Instantiate(spritePrefab, spawnPosition, Quaternion.identity, transform);
        scatteredSprite.Randomise();
        spawnedSprites.Add(scatteredSprite);
    }

    private void Clear()
    {
        foreach (ScatteredSprite scatteredSprite in spawnedSprites)
        {
            if (scatteredSprite != null)
            {
                Destroy(scatteredSprite.gameObject);
            }
        }

        spawnedSprites.Clear();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Vector3 center = new Vector3((minX + maxX) * 0.5f, (minY + maxY) * 0.5f, 0f);
        Vector3 size = new Vector3(Mathf.Abs(maxX - minX), Mathf.Abs(maxY - minY), 0f);
        Gizmos.DrawWireCube(center, size);
    }
}
