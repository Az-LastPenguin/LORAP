using System;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[Serializable]
public class PointerEvent : UnityEvent<PointerEventData> {}

public class CustomSelectable : Selectable
{
    public PointerEvent MouseClickEvent;

    public TextMeshProUGUI text;

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        MouseClickEvent = new PointerEvent();

        base.Awake();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (interactable)
        {
            text.color = colors.highlightedColor;

            base.OnPointerEnter(eventData);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (interactable)
        {
            text.color = colors.normalColor;

            base.OnPointerExit(eventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (interactable)
        {
            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (interactable)
        {
            MouseClickEvent.Invoke(eventData);

            OnDeselect(eventData);

            //OnPointerExit(eventData);

            base.OnPointerUp(eventData);
        }
    }
}
