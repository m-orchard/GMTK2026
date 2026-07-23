using UnityEngine;

public class GoalLineDisplay : MonoBehaviour
{
    [SerializeField] private GameManager gameManager;
    [SerializeField] private RocketAssembly rocket;
    [SerializeField] private float width = 1000f;
    [SerializeField] private float thickness = 0.4f;
    [SerializeField] private Color zoneColor = new Color(0.2f, 1f, 0.4f, 0.35f);

    private void Awake()
    {
        BuildZoneSprite();
    }

    private void OnEnable()
    {
        gameManager.OnTargetHeightChanged += SetTargetHeight;
    }

    private void OnDisable()
    {
        gameManager.OnTargetHeightChanged -= SetTargetHeight;
    }

    private void SetTargetHeight(float targetHeight)
    {
        Vector3 pos = transform.position;
        pos.y = rocket.PadY + targetHeight;
        transform.position = pos;
    }

    private void BuildZoneSprite()
    {
        var texture = new Texture2D(1, 1);
        texture.SetPixel(0, 0, Color.white);
        texture.Apply();

        var sprite = Sprite.Create(texture, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);

        var renderer = gameObject.AddComponent<SpriteRenderer>();
        renderer.sprite = sprite;
        renderer.color = zoneColor;
        renderer.sortingOrder = -1;

        transform.localScale = new Vector3(width, thickness, 1f);
    }
}
