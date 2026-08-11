using LORAP.Utils;
using TMPro;
using UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class PointerEvent : UnityEvent<PointerEventData> { }

internal class BasicButton : Selectable
{
    internal PointerEvent MouseClickEvent = new PointerEvent();

    internal TextMeshProUGUI Text;

    protected override void Awake()
    {
        colors = UIUtils.BasicButtonColors;
        targetGraphic = GetComponentInChildren<Image>();
        Text = GetComponentInChildren<TextMeshProUGUI>();
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {
        base.OnPointerEnter(eventData);

        if (IsActive() && IsInteractable())
            Text.color = colors.highlightedColor;
    }

    public override void OnPointerExit(PointerEventData eventData)
    {
        base.OnPointerExit(eventData);

        if (IsActive() && IsInteractable())
            Text.color = colors.normalColor;
    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!IsActive() || !IsInteractable())
            return;

        UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

        MouseClickEvent.Invoke(eventData);
    }
}
