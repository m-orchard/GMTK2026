using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Fuel : MonoBehaviour
{
    [SerializeField]
    private float value = 1f;

    public float Value { get => value; }

    public int Group = 1;

    public GameObject LabelGroup;

    public TextMeshPro Label;

    public SpriteRenderer Indicator;

    [SerializeField]
    private static Dictionary<int, Color> GroupColours = new()
    {
        { 1, new Color(0, 255, 255) },
        { 2, new Color(255, 0, 255) },
        { 3, new Color(255, 255, 0) },
    };

    void Awake()
    {
        Group = Random.Range(1, 4);
        Indicator.color = GroupColours[Group];
        Label = GetComponentInChildren<TextMeshPro>();
        Label.text = $"+{Value}";
    }

    void LateUpdate()
    {
        LabelGroup.transform.rotation = Quaternion.identity;
    }

    public void SetValue(float value)
    {
        this.value = value;
        Label.text = $"+{Value}";
    }
}
