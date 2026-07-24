using System;
using System.Collections.Generic;
using UnityEngine;

public class Conveyor : MonoBehaviour
{
    [SerializeField] private int slotCount = 4;
    [SerializeField] private Transform frontSlot;
    [SerializeField] private Transform dropPoint;
    [SerializeField] private Vector2 slotOffset = new Vector2(1.5f, 0f);
    [SerializeField] private float moveSpeed = 4f;
    [SerializeField] private float releaseSpeed = 4f;
    [SerializeField] private float arrivalThreshold = 0.02f;
    [SerializeField] private SpriteRenderer beltSurface;
    [SerializeField] private float scrollSpeed = 0.5f;

    private readonly List<Rigidbody2D> queue = new List<Rigidbody2D>();
    private Rigidbody2D releasing;

    public event Action<GameObject> OnPieceReachedDrop;

    public int SlotCount => slotCount;

    public void Enqueue(GameObject instance)
    {
        var body = instance.GetComponent<Rigidbody2D>();
        instance.transform.SetParent(transform, worldPositionStays: true);
        Vector2 entryPosition = SlotPosition(queue.Count + 1);
        body.position = entryPosition;
        instance.transform.position = entryPosition;
        queue.Add(body);
    }

    public void ReleaseFront()
    {
        if (releasing != null || queue.Count == 0)
            return;

        releasing = queue[0];
        queue.RemoveAt(0);
    }

    public void ReplaceFront(GameObject instance)
    {
        if (queue.Count == 0)
            return;

        Rigidbody2D outgoing = queue[0];
        Vector2 frontPosition = outgoing != null ? outgoing.position : SlotPosition(0);

        if (outgoing != null)
            Destroy(outgoing.gameObject);

        var body = instance.GetComponent<Rigidbody2D>();
        instance.transform.SetParent(transform, worldPositionStays: true);
        body.position = frontPosition;
        instance.transform.position = frontPosition;
        queue[0] = body;
    }

    public void Clear()
    {
        foreach (var body in queue)
        {
            if (body != null)
                Destroy(body.gameObject);
        }
        queue.Clear();

        if (releasing != null)
        {
            Destroy(releasing.gameObject);
            releasing = null;
        }
    }

    private void FixedUpdate()
    {
        float queueStep = moveSpeed * Time.fixedDeltaTime;
        for (int i = 0; i < queue.Count; i++)
        {
            Rigidbody2D body = queue[i];
            if (body == null)
                continue;

            Vector2 next = Vector2.MoveTowards(body.position, SlotPosition(i), queueStep);
            body.MovePosition(next);
        }

        AdvanceReleasingPiece();
    }

    private void AdvanceReleasingPiece()
    {
        if (releasing == null)
            return;

        Vector2 target = dropPoint != null ? (Vector2)dropPoint.position : SlotPosition(0);
        Vector2 next = Vector2.MoveTowards(releasing.position, target, releaseSpeed * Time.fixedDeltaTime);
        releasing.MovePosition(next);

        if (Vector2.Distance(next, target) > arrivalThreshold)
            return;

        GameObject arrived = releasing.gameObject;
        releasing = null;
        OnPieceReachedDrop?.Invoke(arrived);
    }

    private void Update()
    {
        if (beltSurface == null)
            return;

        Vector2 offset = beltSurface.material.mainTextureOffset;
        offset.x += scrollSpeed * Time.deltaTime;
        beltSurface.material.mainTextureOffset = offset;
    }

    private Vector2 SlotPosition(int slotIndex)
    {
        Vector2 origin = frontSlot != null ? (Vector2)frontSlot.position : (Vector2)transform.position;
        return origin + slotOffset * slotIndex;
    }
}
