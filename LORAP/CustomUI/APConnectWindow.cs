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

        private static void Init()
        {
            Panel = Object.Instantiate(PrefabHelper.GetPrefab("archipelagoconnect", "APJoinCanvas"));
            Panel.transform.Find("APJoinPanel/Window/ApJoin/ButtonLayout/Connect").gameObject.AddComponent<CustomSelectable>().MouseClickEvent.AddListener(ConnectButtonClick);
            Panel.transform.Find("APJoinPanel/Window/ApJoin/ButtonLayout/Cancel").gameObject.AddComponent<CustomSelectable>().MouseClickEvent.AddListener(CloseButtonClick);

            Panel.SetActive(false);
        }

        public static void Open()
        {
            if (Panel == null)
                Init();

            Panel.SetActive(true);
        }

        public static void Close()
        {
            if (Panel == null)
                Init();

            Panel.SetActive(false);
        }

        public static void SetInfoText(string text)
        {
            Panel.transform.Find("APJoinPanel/Window/ApJoin/[Layout]PanelLayout/CenterPanel/Texts/InfoText").gameObject.GetComponent<TextMeshProUGUI>().text = text;
        }

        private static void ConnectButtonClick(PointerEventData eventData)
        {
            // Do input validity check or whatever.

            LORAP.Instance.TryAPConnect(IP, SlotName, Password);
        }

        private static void CloseButtonClick(PointerEventData eventData)
        {
            Close();
        }
    }
}
