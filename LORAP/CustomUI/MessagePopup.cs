using HarmonyLib;
using LOR_DiceSystem;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace LORAP.CustomUI
{
    internal static class MessagePopup
    {
        private static GameObject Panel;

        private static List<string> messages = new List<string>();

        private static int currentMessage = 0;

        private static List<Tuple<LibraryFloorModel, int, bool>> pageMessages = new List<Tuple<LibraryFloorModel, int, bool>>();

        private static void Init()
        {
            Panel = GameObject.Instantiate(PrefabHelper.GetPrefab("messagepopup", "MessagePopup"));
            Panel.transform.Find("Popup/Buttons/Layout/Confirm").gameObject.AddComponent<CustomSelectable>().MouseClickEvent.AddListener(ConfirmClick);
            Panel.transform.Find("Popup/Buttons/Layout/Skip").gameObject.AddComponent<CustomSelectable>().MouseClickEvent.AddListener(SkipClick);
            Panel.transform.Find("Popup/PopupText").gameObject.GetComponent<TextMeshProUGUI>().font = UIAlarmPopup.instance.transform.Find("[Rect]Normal/[Text]AlarmText").gameObject.GetComponent<TextMeshProUGUI>().font;

            Panel.SetActive(false);
        }

        internal static void ShowMessage(string message)
        {
            if (Panel == null)
                Init();

            messages.Add(message);

            if (!Panel.activeSelf)
            {
                Open();

                SetText(messages[currentMessage]);
            }

            SetMessageNumber(messages.Count - (currentMessage + 1));
        }

        internal static void ShowPages(LibraryFloorModel floor, int level, bool ego = false)
        {
            if (Panel == null)
                Init();

            pageMessages.Add(new Tuple<LibraryFloorModel, int, bool>(floor, level, ego));

            if (!Panel.activeSelf && !UIGetAbnormalityPanel.instance.IsOpened())
            {
                PagesPopup();
            }
        }

        private static void PagesPopup()
        {
            var cur = pageMessages.First();
            pageMessages.RemoveAt(0);

            var floor = cur.Item1;
            var lv = cur.Item2;
            var ego = cur.Item3;

            if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah && floor.Sephirah != SephirahType.None && LibraryModel.Instance.IsOpenedSephirah(floor.Sephirah))
            {
                GameSceneManager.Instance.ActivateUIController();
                UI.UIController.Instance.SetCurrentSephirah(floor.Sephirah);
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
            }

            UIGetAbnormalityPanel panel = UIGetAbnormalityPanel.instance;

            var currentFloor = Traverse.Create(panel).Field<LibraryFloorModel>("currentFloor").Value;
            currentFloor = floor;
            Traverse.Create(panel).Field<GameObject>("ob_blackbgForKeterCompleterOpen").Value.gameObject.SetActive(LibraryModel.Instance.IsKeterCompleteOpen(floor));

            var currentSettinfCardCount = Traverse.Create(panel).Field<int>("currentSettinfCardCount").Value;

            currentSettinfCardCount = -1;
            panel.Open();

            var sep = Traverse.Create(panel).Field<SephirahType>("sep").Value;
            sep = floor.Sephirah;

            Traverse.Create(panel).Field<Image>("img_floorIcon").Value.sprite = UISpriteDataManager.instance._floorIconSet[(int)sep].icon;

            List<EmotionCardXmlInfo> dataListByLevel = Singleton<EmotionCardXmlList>.Instance.GetDataListByLevel(floor.Sephirah, lv+1);
            currentSettinfCardCount = dataListByLevel.Count;

            string id = "";
            switch (sep)
            {
                case SephirahType.None:
                    id = "";
                    break;
                case SephirahType.Malkuth:
                    id = "ui_malkuthfloor";
                    break;
                case SephirahType.Yesod:
                    id = "ui_yesodfloor";
                    break;
                case SephirahType.Hod:
                    id = "ui_hodfloor";
                    break;
                case SephirahType.Netzach:
                    id = "ui_netzachfloor";
                    break;
                case SephirahType.Tiphereth:
                    id = "ui_tipherethfloor";
                    break;
                case SephirahType.Chesed:
                    id = "ui_chesedfloor";
                    break;
                case SephirahType.Gebura:
                    id = "ui_geburafloor";
                    break;
                case SephirahType.Hokma:
                    id = "ui_hokmafloor";
                    break;
                case SephirahType.Binah:
                    id = "ui_binahfloor";
                    break;
                case SephirahType.Keter:
                    id = "ui_keterfloor";
                    break;
            }

            Traverse.Create(panel).Field<TextMeshProUGUI>("txt_floorname").Value.text = TextDataModel.GetText(id);

            string text = "I";
            switch (lv)
            {
                case 1:
                    text = "I";
                    break;
                case 2:
                    text = "II";
                    break;
                case 3:
                    text = "III";
                    break;
                case 4:
                    text = "IV";
                    break;
                case 5:
                    text = "V";
                    break;
                case 6:
                    text = "VI";
                    break;
                case 7:
                    text = "VII";
                    break;
                case 8:
                    text = "VIII";
                    break;
                case 9:
                    text = "IX";
                    break;
                case 10:
                    text = "X";
                    break;
            }

            Traverse.Create(panel).Field<TextMeshProUGUI>("txt_level").Value.text = text;

            panel.SetColor(UIColorManager.Manager.GetSephirahColor(sep));

            if (sep == SephirahType.Binah)
            {
                panel.SetColor(UIColorManager.Manager.GetSephirahGlowColor(sep));
            }

            if (!ego && dataListByLevel.Count > 0)
            {
                Traverse.Create(panel).Field<GameObject>("AbnormalitiesRoot").Value.SetActive(value: true);
                Traverse.Create(panel).Field<GameObject>("EgoCardsRoot").Value.SetActive(value: false);

                Traverse.Create(panel).Field<TextMeshProUGUI>("txt_getabcardtxt").Value.gameObject.SetActive(value: true);
                Traverse.Create(panel).Field<TextMeshProUGUI>("txt_getegocardtxt").Value.gameObject.SetActive(value: false);

                panel.selectablePanel.ChildSelectable = panel.abpanelSelectable;

                var AbnormalityList = Traverse.Create(panel).Field<List<UIEmotionPassiveCardInven>>("AbnormalityList").Value;

                for (int i = 0; i < dataListByLevel.Count; i++)
                {
                    if (i > AbnormalityList.Count)
                    {
                        break;
                    }
                    AbnormalityList[i].Init(dataListByLevel[i]);
                }

                foreach (UIEmotionPassiveCardInven abnormality in AbnormalityList)
                {
                    abnormality.SetActiveDetail(on: true);
                }

                Traverse.Create(panel).Field<bool>("isShowEgo").Value = false;
            }
            else if (ego && Singleton<EmotionEgoXmlList>.Instance.GetEgoCardList(currentFloor.Sephirah).Count > 0)
            {
                Traverse.Create(panel).Field<GameObject>("AbnormalitiesRoot").Value.SetActive(value: false);
                Traverse.Create(panel).Field<GameObject>("EgoCardsRoot").Value.SetActive(value: true);

                Traverse.Create(panel).Field<TextMeshProUGUI>("txt_getabcardtxt").Value.gameObject.SetActive(value: false);
                Traverse.Create(panel).Field<TextMeshProUGUI>("txt_getegocardtxt").Value.gameObject.SetActive(value: true);

                panel.selectablePanel.ChildSelectable = panel.egopanelSelectable;

                List<DiceCardXmlInfo> list = new List<DiceCardXmlInfo> { Singleton<EmotionEgoXmlList>.Instance.GetEgoCardList(floor.Sephirah)[lv - 1] };
                List<DiceCardItemModel> list2 = new List<DiceCardItemModel>();
                for (int i = 0; i < list.Count; i++)
                {
                    list2.Add(new DiceCardItemModel(list[i]));
                }

                Traverse.Create(panel).Field<UIEgoCardList>("egoCardList").Value.SetEgoCards(list2);

                Traverse.Create(panel).Field<bool>("isShowEgo").Value = true;
            }

            typeof(UIGetAbnormalityPanel).GetMethod("SetDefault", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(panel, null);

            Traverse.Create(panel).Field<Animator>("anim").Value.SetTrigger("Reveal");
        }

        internal static void PagesClose()
        {
            UIGetAbnormalityPanel panel = UIGetAbnormalityPanel.instance;

            typeof(UIGetAbnormalityPanel).GetMethod("SetDefault", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(panel, null);

            panel.Close();

            if (pageMessages.Count > 0)
                PagesPopup();
        }

        private static void Open()
        {
            if (Panel == null)
                Init();

            Panel.SetActive(true);

            UISoundManager.instance.PlayEffectSound(UISoundType.Gacha_Hexagon);
            LORAP.Instance.StartCoroutine(RevealAnim());
        }

        private static void Close()
        {
            if (Panel == null)
                Init();

            Panel.SetActive(false);
        }

        private static IEnumerator RevealAnim()
        {
            var canvasGroup = Panel.GetComponent<CanvasGroup>();
            var time = Time.time;

            while (true)
            {
                float cur = Mathf.Min(Mathf.Lerp(0f, 1f, (Time.time - time) / 0.25f), 1f);

                canvasGroup.alpha = cur;

                if (canvasGroup.alpha >= 1f) break;

                yield return new WaitForEndOfFrame();
            }
        }

        private static void SetText(string text)
        {
            Panel.transform.Find("Popup/PopupText").gameObject.GetComponent<TextMeshProUGUI>().text = text;
        }

        private static void SetMessageNumber(int number)
        {
            if (number <= 0)
            {
                Panel.transform.Find("Popup/TotalNumber").gameObject.SetActive(false);
                Panel.transform.Find("Popup/Buttons/Layout/Skip").gameObject.SetActive(false);

                return;
            }

            Panel.transform.Find("Popup/Buttons/Layout/Skip").gameObject.SetActive(true);
            Panel.transform.Find("Popup/TotalNumber").gameObject.SetActive(true);
            Panel.transform.Find("Popup/TotalNumber").gameObject.GetComponent<TextMeshProUGUI>().text = $"{number} More Messages";
        }

        private static void ConfirmClick(PointerEventData eventData)
        {
            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);
            if (currentMessage+1 >= messages.Count)
            {
                messages.Clear();
                currentMessage = 0;
                Close();

                if (pageMessages.Count > 0)
                    PagesPopup();

                return;
            }

            currentMessage++;

            SetText(messages[currentMessage]);
            SetMessageNumber(messages.Count - (currentMessage + 1));
        }

        private static void SkipClick(PointerEventData eventData)
        {
            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);

            messages.Clear();
            currentMessage = 0;
            Close();

            if (pageMessages.Count > 0)
                PagesPopup();
        }
    }
}
