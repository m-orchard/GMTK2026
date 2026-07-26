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
    private bool dispensing = true;

    public event Action<GameObject> OnPieceReachedDrop;

    public int SlotCount => slotCount;

    public void Enqueue(GameObject instance)
    {
        var body = instance.GetComponent<Rigidbody2D>();
        instance.transform.SetParent(transform, worldPositionStays: true);
        SetLocalPosition(body, SlotLocalPosition(queue.Count + 1));
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
        Vector2 frontLocalPosition = outgoing != null
            ? (Vector2)outgoing.transform.localPosition
            : SlotLocalPosition(0);

        if (outgoing != null)
            Destroy(outgoing.gameObject);

        var body = instance.GetComponent<Rigidbody2D>();
        instance.transform.SetParent(transform, worldPositionStays: true);
        SetLocalPosition(body, frontLocalPosition);
        queue[0] = body;
    }

    public void StopDispensing()
    {
        dispensing = false;
    }

    public void Clear()
    {
        dispensing = true;

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

            Vector2 nextLocal = Vector2.MoveTowards(body.transform.localPosition, SlotLocalPosition(i), queueStep);
            SetLocalPosition(body, nextLocal);
        }

        AdvanceReleasingPiece();
    }

    private void AdvanceReleasingPiece()
    {
        if (releasing == null || !dispensing)
            return;

        Vector2 targetLocal = DropLocalPosition();
        Vector2 nextLocal = Vector2.MoveTowards(releasing.transform.localPosition, targetLocal, releaseSpeed * Time.fixedDeltaTime);
        SetLocalPosition(releasing, nextLocal);

        if (Vector2.Distance(nextLocal, targetLocal) > arrivalThreshold)
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

    private Vector2 SlotLocalPosition(int slotIndex)
    {
        Vector2 origin = frontSlot != null ? (Vector2)transform.InverseTransformPoint(frontSlot.position) : Vector2.zero;
        return origin + slotOffset * slotIndex;
    }

    private Vector2 DropLocalPosition()
    {
        return dropPoint != null ? (Vector2)transform.InverseTransformPoint(dropPoint.position) : SlotLocalPosition(0);
    }

    private static void SetLocalPosition(Rigidbody2D body, Vector2 localPosition)
    {
        Vector3 current = body.transform.localPosition;
        body.transform.localPosition = new Vector3(localPosition.x, localPosition.y, current.z);
    }
}
