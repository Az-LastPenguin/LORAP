using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using LORAP.Archipelago;
using LORAP.Utils;
using TMPro;
using UnityEngine;

namespace LORAP.CustomUI.Components.ClientTabs
{
    internal class HintsTab : MonoBehaviour, IClientTab
    {
        private GameObject HintPrefab = AssetBundleHelper.GetAsset("APClientHint");

        private GameObject Tab;

        private Transform HintHolder;
        private TMP_InputField InputField;

        public void Awake()
        {
            // Get all the stuff
            Tab = gameObject;

            HintHolder = Tab.transform.Find("ScrollRect/Viewport/Content");
            InputField = Tab.transform.Find("HintInput").GetComponent<TMP_InputField>();

            // Listen for HintEvent to show hints in Hints Tab
            LocationManager.HintEvent += UpdateTab;
        }

        public void Update()
        {
            if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(InputField?.text))
            {
                SessionManager.SayMessage($"!hint {InputField.text}");
                InputField.text = "";
            }

            InputField.ActivateInputField();
        }

        public void UpdateTab()
        {
            ClearHints();

            foreach (Hint hint in LocationManager.KnownHints)
                AddHint(hint);
        }

        public void ResetTab()
        {

        }

        private void ClearHints()
        {
            for (int i = HintHolder.childCount - 1; i >= 0; i--)
                Destroy(HintHolder.GetChild(i).gameObject);
        }

        private void AddHint(Hint hint)
        {
            ItemLocationPair pair = LocationManager.GetLocationPair(hint.LocationId);

            GameObject newHint = Instantiate(HintPrefab, HintHolder);
            Transform holder = newHint.transform.Find("Holder");
            holder.Find("Receiving").GetComponentInChildren<TextMeshProUGUI>().text = SessionManager.GetPlayerName(pair.Receiving);
            holder.Find("Item").GetComponentInChildren<TextMeshProUGUI>().text = pair.Item.Name;
            holder.Find("Finding").GetComponentInChildren<TextMeshProUGUI>().text = SessionManager.GetPlayerName(pair.Finding);
            holder.Find("Location").GetComponentInChildren<TextMeshProUGUI>().text = pair.Location.Name;
            holder.Find("Status").GetComponentInChildren<TextMeshProUGUI>().text = FormatStatus(hint.Status);
        }

        private string FormatStatus(HintStatus status)
        {
            string color = status switch
            {
                HintStatus.Unspecified => "FFFFFF",
                HintStatus.NoPriority => "00EEEE",
                HintStatus.Avoid => "FA8072",
                HintStatus.Priority => "AF99EF",
                HintStatus.Found => "00FF7F",
            };

            return $"<color=#{color}>{status}</color>";
        }
    }
}
