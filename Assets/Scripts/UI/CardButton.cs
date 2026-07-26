using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public event Action Clicked;
    public event Action PointerEntered;
    public event Action PointerExited;
    public event Action PointerPressed;
    public event Action PointerReleased;

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        PointerEntered?.Invoke();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        PointerExited?.Invoke();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        PointerPressed?.Invoke();
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        PointerReleased?.Invoke();
    }
}
