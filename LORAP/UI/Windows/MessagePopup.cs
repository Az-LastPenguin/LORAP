using System.Collections.Generic;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;

namespace LORAP.CustomUI
{
    internal class MessagePopup : SingletonBehavior<MessagePopup>
    {
        private GameObject Panel;

        private Animator Animator;

        private GameObject SkipButton;

        private TextMeshProUGUI PopupText;
        private GameObject MessageNumber;
        private TextMeshProUGUI MessageNumberText;


        private List<string> Messages = new List<string>();

        internal void Awake()
        {
            // Get all the stuff
            Panel = transform.Find("Popup").gameObject;

            Animator = Panel.GetComponent<Animator>();

            PopupText = Panel.transform.Find("PopupText").GetComponent<TextMeshProUGUI>();

            MessageNumber = Panel.transform.Find("TotalNumber").gameObject;
            MessageNumberText = MessageNumber.GetComponent<TextMeshProUGUI>();

            // Add button events
            Transform Buttons = Panel.transform.Find("Buttons/Layout");
            Buttons.Find("Confirm").gameObject.AddComponent<BasicButton>().MouseClickEvent.AddListener((_) => ConfirmClick());

            SkipButton = Buttons.Find("Skip").gameObject;
            SkipButton.AddComponent<BasicButton>().MouseClickEvent.AddListener((_) => SkipClick());

            // Start inactive
            Panel.SetActive(false);
        }

        private void ConfirmClick()
        {
            NextMessage();
        }

        private void SkipClick()
        {
            Messages.Clear();

            Close();
        }

        internal void ShowMessage(string message)
        {
            Messages.Add(message);

            // If there is a battle currently or it's about to start, we delay this message.
            if (StageController.Instance.battleState != StageController.BattleState.None)
                return;

            Open();
        }

        internal void Open()
        {
            UpdatePanel();

            if (Panel.activeSelf)
                return;

            Panel.SetActive(true);

            UISoundManager.instance.PlayEffectSound(UISoundType.Gacha_Hexagon);

            Animator.SetTrigger("MessagePopupReveal");

            NextMessage();
        }

        internal void Close()
        {
            Panel.SetActive(false);
        }

        private void NextMessage()
        {
            if (Messages.Count <= 0)
            {
                Close();
                return;
            }

            string Message = Messages.First();
            Messages.RemoveAt(0);

            PopupText.text = Message;

            UpdatePanel();
        }

        private void UpdatePanel()
        {
            // If there are any messages left, show how much and an option to skip
            SkipButton.SetActive(Messages.Count > 0);
            MessageNumber.SetActive(Messages.Count > 0);
            if (Messages.Count > 0)
                MessageNumberText.text = $"{Messages.Count} more messages!";
        }
    }
}
