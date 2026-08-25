using LORAP.CustomUI.Components.ClientTabs;
using System.Collections.Generic;
using UnityEngine;

namespace LORAP.CustomUI 
{
    internal class APClientWindow : SingletonBehavior<APClientWindow>
    {
        private GameObject Panel;

        private List<TabButton> TabButtons = new List<TabButton>();

        private List<IClientTab> Tabs = new List<IClientTab>();

        internal void Awake()
        {
            Panel = transform.Find("Window").gameObject;

            // Get all Tab button objects
            Transform TabButtonTransform = Panel.transform.Find("Frame/Buttons");
            TabButtons.Add(TabButtonTransform.Find("Archipelago").gameObject.AddComponent<TabButton>());
            TabButtons.Add(TabButtonTransform.Find("Hints").gameObject.AddComponent<TabButton>());
            TabButtons.Add(TabButtonTransform.Find("Settings").gameObject.AddComponent<TabButton>());
            TabButtons.Add(TabButtonTransform.Find("Debug").gameObject.AddComponent<TabButton>());

            // Connect click event for every tab button
            foreach (TabButton button in TabButtons)
                button.MouseClickEvent.AddListener((_) => SetTab(button.name));

            // Get all the tabs
            Transform TabTransform = Panel.transform.Find("Frame/Tabs");
            Tabs.Add(TabTransform.Find("Archipelago").gameObject.AddComponent<ArchipelagoTab>());
            Tabs.Add(TabTransform.Find("Hints").gameObject.AddComponent<HintsTab>());
            Tabs.Add(TabTransform.Find("Settings").gameObject.AddComponent<SettingsTab>());
            Tabs.Add(TabTransform.Find("Debug").gameObject.AddComponent<DebugTab>());

            // Start inactive
            Panel.SetActive(false);
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.F2))
            {
                if (Panel.activeSelf)
                    Close();
                else
                    Open();
            }
        }

        internal void Reset()
        {
            Open();

            foreach (IClientTab tab in Tabs)
            {
                tab.gameObject.SetActive(true);
                tab.ResetTab();
            }

            SetTab("Archipelago");

            Close();
        }

        internal void Open()
        {
            Panel.SetActive(true);
        }

        internal void Close()
        {
            Panel.SetActive(false);
        }

        private void SetTab(string tabName)
        {
            foreach (TabButton tabButton in TabButtons)
            {
                tabButton.GetComponent<TabButton>().SetSelected(tabButton.name == tabName);
            }

            foreach (IClientTab tab in Tabs)
            {
                tab.gameObject.SetActive(tab.gameObject.name == tabName);

                if (tab.gameObject.name == tabName)
                    tab.UpdateTab();
            }
        }
    }
}
