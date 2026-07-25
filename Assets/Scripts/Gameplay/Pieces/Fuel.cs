using TMPro;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    public float Value = 1f;

    public int Group = 1;

    void Awake()
    {
        Group = Random.Range(1, 4);
        var tmp = GetComponentInChildren<TextMeshPro>();
        tmp.text = $"{Group}";
    }
}
