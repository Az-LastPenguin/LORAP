using System;
using System.Collections.Generic;
using System.Linq;
using LORAP.Utils;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LORAP.CustomUI
{
    internal class AbnoEgoPagePopup
    {
        private static UIGetAbnormalityPanel Panel => UIGetAbnormalityPanel.instance;

        private static GameObject SkipButton;

        private static List<Tuple<LibraryFloorModel, int, bool>> queue = new List<Tuple<LibraryFloorModel, int, bool>>();

        internal static void Init()
        {
            // Move the Abno and EGO page receive window to other canvas, also center EGO page display
            Panel.transform.SetParent(GameObject.Find("[Canvas][Script]PopupCanvas").transform);
            Panel.transform.localScale = Vector3.one;
            Panel.EgoCardsRoot.transform.Find("[Prefab]DetailEgoCardSlot").gameObject.GetComponent<Canvas>().sortingOrder = 90;
            Panel.EgoCardsRoot.transform.Find("[Layout]CardViewList").localPosition = new Vector3(-90, 17.7f, 0);
            Panel.EgoCardsRoot.transform.Find("[Layout]CardViewList").gameObject.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperCenter;

            // Create "Skip" button by cloning "Confirm" button and reconnecting events
            SkipButton = GameObject.Instantiate(Panel.transform.Find("[Image]ExitFrame").gameObject);
            SkipButton.transform.parent = Panel.transform;

            // Move it move it
            SkipButton.transform.localPosition = new Vector3(250, -392.7f, 0f);

            // Reconnect events
            EventTrigger trigger = SkipButton.GetComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry();
            pointerEnter.eventID = EventTriggerType.PointerEnter;
            pointerEnter.callback.AddListener((data) => PointerOverButton());
            trigger.triggers.Add(pointerEnter);

            EventTrigger.Entry pointerExit = new EventTrigger.Entry();
            pointerExit.eventID = EventTriggerType.PointerExit;
            pointerExit.callback.AddListener((data) => PointerExitButton());
            trigger.triggers.Add(pointerExit);

            EventTrigger.Entry pointerClick = new EventTrigger.Entry();
            pointerClick.eventID = EventTriggerType.PointerClick;
            pointerClick.callback.AddListener((data) => Skip());
            trigger.triggers.Add(pointerClick);

            // Fix CanvasGroup
            SkipButton.GetComponent<CanvasGroup>().alpha = 1;

            // Disable localization
            SkipButton.GetComponentInChildren<UITextDataLoader>().enabled = false;

            // Start disabled
            SkipButton.SetActive(false);
        }

        internal static void ShowPages(LibraryFloorModel floor, int level, bool isEgo = false)
        {
            queue.Add(new Tuple<LibraryFloorModel, int, bool>(floor, level, isEgo));

            // If there is a battle currently or it's about to start, we delay this message.
            if (StageController.Instance.battleState != StageController.BattleState.None)
                return;

            Open();
        }

        internal static void Open()
        {
            UpdatePanel();

            if (Panel.gameObject.activeSelf)
                return;

            NextMessage();
        }

        internal static void NextMessage()
        {
            if (queue.Count <= 0)
            {
                Close();
                return;
            }

            Tuple<LibraryFloorModel, int, bool> message = queue.First();
            queue.RemoveAt(0);

            var floor = message.Item1;
            var seph = floor.Sephirah;
            var level = message.Item2;
            var isEgo = message.Item3;

            List<EmotionCardXmlInfo> AbnoPages = EmotionCardXmlList.Instance.GetDataListByLevel(seph, level + 1);
            List<DiceCardItemModel> EgoPages = new List<DiceCardItemModel> { new DiceCardItemModel(EmotionEgoXmlList.Instance.GetEgoCardList(floor.Sephirah)[level - 1]) };
            Panel.currentSettinfCardCount = AbnoPages.Count;

            Panel.img_floorIcon.sprite = UISpriteDataManager.instance.floorIconSet[(int)seph].icon;
            Panel.txt_floorname.text = seph.FloorName();
            Panel.txt_level.text = level.ToRoman();
            Panel.SetColor(seph == SephirahType.Binah ? UIColorManager.Manager.GetSephirahGlowColor(seph) : UIColorManager.Manager.GetSephirahColor(seph));
            SetColor();

            Panel.AbnormalitiesRoot.SetActive(!isEgo);
            Panel.txt_getabcardtxt.gameObject.SetActive(!isEgo);
            Panel.EgoCardsRoot.SetActive(isEgo);
            Panel.txt_getegocardtxt.gameObject.SetActive(isEgo);
            Panel.selectablePanel.ChildSelectable = isEgo ? Panel.egopanelSelectable : Panel.abpanelSelectable;
            Panel.isShowEgo = isEgo;

            if (!isEgo && AbnoPages.Count > 0)
            {
                var AbnormalityList = Panel.AbnormalityList;

                for (int i = 0; i < AbnoPages.Count; i++)
                {
                    if (i > AbnormalityList.Count)
                        break;

                    AbnormalityList[i].Init(AbnoPages[i]);
                    AbnormalityList[i].SetActiveDetail(false);
                }
            }
            else if (isEgo && EgoPages.Count > 0)
            {
                Panel.egoCardList.SetEgoCards(EgoPages);
            }

            Panel.Open();
            Panel.SetDefault();
            Panel.anim.SetTrigger("Reveal");

            UpdatePanel();
        }

        private static void UpdatePanel()
        {
            // If there are any messages left, show how much and an option to skip
            SkipButton.SetActive(queue.Count > 0);
            SkipButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Skip ({queue.Count})";
        }

        internal static void Close()
        {
            Panel.SetDefault();
            Panel.Close();
        }

        private static void Skip()
        {
            queue.Clear();
            Close();
        }


        // Copied from UIGetAbnormalityPanel specifically for "Skip" Button
        private static void SetColor()
        {
            SkipButton.GetComponent<Image>().color = Panel.OriginalColor;

            Color white = Color.white;
            white.a = 0.3f;
            Color glowColor = Panel.OriginalColor;
            glowColor.a = 0.5f;

            TextMeshProMaterialSetter obj = SkipButton.GetComponentInChildren<TextMeshProMaterialSetter>();

            obj.outlineColor = white;
            obj.glowColor = glowColor;
            obj.glowOn = false;
            obj.glowOn = true;
            obj.independentSetting = true;
            obj.gameObject.SetActive(false);
            obj.gameObject.SetActive(true);
        }

        private static void PointerOverButton()
        {
            TextMeshProMaterialSetter component = SkipButton.GetComponentInChildren<TextMeshProMaterialSetter>();
            Image image = SkipButton.GetComponent<Image>();

            Color Color = UIColorManager.Manager.GetUIColor(UIColor.Highlighted);
            image.color = Color;
            Color glowColor = Color;
            glowColor.a = 0.35f;

            component.glowColor = glowColor;
            component.gameObject.SetActive(false);
            component.gameObject.SetActive(true);
        }

        private static void PointerExitButton()
        {
            TextMeshProMaterialSetter component = SkipButton.GetComponentInChildren<TextMeshProMaterialSetter>();
            Image image = SkipButton.GetComponent<Image>();

            Color Color = Panel.OriginalColor;
            image.color = Color;
            Color glowColor = Color;
            glowColor.a = 0.35f;

            component.glowColor = glowColor;
            component.gameObject.SetActive(false);
            component.gameObject.SetActive(true);
        }
    }
}
