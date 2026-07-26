using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class CardButton : MonoBehaviour, IPointerClickHandler
{
    public event Action Clicked;

    public void OnPointerClick(PointerEventData eventData)
    {
        Clicked?.Invoke();
    }
}
