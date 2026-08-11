using LORAP.Archipelago;
using LORAP.Utils;
using System;
using System.Linq;
using TMPro;
using UnityEngine;

namespace LORAP.CustomUI.Components.ClientTabs
{
    internal class SettingsTab : MonoBehaviour, IClientTab
    {
        private GameObject BoolPrefab = AssetBundleHelper.GetAsset("BoolSetting");
        private GameObject EnumPrefab = AssetBundleHelper.GetAsset("EnumSetting");

        private GameObject Tab;

        private Transform SettingHolder;
        private Transform OptionHolder;

        public void Awake()
        {
            // Get all the stuff
            Tab = gameObject;

            SettingHolder = Tab.transform.Find("SettingsRect/Viewport/Content");
            OptionHolder = Tab.transform.Find("OptionsRect/Viewport/Content");
        }

        public void UpdateTab()
        {
            
        }

        private void ClearHolders()
        {
            for (int i = SettingHolder.childCount - 1; i >= 0; i--)
                Destroy(SettingHolder.GetChild(i).gameObject);

            for (int i = OptionHolder.childCount - 1; i >= 0; i--)
                Destroy(OptionHolder.GetChild(i).gameObject);
        }

        public void ResetTab()
        {
            ClearHolders();

            // Populate the holders
            foreach (ISetting setting in SettingsManager.AllSettings.Values)
            {
                // Check if this setting should be changes
                if (setting.SlotDataID != "" && !setting.Overridable)
                    continue;

                Transform thisGoesHere = setting.SlotDataID == "" ? SettingHolder : OptionHolder;

                Type genericType = setting.GetType().GetGenericArguments()[0];

                if (genericType.IsEnum) // EnumSetting
                {
                    GameObject newSetting = Instantiate(EnumPrefab, thisGoesHere);
                    newSetting.transform.Find("Setting/SettingName").GetComponent<TextMeshProUGUI>().text = setting.Name;

                    if (setting.Description != "")
                        newSetting.transform.Find("SettingDescription/Description").GetComponent<TextMeshProUGUI>().text = setting.Description;
                    else
                        newSetting.transform.Find("SettingDescription").gameObject.SetActive(false);

                    TMP_Dropdown dropdown = newSetting.GetComponentInChildren<TMP_Dropdown>();
                    dropdown.ClearOptions();
                    dropdown.AddOptions(Enum.GetNames(genericType).ToList());
                    dropdown.SetValueWithoutNotify(Convert.ToInt32(setting.GetValueAsObj()));
                    dropdown.RefreshShownValue();

                    dropdown.onValueChanged.AddListener(index =>
                    {
                        setting.SetValue(Enum.ToObject(genericType, index));
                    });
                }
                else if (genericType == typeof(bool)) // BoolSetting
                {
                    GameObject newSetting = Instantiate(BoolPrefab, thisGoesHere);
                    newSetting.transform.Find("Setting/SettingName").GetComponent<TextMeshProUGUI>().text = setting.Name;

                    if (setting.Description != "")
                        newSetting.transform.Find("SettingDescription/Description").GetComponent<TextMeshProUGUI>().text = setting.Description;
                    else
                        newSetting.transform.Find("SettingDescription").gameObject.SetActive(false);

                    BoolSettingComponent boolSetting = newSetting.AddComponent<BoolSettingComponent>();
                    boolSetting.SetValue(Convert.ToBoolean(setting.GetValueAsObj()));

                    boolSetting.ChangeEvent.AddListener(value =>
                    {
                        setting.SetValue(value);
                    });
                }
            }
        }
    }
}
