using LORAP.Archipelago;
using LORAP.Gameplay;
using LORAP.Utils;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

namespace LORAP.CustomUI
{
    internal static class APConnectWindow
    {
        private static GameObject Panel;

        public static string IP
        {
            get
            {
                if (Panel == null)
                    return null;

                return Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/IPPortInput").gameObject.GetComponent<TMP_InputField>().text;
            }
        }

        public static string SlotName
        {
            get
            {
                if (Panel == null)
                    return null;

                return Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/NicknameInput").gameObject.GetComponent<TMP_InputField>().text;
            }
        }

        public static string Password
        {
            get
            {
                if (Panel == null)
                    return null;

                return Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/PasswordInput").gameObject.GetComponent<TMP_InputField>().text;
            }
        }

        internal static void Init()
        {
            Panel = GameObject.Instantiate(PrefabHelper.GetPrefab("archipelagoconnect", "APJoinCanvas"));

            Panel.transform.Find("APJoinPanel/Window/ApJoin/ButtonLayout/Connect").gameObject.AddComponent<CustomSelectable>().MouseClickEvent.AddListener(ConnectButtonClick);
            Panel.transform.Find("APJoinPanel/Window/ApJoin/ButtonLayout/Cancel").gameObject.AddComponent<CustomSelectable>().MouseClickEvent.AddListener(CloseButtonClick);

            Timing.After(3f, () =>
            {
                // Change font
                var TitleText = Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/[Rect]Center_Title/[Text]Title_TextMesh").gameObject.GetComponent<TextMeshProUGUI>();
                var IPFieldText = Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/Texts/IPPortText").gameObject.GetComponent<TextMeshProUGUI>();
                var SlotFieldText = Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/Texts/NicknameText").gameObject.GetComponent<TextMeshProUGUI>();
                var PassFieldText = Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/Texts/PasswordText").gameObject.GetComponent<TextMeshProUGUI>();
                var Text = Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/Texts/InfoText").gameObject.GetComponent<TextMeshProUGUI>();

                TitleText.font = UIHelper.Font3;
                TitleText.fontMaterial = UIHelper.Font3Material;
                IPFieldText.font = UIHelper.Font3;
                IPFieldText.fontMaterial = UIHelper.Font3Material;
                SlotFieldText.font = UIHelper.Font3;
                SlotFieldText.fontMaterial = UIHelper.Font3Material;
                PassFieldText.font = UIHelper.Font3;
                PassFieldText.fontMaterial = UIHelper.Font3Material;
                Text.font = UIHelper.Font3;
                Text.fontMaterial = UIHelper.Font3Material;
            });

            Panel.SetActive(false);
        }

        public static void Open()
        {
            var LastData = SaveManager.LoadLastSessionData();
            if (LastData != null)
            {
                Debug.Log($"{LastData.IP} {LastData.SlotName} {LastData.Progress}");
                Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/IPPortInput").gameObject.GetComponent<TMP_InputField>().text = LastData.IP ?? "";
                Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/NicknameInput").gameObject.GetComponent<TMP_InputField>().text = LastData.SlotName ?? "";
                Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/Texts/InfoText").gameObject.GetComponent<TextMeshProUGUI>().text = $"Last run progress: {LastData.Progress.ToString("P1") ?? "Unknown"}";
            }

            Panel.SetActive(true);
        }

        public static void Close()
        {
            Panel.SetActive(false);
        }

        public static void SetInfoText(string text)
        {
            Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/Texts/InfoText").gameObject.GetComponent<TextMeshProUGUI>().text = text;
        }

        private static void ConnectButtonClick(PointerEventData eventData)
        {
            // Do input validity check or whatever.

            ConnectionManager.TryConnect(IP, SlotName, Password);
        }

        private static void CloseButtonClick(PointerEventData eventData)
        {
            Close();
        }
    }
}
