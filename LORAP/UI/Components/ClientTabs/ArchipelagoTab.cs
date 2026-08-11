using LORAP.Archipelago;
using LORAP.Utils;
using TMPro;
using UnityEngine;

namespace LORAP.CustomUI.Components.ClientTabs
{
    internal class ArchipelagoTab : MonoBehaviour, IClientTab
    {
        private GameObject MessagePrefab = AssetBundleHelper.GetAsset("APClientMessage");

        private GameObject Tab;

        private Transform MessageHolder;
        private TMP_InputField InputField;

        public void Awake()
        {
            // Get all the stuff
            Tab = gameObject;

            MessageHolder = Tab.transform.Find("ScrollRect/Viewport/Content");
            InputField = Tab.transform.Find("MessageInput").GetComponent<TMP_InputField>();

            // Listen for MessageEvent to show messages in Archipelago Tab
            SessionManager.MessageEvent += AddMessage;

            // Make Clear Messages button work
            Tab.transform.Find("Clear").gameObject.AddComponent<BasicButton>().MouseClickEvent.AddListener((_) => ClearMessages());
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(InputField?.text))
            {
                SessionManager.SayMessage(InputField.text);
                InputField.text = "";
            }

            InputField.ActivateInputField();
        }

        public void UpdateTab() { }

        public void ResetTab()
        {
            ClearMessages();
        }

        private void AddMessage(string message)
        {
            Instantiate(MessagePrefab, MessageHolder).GetComponent<TextMeshProUGUI>().text = message;
        }

        private void ClearMessages()
        {
            for (int i = MessageHolder.childCount - 1; i >= 0; i--)
                Destroy(MessageHolder.GetChild(i).gameObject);
        }
    }
}
