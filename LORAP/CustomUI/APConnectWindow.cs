using LORAP.Archipelago;
using LORAP.Gameplay;
using LORAP.Playthru;
using LORAP.Utils;
using UnityEngine;
using UnityEngine.EventSystems;
using System;
using System.Diagnostics;
using TMPro;
using System.Linq;

namespace LORAP.CustomUI 
{
    internal static class APConnectWindow
    {
        private static GameObject Panel;

        private static Animator Animator => Panel.GetComponent<Animator>();

        private static TMP_InputField IPInput => Panel.transform.Find("APJoinPanel/Window/Container/Layout/CenterPanel/Inputs/IPPortInput").gameObject.GetComponent<TMP_InputField>();
        private static TMP_InputField SlotInput => Panel.transform.Find("APJoinPanel/Window/Container/Layout/CenterPanel/Inputs/SlotInput").gameObject.GetComponent<TMP_InputField>();
        private static TMP_InputField PassInput => Panel.transform.Find("APJoinPanel/Window/Container/Layout/CenterPanel/Inputs/PasswordInput").gameObject.GetComponent<TMP_InputField>();
        private static TextMeshProUGUI InfoText => Panel.transform.Find("APJoinPanel/Window/Container/Layout/CenterPanel/Texts/InfoText").gameObject.GetComponent<TextMeshProUGUI>();

        private static GameObject ConnectButton => Panel.transform.Find("APJoinPanel/Window/Container/ButtonLayout/Connect").gameObject;
        private static GameObject CancelButton => Panel.transform.Find("APJoinPanel/Window/Container/ButtonLayout/Cancel").gameObject;

        internal static string IP => Panel != null ? IPInput.text : "";
        internal static string Slot => Panel != null ? SlotInput.text : "";
        internal static string Password => Panel != null ? PassInput.text : "";

        internal static void Init()
        {
            Panel = GameObject.Instantiate(AssetBundleHelper.GetAsset("APJoinCanvas"));

            ConnectButton.AddComponent<BasicButtonScript>().MouseClickEvent.AddListener(ConnectButtonClick);
            CancelButton.AddComponent<BasicButtonScript>().MouseClickEvent.AddListener(CancelButtonClick);

            Panel.SetActive(false);
        }

        internal static void Open()
        {
            Animator.SetTrigger("ConnectRevealAnim");

            var LastData = SaveManager.LoadLastSessionData();
            if (LastData != null)
            {
                IPInput.text = LastData.IP ?? "";
                SlotInput.text = LastData.SlotName ?? "";

                InfoText.text = $"Last run progress: {LastData.Progress.ToString("P1") ?? "Unknown"}";
            }

            Panel.SetActive(true);
        }

        internal static void Close()
        {
            Panel.SetActive(false);
        }

        internal static void SetInfoText(string text)
        {
            InfoText.text = text;
        }

        private static void ConnectButtonClick(PointerEventData eventData)
        {
            try
            {
                // Set session data
                SessionManager.sessionData = new SessionData()
                {
                    IP = IP,
                    SlotName = Slot,
                    Password = Password,
                };

                // Try connecting to the slot
                SessionManager.TryConnect(IP, Slot, Password);

                // If we successfully connected, start the game
                PlaythruManager.StartGame();
            }
            catch (Exception e)
            {
                string origin = $"{e.TargetSite.DeclaringType.FullName}.{e.TargetSite.Name}";
                string exText = $"[{origin}]\n{e.Message}";
                SetInfoText(exText);

                SessionManager.EndSession();

                UnityEngine.Debug.LogException(e);
                return;
            }
        }

        private static void CancelButtonClick(PointerEventData eventData)
        {
            Close();
        }
    }
}
