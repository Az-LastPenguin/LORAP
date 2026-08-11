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
    internal class AbnoEgoPagePopup : SingletonBehavior<AbnoEgoPagePopup>
    {
        private UIGetAbnormalityPanel AbnoPanel;

        private GameObject SkipButton;

        private List<Tuple<LibraryFloorModel, int, bool>> queue = new List<Tuple<LibraryFloorModel, int, bool>>();

        internal void Awake()
        {
            AbnoPanel = UIGetAbnormalityPanel.instance;

            // Move the Abno and EGO page receive window to other canvas
            AbnoPanel.transform.SetParent(GameObject.Find("[Canvas][Script]PopupCanvas").transform);
            AbnoPanel.transform.localScale = Vector3.one;

            // Also center EGO page display
            AbnoPanel.EgoCardsRoot.transform.Find("[Prefab]DetailEgoCardSlot").gameObject.GetComponent<Canvas>().sortingOrder = 90;
            AbnoPanel.EgoCardsRoot.transform.Find("[Layout]CardViewList").localPosition = new Vector3(-90, 17.7f, 0);
            AbnoPanel.EgoCardsRoot.transform.Find("[Layout]CardViewList").gameObject.GetComponent<GridLayoutGroup>().childAlignment = TextAnchor.UpperCenter;

            // Create "Skip" button by cloning "Confirm" button
            SkipButton = GameObject.Instantiate(AbnoPanel.transform.Find("[Image]ExitFrame").gameObject);
            SkipButton.transform.parent = AbnoPanel.transform;
            SkipButton.transform.localPosition = new Vector3(250, -392.7f, 0f);

            // Reconnect events for the cloned button
            EventTrigger trigger = SkipButton.GetComponent<EventTrigger>();
            trigger.triggers.Clear();

            EventTrigger.Entry pointerEnter = new EventTrigger.Entry() { eventID = EventTriggerType.PointerEnter };
            pointerEnter.callback.AddListener((_) => SkipPointerOver());
            trigger.triggers.Add(pointerEnter);

            EventTrigger.Entry pointerExit = new EventTrigger.Entry() { eventID = EventTriggerType.PointerExit };
            pointerExit.callback.AddListener((_) => SkipPointerExit());
            trigger.triggers.Add(pointerExit);

            EventTrigger.Entry pointerClick = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
            pointerClick.callback.AddListener((_) => Skip());
            trigger.triggers.Add(pointerClick);

            // Fix its CanvasGroup
            SkipButton.GetComponent<CanvasGroup>().alpha = 1;

            // Disable auto text localization
            SkipButton.GetComponentInChildren<UITextDataLoader>().enabled = false;

            // Start with skip button inactive
            SkipButton.SetActive(false);
        }

        internal void ShowPages(LibraryFloorModel floor, int level, bool isEgo = false)
        {
            queue.Add(new Tuple<LibraryFloorModel, int, bool>(floor, level, isEgo));

            // If there is a battle currently or it's about to start, we delay this message.
            if (StageController.Instance.battleState != StageController.BattleState.None)
                return;

            Open();
        }

        internal void Open()
        {
            UpdatePanel();

            if (AbnoPanel.gameObject.activeSelf)
                return;

            NextMessage();
        }

        internal void NextMessage()
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
            AbnoPanel.currentSettinfCardCount = AbnoPages.Count;

            AbnoPanel.img_floorIcon.sprite = UISpriteDataManager.instance.floorIconSet[(int)seph].icon;
            AbnoPanel.txt_floorname.text = seph.FloorName();
            AbnoPanel.txt_level.text = level.ToRoman();
            AbnoPanel.SetColor(seph == SephirahType.Binah ? UIColorManager.Manager.GetSephirahGlowColor(seph) : UIColorManager.Manager.GetSephirahColor(seph));
            SkipSetColor();

            AbnoPanel.AbnormalitiesRoot.SetActive(!isEgo);
            AbnoPanel.txt_getabcardtxt.gameObject.SetActive(!isEgo);
            AbnoPanel.EgoCardsRoot.SetActive(isEgo);
            AbnoPanel.txt_getegocardtxt.gameObject.SetActive(isEgo);
            AbnoPanel.selectablePanel.ChildSelectable = isEgo ? AbnoPanel.egopanelSelectable : AbnoPanel.abpanelSelectable;
            AbnoPanel.isShowEgo = isEgo;

            if (!isEgo && AbnoPages.Count > 0)
            {
                var AbnormalityList = AbnoPanel.AbnormalityList;

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
                AbnoPanel.egoCardList.SetEgoCards(EgoPages);
            }

            AbnoPanel.Open();
            AbnoPanel.SetDefault();
            AbnoPanel.anim.SetTrigger("Reveal");

            UpdatePanel();
        }

        private void UpdatePanel()
        {
            // If there are any messages left, show how much and an option to skip
            SkipButton.SetActive(queue.Count > 0);
            SkipButton.GetComponentInChildren<TextMeshProUGUI>().text = $"Skip ({queue.Count})";
        }

        internal void Close()
        {
            AbnoPanel.SetDefault();
            AbnoPanel.Close();
        }

        private void Skip()
        {
            queue.Clear();
            Close();
        }


        // Methods stolen from UIGetAbnormalityPanel for the "Skip" Button
        private void SkipSetColor()
        {
            SkipButton.GetComponent<Image>().color = AbnoPanel.OriginalColor;

            Color white = Color.white;
            white.a = 0.3f;
            Color glowColor = AbnoPanel.OriginalColor;
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

        private void SkipPointerOver()
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

        private void SkipPointerExit()
        {
            TextMeshProMaterialSetter component = SkipButton.GetComponentInChildren<TextMeshProMaterialSetter>();
            Image image = SkipButton.GetComponent<Image>();

            Color Color = AbnoPanel.OriginalColor;
            image.color = Color;
            Color glowColor = Color;
            glowColor.a = 0.35f;

            component.glowColor = glowColor;
            component.gameObject.SetActive(false);
            component.gameObject.SetActive(true);
        }
    }
}
