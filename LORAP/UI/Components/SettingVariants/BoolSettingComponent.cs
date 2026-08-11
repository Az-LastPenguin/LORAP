using UI;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

internal class BoolSettingChangeEvent : UnityEvent<bool> { }

internal class BoolSettingComponent : Selectable
{
    internal BoolSettingChangeEvent ChangeEvent = new BoolSettingChangeEvent();

    private GameObject Checkmark;

    private bool Value = false;

    protected override void Awake()
    {
        Checkmark = transform.Find("Setting/CheckmarkHolder/Checkmark").gameObject;
    }

    public void SetValue(bool value)
    {
        Value = value;

        Checkmark.SetActive(value);
    }

    public override void OnPointerEnter(PointerEventData eventData)
    {

    }

    public override void OnPointerExit(PointerEventData eventData)
    {

    }

    public override void OnPointerUp(PointerEventData eventData)
    {
        base.OnPointerUp(eventData);

        if (!IsActive() || !IsInteractable())
            return;

        UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

        SetValue(!Value);

        ChangeEvent.Invoke(Value);
    }
}
