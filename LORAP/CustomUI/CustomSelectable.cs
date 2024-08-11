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

    private TextMeshProUGUI input;

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    protected override void Awake()
    {
        MouseClickEvent = new();

        input = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        image = gameObject.transform.Find("[Image]buttonImage").gameObject.GetComponent<Image>();

        base.Awake();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        if (base.interactable)
        {
            input.color = new Color(0.1333f, 1, 0.8941f, 1);
            image.color = new Color(0.1333f, 1, 0.8941f, 1);

            base.OnPointerEnter(eventData);
        }
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        if (base.interactable)
        {
            input.color = new Color(0.9372f, 0.7607f, 0.5058f, 1);
            image.color = new Color(0.9372f, 0.7607f, 0.5058f, 1);

            base.OnPointerExit(eventData);
        }
    }

    public override void OnPointerDown(PointerEventData eventData)
    {
        if (base.interactable)
        {
            base.OnPointerDown(eventData);
        }
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        if (base.interactable)
        {
            MouseClickEvent.Invoke(eventData);

            OnPointerExit(eventData);

            base.OnPointerUp(eventData);
        }
    }
}
