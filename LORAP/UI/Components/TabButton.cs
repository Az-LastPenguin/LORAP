using LORAP.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class TabButton : Selectable
{
    internal PointerEvent MouseClickEvent = new PointerEvent();

    internal bool IsSelected = false;

    private GameObject BackImageObject;

    protected override void Awake()
    {
        colors = UIUtils.BasicButtonColors;
        targetGraphic = GetComponentInChildren<TextMeshProUGUI>();
        BackImageObject = GetComponentInChildren<Image>().gameObject;

        BackImageObject.SetActive(false);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (IsActive() && IsInteractable() && !IsSelected)
            BackImageObject.SetActive(true);
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (IsActive() && IsInteractable() && !IsSelected)
            BackImageObject.SetActive(false);
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (IsActive() && IsInteractable())
            MouseClickEvent.Invoke(eventData);
    }

    internal void SetSelected(bool isSelected)
    {
        BackImageObject.SetActive(isSelected);
        IsSelected = isSelected;
    }
}
