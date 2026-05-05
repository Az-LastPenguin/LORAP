using System;
using LORAP.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BasicButtonScript : CustomSelectable
{
    protected override void Awake()
    {
        targetGraphic = transform.Find("Image").gameObject.GetComponent<Image>();
        text = GetComponentInChildren<TextMeshProUGUI>();
        colors = UIUtils.BasicButtonColors;

        base.Awake();
    }
}
