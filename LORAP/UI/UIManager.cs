using LORAP.Utils;
using System.Linq;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static BattleUnitInformationUI_PassiveList;

namespace LORAP.CustomUI
{
    internal static class UIManager
    {
        private static GameObject APChatWindowPrefab = AssetBundleHelper.GetAsset("APChatWindow");
        private static GameObject APConnectWindowPrefab = AssetBundleHelper.GetAsset("APConnectWindow");
        private static GameObject MessagePopupPrefab = AssetBundleHelper.GetAsset("MessagePopup");
        private static GameObject APClienntWindowPrefab = AssetBundleHelper.GetAsset("APClient");

        internal static void Init()
        {
            // Init mod's custom UI
            // APChatWindow (Archipelago Live Feed)
            GameObject.Instantiate(APChatWindowPrefab).AddComponent<APChatWindow>();

            // APConnectWindow (Connect to Archipelago)
            GameObject.Instantiate(APConnectWindowPrefab).AddComponent<APConnectWindow>();

            // MessagePopup (Items received popup & other things)
            GameObject.Instantiate(MessagePopupPrefab).AddComponent<MessagePopup>();

            // AbnoEgoPagePopup (Received Abno/EGO pages)
            UIGetAbnormalityPanel.instance.gameObject.AddComponent<AbnoEgoPagePopup>();

            // APClientWindow (Archipelago Client & LORAP Settings & Debug window)
            GameObject.Instantiate(APClienntWindowPrefab).AddComponent<APClientWindow>();

            // Apply other general UI changes
            ApplyUIChanges();
        }

        private static void ApplyUIChanges()
        {
            Debug.Log("[LORAP] Applying UI Changes");

            // Make Esc menu above everything else
            GameObject.Find("[Canvas][Script]PopupCanvas").GetComponent<Canvas>().sortingOrder = 90;
            GameObject.Find("[Canvas][Script]PopupCanvas/[Script]PopupManager").transform.SetAsLastSibling();
            Canvas newCanvas = GameObject.Find("[Canvas][Script]PopupCanvas/[Script]PopupManager").AddComponent<Canvas>();
            newCanvas.overrideSorting = true;
            newCanvas.sortingOrder = 101;
            newCanvas.gameObject.AddComponent<GraphicRaycaster>();

            // Change UIFloorPanel: Remove floor level (irrelevant in this mod) & move quest info up, also add more info lines
            UIFloorPanel floorPanel = UI.UIController.Instance.GetUIPanel(UIPanelType.FloorInfo) as UIFloorPanel;
            floorPanel.transform.Find("PanelActiveController/[Rect]Info_Panel/[Rect]LevelBg").gameObject.SetActive(false);
            floorPanel.questPanel.transform.localPosition += new Vector3(0, 60f, 0);

            // Hide amount of pages from burning books
            UIBookPanel bookPanel = UI.UIController.Instance.GetUIPanel(UIPanelType.Book) as UIBookPanel;
            foreach (UIRewardEquipPageSlot slot in bookPanel.DropBookInfoPanel.rewardItemList.equipPageSlotList)
            {
                slot.ob_PerAlarm.SetActive(false);
            }
            foreach (UIRewardCardSlot slot in bookPanel.DropBookInfoPanel.rewardItemList.cardSlotList)
            {
                slot.ob_peralarm.SetActive(false);
            }

            // Hide "Reset Rewards" button from the book burning screen
            UIShowUsingBookInfoPanel dropBookPanel = (UI.UIController.Instance.Panels.ElementAt(3) as UIBookPanel).DropBookInfoPanel;
            dropBookPanel.button_rewardResetButton.gameObject.SetActive(false);

            // Add more slots (4 -> 16) for books in the passive succession menu
            UIPassiveSuccessionEquipBookList passiveBookList = UIPassiveSuccessionPopup.Instance.equipBookList;

            // Enable masking for left book panel
            passiveBookList.rect_ViewPort.GetComponent<Mask>().enabled = true;

            // Add slots to left book panel
            for (int i = 0; i < 12; i++)
            {
                GameObject copy = GameObject.Instantiate(passiveBookList.bookslotlist[0].gameObject, passiveBookList.bookslotlist[0].transform.parent);
                passiveBookList.bookslotlist[0].transform.parent.GetComponent<RectTransform>().sizeDelta += new Vector2(0, 28);
                passiveBookList.bookslotlist.Add(copy.GetComponent<UIPassiveSuccessionEquipBookSlot>());
            }

            // Add event to left book slots for scrolling to pass it through to the scrollrect
            ScrollRect bookListRect = passiveBookList.rect_ViewPort.parent.GetComponent<ScrollRect>();
            foreach (var slot in passiveBookList.bookslotlist)
            {
                EventTrigger evt = slot.GetComponentInChildren<EventTrigger>();

                evt.AddCallback(EventTriggerType.Scroll, (data) => { bookListRect.OnScroll((PointerEventData)data); });
            }

            // Add slots to center bool panel
            UIPassiveSuccessionCenterPanel centerBookList = UIPassiveSuccessionPopup.Instance.centerBookListPanel;
            for (int i = 0; i < 12; i++)
            {
                GameObject slot = centerBookList.rect_bookSlotsLayout.GetChild(0).gameObject;
                GameObject.Instantiate(slot, slot.transform.parent);
            }

            // Add more left panel passive slots
            UIPassiveSuccessionList passiveList = UIPassiveSuccessionPopup.Instance.equipPassiveList;
            for (int i = 0; i < 40; i++)
            {
                GameObject slot = passiveList.rect_slotsLayout.transform.GetChild(0).gameObject;
                GameObject.Instantiate(slot, slot.transform.parent);
            }

            // Add slots to library unit info
            UICardPanel cardPanel = UI.UIController.Instance.GetUIPanel(UIPanelType.Page) as UICardPanel;
            UISetInfoSlotListSc charPassiveSlotList = cardPanel.librarianInfoPanel.passiveSlotsPanel;
            for (int i = 0; i < 46; i++)
            {
                GameObject slot = charPassiveSlotList._slotList[0].gameObject;
                GameObject copy = GameObject.Instantiate(slot, slot.transform.parent);
                charPassiveSlotList._slotList.Add(copy.GetComponent<UILibrarianEquipInfoSlot>());
            }

            // Add slots to enemy and library unit passive list in battle
            BattleUnitInformationUI_PassiveList enemyPassive = BattleManagerUI.Instance.ui_unitInformation.passivelistManager;
            for (int i = 0; i < 46; i++) // I HATE PROJECT MOON CODING
            {
                GameObject slot = enemyPassive.passiveSlotList[0].Rect.gameObject;
                GameObject copy = GameObject.Instantiate(slot, slot.transform.parent);

                BattleUnitInformationPassiveSlot ps = new BattleUnitInformationPassiveSlot();
                ps.Rect = copy.GetComponent<RectTransform>();
                ps.txt_PassiveDesc = copy.GetComponentInChildren<TextMeshProUGUI>();
                ps.img_Icon = copy.transform.Find("[Image]Icon").GetComponent<Image>();
                ps.img_IconGlow = copy.transform.Find("[Image]IconGlow").GetComponent<Image>();

                enemyPassive.passiveSlotList.Add(ps);
            }

            BattleUnitInformationUI_PassiveList playerPassive = BattleManagerUI.Instance.ui_unitInformationPlayer.passivelistManager;
            for (int i = 0; i < 46; i++) // I FUCKING HATE IT
            {
                GameObject slot = playerPassive.passiveSlotList[0].Rect.gameObject;
                GameObject copy = GameObject.Instantiate(slot, slot.transform.parent);

                BattleUnitInformationPassiveSlot ps = new BattleUnitInformationPassiveSlot();
                ps.Rect = copy.GetComponent<RectTransform>();
                ps.txt_PassiveDesc = copy.GetComponentInChildren<TextMeshProUGUI>();
                ps.img_Icon = copy.transform.Find("[Image]Icon").GetComponent<Image>();
                ps.img_IconGlow = copy.transform.Find("[Image]IconGlow").GetComponent<Image>();

                playerPassive.passiveSlotList.Add(ps);
            }

            // Make floor icons be not shit
            foreach (var icon in UISpriteDataManager.instance.floorIconSet)
            {
                icon.iconGlow = icon.icon;
            }

            // Add slots for abno pages for unit info in battle (For player only)
            BattleUnitInformationUI unitInfo = BattleManagerUI._instance.ui_unitInformationPlayer;
            GameObject abnoPageSlotBase = unitInfo.AbnormalityCardList[0].gameObject;

            for (int i = 0; i < 10; i++)
            {
                GameObject copy = GameObject.Instantiate(abnoPageSlotBase, abnoPageSlotBase.transform.parent);
                unitInfo.AbnormalityCardList.Add(copy.GetComponent<EmotionPassiveCardUI>());
            }

            // Make each of them a canvas to change the sorting order and make them be displayed on top when hovering
            foreach (EmotionPassiveCardUI slot in unitInfo.AbnormalityCardList)
            {
                slot.gameObject.AddComponent<Canvas>();
                slot.gameObject.AddComponent<GraphicRaycaster>();
            }

            // Make horizontal sort element place abno pages closer
            abnoPageSlotBase.transform.parent.gameObject.GetComponent<HorizontalLayoutGroup>().childControlWidth = true;

            // Hide sort buttons in book burn screen    
            bookPanel.invenFeedBookList.gradeFilter.transform.Find("[Rect]ToggleList").gameObject.SetActive(false);

            // Add hints for AP Client and Chat
            GameObject pcGuide = UI.UIControlManager.Instance.GetTitlePanel().PCGuide.gameObject;
            pcGuide.transform.localPosition = new Vector3(300, 0, 0);

            GameObject apHelp = GameObject.Instantiate(pcGuide.transform.GetChild(0).gameObject, pcGuide.transform);
            apHelp.transform.SetSiblingIndex(0);
            apHelp.transform.Find("Icon").gameObject.SetActive(false);
            apHelp.GetComponent<RectTransform>().sizeDelta = new Vector2(730, 30);

            GameObject textObject = apHelp.transform.Find("Title_TextMesh").gameObject;
            textObject.GetComponent<UITextDataLoader>().enabled = false;
            textObject.GetComponent<RectTransform>().sizeDelta = new Vector2(670, 30);
            textObject.GetComponent<TextMeshProUGUI>().text = "[AP Client: Tab to Toggle]       [AP Feed: F2 to Toggle | Drag to Move | LeftCtrl + Drag to Resize]";
        }
    
        internal static void RunJoinReset()
        {
            APConnectWindow.Instance.Close();

            APChatWindow.Instance.ClearMessages();
            APChatWindow.Instance.Open();

            APClientWindow.Instance.Reset();
        }
    }
}
