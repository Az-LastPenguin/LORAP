using LORAP.Utils;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;

namespace LORAP.CustomUI
{
    internal static class MessagePopup
    {
        private static GameObject Panel;
        internal static bool Active => Panel.activeSelf;

        private static Animator Animator => Panel.GetComponent<Animator>();

        private static GameObject ConfirmButton => Panel.transform.Find("Popup/Buttons/Layout/Confirm").gameObject;
        private static GameObject SkipButton => Panel.transform.Find("Popup/Buttons/Layout/Skip").gameObject;

        private static TextMeshProUGUI Text => Panel.transform.Find("Popup/PopupText").gameObject.GetComponent<TextMeshProUGUI>();
        private static GameObject MessageNumber => Panel.transform.Find("Popup/TotalNumber").gameObject;
        private static TextMeshProUGUI MessageNumberText => MessageNumber.GetComponent<TextMeshProUGUI>();


        private static List<string> messages = new List<string>();

        internal static void Init()
        {
            Panel = GameObject.Instantiate(AssetBundleHelper.GetAsset("MessagePopup"));

            ConfirmButton.AddComponent<BasicButtonScript>().MouseClickEvent.AddListener(ConfirmClick);
            SkipButton.AddComponent<BasicButtonScript>().MouseClickEvent.AddListener(SkipClick);

            Panel.SetActive(false);
        }

        internal static void ShowMessage(string message)
        {
            messages.Add(message);

            // If there is a battle currently or it's about to start, we delay this message.
            if (StageController.Instance.battleState != StageController.BattleState.None)
                return;

            Open();
        } 

        internal static void Open()
        {
            UpdatePanel();

            if (Panel.activeSelf)
                return;

            Panel.SetActive(true);

            UISoundManager.instance.PlayEffectSound(UISoundType.Gacha_Hexagon);

            Animator.SetTrigger("PopupRevealAnim");

            NextMessage();
        }

        private static void Close()
        {
            Panel.SetActive(false);
        }

        private static void NextMessage()
        {
            if (messages.Count <= 0)
            {
                Close();
                return;
            }

            string Message = messages.First();
            messages.RemoveAt(0);

            Text.text = Message;

            UpdatePanel();
        }

        private static void UpdatePanel()
        {
            // If there are any messages left, show how much and an option to skip
            SkipButton.SetActive(messages.Count > 0);
            MessageNumber.SetActive(messages.Count > 0);
            if (messages.Count > 0)
                MessageNumberText.text = $"{messages.Count} more messages!";
        }

        private static void ConfirmClick(PointerEventData eventData)
        {
            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

            NextMessage();
        }

        private static void SkipClick(PointerEventData eventData)
        {
            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

            messages.Clear();

            Close();
        }
    }
}
