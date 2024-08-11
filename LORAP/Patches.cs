namespace LORAP.Patches
{
    using GameSave;
    using global::LORAP.CustomUI;
    using HarmonyLib;
    using LOR_DiceSystem;
    using StoryScene;
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using TMPro;
    using UI;
    using UI.Title;
    using UnityEngine;
    using UnityEngine.UI;

    [HarmonyPatch(typeof(UIAbnormalityPanel))]
    internal class UIAbnormalityPanelPatches
    {
        [HarmonyPatch(nameof(UIAbnormalityPanel.SetData))]
        [HarmonyPrefix]
        static bool CheckOpenFloorPrefix(UIAbnormalityPanel __instance, LibraryFloorModel floor)
        {
            Traverse.Create(__instance).Field<UIEmotionPassiveCardInven>("previewCard").Value.SetActiveDetail(on: false);
            var AbCategoryPanel = Traverse.Create(__instance).Field<List<UIAbnormalityCategoryPanel>>("AbCategoryPanel").Value;

            int i;
            for (i = 0; i < floor.GetEmotionCardAmount(); i++)
            {
                if (AbCategoryPanel.Count <= i)
                {
                    return false;
                }
                List<EmotionCardXmlInfo> dataListByLevel = Singleton<EmotionCardXmlList>.Instance.GetDataListByLevel(floor.Sephirah, i + 2);
                AbCategoryPanel[i].SetData(dataListByLevel, i, __instance);
            }
            for (; i < AbCategoryPanel.Count; i++)
            {
                AbCategoryPanel[i].SetLock();
            }

            Traverse.Create(__instance).Field<ScrollRect>("scroll").Value.normalizedPosition = new Vector2(0f, 1f);
            __instance.OnChangedScrollView();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIEgoCardPanel))]
    internal class UIEgoCardPanelPatches
    {
        [HarmonyPatch(nameof(UIEgoCardPanel.SetData))]
        [HarmonyPrefix]
        static bool CheckOpenFloorPrefix(UIEgoCardPanel __instance, LibraryFloorModel floor)
        {
            List<DiceCardXmlInfo> egoCardList = Singleton<EmotionEgoXmlList>.Instance.GetEgoCardList(floor.Sephirah);
            var slotList = Traverse.Create(__instance).Field<List<UIEgoCardPreviewSlot>>("slotList").Value;

            int i;
            for (i = 0; i < floor.GetEGOAmount(); i++)
            {
                if (slotList.Count > i)
                {
                    slotList[i].Init(new DiceCardItemModel(egoCardList[i]));
                }
            }
            for (; i < slotList.Count; i++)
            {
                slotList[i].Init(null);
            }

            __instance.HideDetailSlotByInventory();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIFloorPanel))]
    internal class UIFloorPanelPatches
    {
        [HarmonyPatch(nameof(UIFloorPanel.CheckOpenFloor))]
        [HarmonyPrefix]
        static bool CheckOpenFloorPrefix(UIFloorPanel __instance)
        {
            return false;
        }

        [HarmonyPatch(nameof(UIFloorPanel.OnUpdatePhase))]
        [HarmonyPostfix]
        static void OnUpdatePhasePostfix(UIFloorPanel __instance)
        {
            Traverse.Create(__instance).Field<GameObject>("abnormalityEgoTap").Value.SetActive(value: true);
            Traverse.Create(__instance).Field<GameObject>("onlyAbnormalityTap").Value.SetActive(value: false);
        }
    }

    [HarmonyPatch(typeof(UITitleController))]
    internal class UITitleControllerPatches
    {
        [HarmonyPatch("Continue")]
        [HarmonyPrefix]
        static bool Prefix(UITitleController __instance)
        {
            APConnectWindow.Open();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIFloorQuestPanel))]
    internal class UIFloorQuestPanelPatches
    {
        [HarmonyPatch("SetData")]
        [HarmonyPrefix]
        static bool Prefix(UIFloorQuestPanel __instance, LibraryFloorModel floor)
        {
            var questSlotList = Traverse.Create(__instance).Field<UIFloorQuestSlot[]>("questSlotList").Value;

            var setText = (UIFloorQuestSlot slot, string text, bool complete) =>
            {
                slot.SetActiveSlot(on: true);
                Color color = (complete ? UIColorManager.Manager.GetUIColor(UIColor.Disabled) : UIColorManager.Manager.GetUIColor(UIColor.Default));
                slot.SetColor(color);

                var txt_QuestName = Traverse.Create(slot).Field<TextMeshProUGUI>("txt_QuestName").Value;
                txt_QuestName.text = text;
                txt_QuestName.ForceMeshUpdate();

                var img_BgFrame = Traverse.Create(slot).Field<Image>("img_BgFrame").Value;
                img_BgFrame.rectTransform.sizeDelta = new Vector2(txt_QuestName.preferredWidth + 10f, img_BgFrame.rectTransform.sizeDelta.y);

                txt_QuestName.enabled = false;
                txt_QuestName.enabled = true;

                float x = ((txt_QuestName.preferredWidth + 25f > 370f) ? 370f : (txt_QuestName.preferredWidth + 25f));
                img_BgFrame.rectTransform.sizeDelta = new Vector2(x, img_BgFrame.rectTransform.sizeDelta.y);

                Traverse.Create(slot).Field<TextMeshProUGUI>("txt_QuestProgress").Value.text = "";

                var img_Icon = Traverse.Create(slot).Field<Image>("img_Icon").Value;
                img_Icon.sprite = (complete ? UISpriteDataManager.instance._floorQuestStateIcon[1] : UISpriteDataManager.instance._floorQuestStateIcon[0]);
                img_Icon.enabled = true;
                img_Icon.color = (complete ? UIColorManager.Manager._floorQuestSlotIconColor[1] : UIColorManager.Manager._floorQuestSlotIconColor[0]);
            };

            setText(questSlotList[0], $"Emotion cards: Unknown", false);
            setText(questSlotList[1], $"EGO: Unknown", false);
            setText(questSlotList[2], $"Librarian: Unknown", false);
            questSlotList[3].SetActiveSlot(on: false);
            if (floor.Sephirah == SephirahType.Keter)
                setText(questSlotList[3], $"Black Silence: Unknown", false);
            if (floor.Sephirah == SephirahType.Binah)
                setText(questSlotList[3], $"Binah: Unknown", false);
            questSlotList[4].SetActiveSlot(on: false);

            return false;
        }
    }

    [HarmonyPatch(typeof(UITitleController), nameof(UITitleController.OnSelectButton))]
    internal class UiTitlePatch
    {
        static void Postfix(UITitleController __instance)
        {
            foreach (var b in __instance.TitleButtons)
            {
                if (b.type == TitleActionType.New_Game)
                {
                    b.SetState(ButtonState.Disabled);
                }

                if (b.type == TitleActionType.Continue)
                {
                    b.gameObject.GetComponentInChildren<TextMeshProUGUI>().text = "Archipelago Connect";
                }
            }
        }
    }

    [HarmonyPatch(typeof(StaticDataLoader))]
    internal class StaticDataLoaderPatches
    {
        [HarmonyPatch("LoadEmotionCard")]
        [HarmonyPrefix]
        static bool LoadEmotionCardPrefix(StaticDataLoader __instance)
        {
            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            var LoadNewEmotionCard = typeof(StaticDataLoader).GetMethod("LoadNewEmotionCard", BindingFlags.NonPublic | BindingFlags.Instance);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_keter" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_malkuth" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_yesod" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_hod" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_netzach" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_tiphereth" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_geburah" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_chesed" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_binah" }) as EmotionCardXmlRoot).emotionCardXmlList);
            list.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_hokma" }) as EmotionCardXmlRoot).emotionCardXmlList);

            List<EmotionCardXmlInfo> shuffledList = new List<EmotionCardXmlInfo>();
            var Random = new System.Random(LORAP.Seed);
            List<SephirahType> sephirahs = new List<SephirahType>() {SephirahType.Keter, SephirahType.Malkuth, SephirahType.Yesod, SephirahType.Hod, SephirahType.Netzach, SephirahType.Tiphereth, SephirahType.Gebura, SephirahType.Chesed, SephirahType.Binah, SephirahType.Hokma};
            foreach (var seph in sephirahs)
            {
                for (int i = 2; i < 7; i++)
                {
                    for (int j = 0; j < 3; j++)
                    {
                        var rng = Random.Next(list.Count);
                        var card = list.ElementAt(rng);
                        list.RemoveAt(rng);

                        card.Sephirah = seph;
                        card.Level = i;
                        card.EmotionLevel = Random.Next(1, 4);
                        shuffledList.Add(card);
                    }
                }
            }

            shuffledList.AddRange((LoadNewEmotionCard.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionCard_enemy" }) as EmotionCardXmlRoot).emotionCardXmlList);

            Singleton<EmotionCardXmlList>.Instance.Init(shuffledList);

            return false;
        }

        [HarmonyPatch("LoadEmotionEgo")]
        [HarmonyPrefix]
        static bool LoadEmotionEgoPrefix(StaticDataLoader __instance)
        {
            List<EmotionEgoXmlInfo> list = new List<EmotionEgoXmlInfo>();
            var LoadNewEmotionEgo = typeof(StaticDataLoader).GetMethod("LoadNewEmotionEgo", BindingFlags.NonPublic | BindingFlags.Instance);
            list.AddRange((LoadNewEmotionEgo.Invoke(__instance, new object[] { "Xml/Card/EmotionCard/EmotionEgo" }) as EmotionEgoXmlRoot).egoXmlList);

            List<EmotionEgoXmlInfo> shuffledList = new List<EmotionEgoXmlInfo>();
            var Random = new System.Random(LORAP.Seed);
            List<SephirahType> sephirahs = new List<SephirahType>() { SephirahType.Keter, SephirahType.Malkuth, SephirahType.Yesod, SephirahType.Hod, SephirahType.Netzach, SephirahType.Tiphereth, SephirahType.Gebura, SephirahType.Chesed, SephirahType.Binah, SephirahType.Hokma };
            foreach (var seph in sephirahs)
            {
                for (int i = 0; i < 5; i++)
                {
                    var rng = Random.Next(list.Count);
                    var card = list.ElementAt(rng);
                    list.RemoveAt(rng);

                    card.Sephirah = seph;
                    shuffledList.Add(card);
                }
            }

            Singleton<EmotionEgoXmlList>.Instance.Init(shuffledList);

            return false;
        }
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class LibraryModelPatches
    {
        [HarmonyPatch(nameof(LibraryModel.IsClearRats))]
        [HarmonyPrefix]
        static bool IsClearRatsPrefix(LibraryModel __instance, ref bool __result)
        {
            __result = true;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsClearYuns))]
        [HarmonyPrefix]
        static bool IsClearYunsPrefix(LibraryModel __instance, ref bool __result)
        {
            __result = true;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.GetEpNumberTalkStory))]
        [HarmonyPrefix]
        static bool GetEpNumberTalkStoryPrefix(LibraryModel __instance, ref int __result)
        {
            __result = 0;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.CanLevelUpSephirah))]
        [HarmonyPrefix]
        static bool CanLevelUpSephirahPrefix(LibraryModel __instance, SephirahType sep, ref bool __result)
        {
            if (!Traverse.Create(__instance).Field<HashSet<SephirahType>>("_openedSephirah").Value.Contains(sep))
            {
                __result = false;
                return false;
            }

            if (APPlaythruManager.AbnoProgress[sep] < 6)
            {
                __result = true;
            }

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.CheckCreatureBossBattle))]
        [HarmonyPrefix]
        static bool CheckCreatureBossBattlePrefix(LibraryModel __instance, LibraryFloorModel floor, ref bool __result)
        {
            __result = false;

            switch (floor.Sephirah)
            {
                case SephirahType.Keter: // Do something about keter realization
                    __result = false;
                    break;
                case SephirahType.Malkuth:
                case SephirahType.Yesod:
                case SephirahType.Hod:
                case SephirahType.Netzach:
                case SephirahType.Tiphereth:
                case SephirahType.Gebura:
                case SephirahType.Chesed:
                    if (APPlaythruManager.AbnoProgress[floor.Sephirah] == 5)
                    {
                        __result = true;
                    }
                    break;
                case SephirahType.Binah:
                case SephirahType.Hokma:
                    if (floor.Level == 4)
                    {
                        __result = true;
                    }
                    break;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(LibraryFloorModel))]
    internal class LibraryFloorModelPatches
    {
        [HarmonyPatch(nameof(LibraryFloorModel.UpdateOpenedCount), typeof(int))]
        [HarmonyPrefix]
        static bool UpdateOpenedCountPrefix(LibraryFloorModel __instance)
        {
            int res = 1;
            if (Traverse.Create(__instance).Field<int>("_opendUnitCount").Value != 0)
            {
                res = Traverse.Create(__instance).Field<int>("_opendUnitCount").Value;
            }
            Traverse.Create(__instance).Field<int>("_opendUnitCount").Value = res;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIBookPanel))]
    internal class UIBookPanelPatches
    {
        [HarmonyPatch("FeedBookTargetSephirah")]
        [HarmonyPrefix]
        static bool FeedBookTargetSephirahPrefix(UIBookPanel __instance, SephirahType sep)
        {
            // UIGachaResultPopup
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
            Traverse.Create(UIGachaResultPopup.Instance).Field<TextMeshProUGUI>("txt_floorName").Value.text = TextDataModel.GetText(id);
            Traverse.Create(UIGachaResultPopup.Instance).Field<TextMeshProUGUI>("txt_floorName").Value.color = UIColorManager.Manager.GetSephirahColor(sep);
            TextMeshProMaterialSetter component = Traverse.Create(UIGachaResultPopup.Instance).Field<TextMeshProUGUI>("txt_floorName").Value.GetComponent<TextMeshProMaterialSetter>();
            if (component != null)
            {
                component.underlayColor = UIColorManager.Manager.GetSephirahGlowColor(sep);
            }
            Traverse.Create(UIGachaResultPopup.Instance).Field<Image>("Img_bgFrame").Value.color = UIColorManager.Manager.GetSephirahColor(sep);

            UIGachaResultPopup.Instance.SetDefault();
            UIGachaResultPopup.Instance._currentPage = 0;

            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(sep);
            List<BookDropResult> list = new List<BookDropResult>();

            foreach (LorId currentAddedBookId in Traverse.Create(__instance).Field<List<LorId>>("_currentAddedBookIdList").Value)
            {
                var book = Singleton<DropBookXmlList>.Instance.GetData(currentAddedBookId);

                switch (currentAddedBookId.id)
                {
                    case 123456:
                        book = APPlaythruManager.ObjetdartBook;
                        BookDropResult result = new BookDropResult();

                        var rng = RandomUtil.SelectOne(book.DropItemList.Where(p => Singleton<BookInventoryModel>.Instance.GetBookCount(p.id) == 0).ToList());
                        result.id = rng.id;
                        result.bookInstanceId = Singleton<BookInventoryModel>.Instance.CreateBook(new LorId(rng.id.id)).instanceId;
                        result.itemType = DropItemType.Equip;
                        result.number = 1;
                        list.Add(result);
                        break;
                    default:
                        result = new BookDropResult();
                        result.bookInstanceId = 161616;
                        result.number = book.id.id;
                        list.Add(result);
                        break;
                }

                Singleton<DropBookInventoryModel>.Instance.RemoveBook(currentAddedBookId);
            }

            UIGachaEffect.instance.StartGachaProcess(sep, floor.Level);

            Traverse.Create(UIGachaResultPopup.Instance).Field<List<BookDropResult>>("DropList").Value = list;

            UIGachaResultPopup.Instance.UpdateGachaList();

            LibraryModel.Instance.CheckAllCards();
            LibraryModel.Instance.CheckAllEquips();
            __instance.SetAddedBookClear();
            __instance.OnUpdatePhase();
            Singleton<SaveManager>.Instance.SavePlayData(1);

            return false;
        }
    }

    [HarmonyPatch(typeof(UIGachaResultPopup))]
    internal class UIGachaResultPopupPatches
    {
        [HarmonyPatch(nameof(UIGachaResultPopup.UpdateGachaList))]
        [HarmonyPrefix]
        static bool UpdateGachaListPrefix(UIGachaResultPopup __instance)
        {
            __instance.InitPopup();
            typeof(UIGachaResultPopup).GetMethod("UpdatePageButtons", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);
            var _currentViewDropList = Traverse.Create(__instance).Field<List<BookDropResult>>("_currentViewDropList").Value;
            _currentViewDropList = new List<BookDropResult>();
            var _gachaSlotList = Traverse.Create(__instance).Field<List<UIGachaSlot>>("_gachaSlotList").Value;
            var DropList = Traverse.Create(__instance).Field<List<BookDropResult>>("DropList").Value;
            for (int i = _gachaSlotList.Count * __instance._currentPage; i < _gachaSlotList.Count * (__instance._currentPage + 1); i++)
            {
                if (i < DropList.Count)
                {
                    _currentViewDropList.Add(DropList[i]);
                }
            }
            for (int j = 0; j < _gachaSlotList.Count; j++)
            {
                if (j < _currentViewDropList.Count)
                {
                    BookDropResult bookDropResult = _currentViewDropList[j];

                    if (bookDropResult.bookInstanceId == 161616)
                    {
                        var slot = _gachaSlotList[j];
                        slot.isCard = false;
                        slot.equipSlot.SetActiveSlot(true);
                        slot.cardSlot.SetActiveSlot(false);
                        slot.equipSlot.BookName.text = "Archipelago Check";
                        slot.equipSlot.index = j;
                        slot.equipSlot._book = null;
                        slot.equipSlot.Icon.sprite = UISpriteDataManager.instance?.GetStoryIcon("Chapter1").icon;
                        slot.equipSlot.IconGlow.sprite = UISpriteDataManager.instance?.GetStoryIcon("Chapter1").iconGlow;
                        Color uIColor = UIColorManager.Manager.GetUIColor(UIColor.Default);
                        Color equipRarityColor = UIColorManager.Manager.GetEquipRarityColor(Rarity.Unique);
                        typeof(UIGachaSlot).GetMethod("SetColor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(slot.equipSlot, new object[] { uIColor });
                        typeof(UIGachaSlot).GetMethod("SetGlowColor", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(slot.equipSlot, new object[] { equipRarityColor });

                        continue;
                    }

                    if (bookDropResult.itemType == DropItemType.Card)
                    {
                        _gachaSlotList[j].SetCard(new DiceCardItemModel(ItemXmlDataList.instance.GetCardItem(bookDropResult.id)), j);
                    }
                    else
                    {
                        _gachaSlotList[j].SetEquip(Singleton<BookInventoryModel>.Instance.GetBookByInstanceId(bookDropResult.bookInstanceId), j);
                    }
                    _gachaSlotList[j].SetActiveSlotAll(b: true);
                }
                else
                {
                    _gachaSlotList[j].SetActiveSlotAll(b: false);
                }
            }

            return false;
        }

        [HarmonyPatch(nameof(UIGachaResultPopup.PointerEnterSlot), typeof(UIGachaEquipSlot))]
        [HarmonyPrefix]
        static bool PointerEnterSlotPrefix(UIGachaResultPopup __instance, UIGachaEquipSlot gachaEquip)
        {
            if (gachaEquip._book == null)
                return false;

            return true;
        }
    }

    [HarmonyPatch(typeof(StageClassInfo))]
    internal class StageClassInfoPatches
    {
        [HarmonyPatch(nameof(StageClassInfo.currentState), MethodType.Getter)]
        [HarmonyPrefix]
        static bool CurrentStateGetterPrefix(StageClassInfo __instance, ref StoryState __result)
        {
            __result = StoryState.Close;

            if ((__instance.chapter == 1 || __instance.chapter == 2) && __instance.storyType != "Chapter2")
                __result = StoryState.Clear;

            if (APPlaythruManager.GetReceptionOpened(__instance.id.id))
                __result = StoryState.Clear;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIStoryProgressPanel))]
    internal class UIStoryProgressPanelPatches
    {
        [HarmonyPatch("SetStoryLine")]
        [HarmonyPrefix]
        static bool SetStoryLinePrefix(UIStoryProgressPanel __instance)
        {
            __instance.currentSlot = null;
            StoryTotal.instance.SetData();
            foreach (GameObject chapter in Traverse.Create(__instance).Field<List<GameObject>>("chapterList").Value)
            {
                chapter.SetActive(true);
            }

            foreach (var block in Traverse.Create(__instance).Field<List<UIBlockChapterAlarm>>("blockChapterList").Value)
            {
                block.root.gameObject.SetActive(false);
            }

            foreach (var icon in Traverse.Create(__instance).Field<List<UIStoryProgressIconSlot>>("iconList").Value)
            {
                StoryLineData storyLineData = StoryTotal.instance._lineList.Find((StoryLineData x) => x.currentstory == icon.currentStory);
                List<StageClassInfo> story = storyLineData != null ? storyLineData.stageList : Traverse.Create(icon).Field<List<StageClassInfo>>("storyData").Value;
                icon.SetSlotData(story);
                icon.SetActiveStory(true);
                if (story[0].chapter == 7 && story[0].id != 60001)
                {
                    icon.SetActiveStory(false);
                }
            }

            foreach (UIStoryProgressIconSlot chapterIcon in Traverse.Create(__instance).Field<List<UIStoryProgressIconSlot>>("chapterIconList").Value)
            {
                chapterIcon.SetChapterStoryIcon();
                chapterIcon.SetChapterStoryIconDefault();
            }

            foreach (var c in Traverse.Create(__instance).Field("chapterIconList").GetValue() as List<UIStoryProgressIconSlot>)
            {
                Traverse.Create(c).Field("isDisabled").SetValue(true);
                c.GetComponentInChildren<UICustomSelectable>().interactable = false;
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(UIController))]
    internal class UIControllerPatches
    {
        [HarmonyPatch("OpenStory", typeof(StageStoryInfo), typeof(StoryRoot.OnEndStoryFunc), typeof(bool), typeof(bool), typeof(bool))]
        [HarmonyPrefix]
        static bool OpenStoryPrefix(UIController __instance, StoryRoot.OnEndStoryFunc endFunc)
        {
            endFunc();

            return false;
        }

        [HarmonyPatch(nameof(UIController.OnClickStartCreatureStage))]
        [HarmonyPrefix]
        static bool OnClickStartCreatureStagePrefix(UIController __instance, SephirahType targetSephirah)
        {
            FloorLevelXmlInfo data = Singleton<FloorLevelXmlList>.Instance.GetData(targetSephirah, APPlaythruManager.AbnoProgress[targetSephirah]);
            if (data == null)
            {
                return false;
            }
            Singleton<StageController>.Instance.SetCurrentSephirah(targetSephirah);
            StageClassInfo data2 = Singleton<StageClassInfoList>.Instance.GetData(data.stageId);
            if (data2 == null)
            {
                return false;
            }
            Singleton<StageController>.Instance.InitStageByCreature(data2);
            foreach (UnitBattleDataModel unitBattleData in Singleton<StageController>.Instance.GetCurrentStageFloorModel().GetUnitBattleDataList())
            {
                if (targetSephirah == SephirahType.Binah && LibraryModel.Instance.IsBinahLockedInStage(data2) && unitBattleData.unitData.isSephirah)
                {
                    unitBattleData.IsAddedBattle = false;
                }
                else
                {
                    unitBattleData.IsAddedBattle = true;
                }
            }

            GlobalGameManager.Instance.LoadBattleScene();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIInvitationInfoPanel))]
    internal class UIInvitationInfoPanelPatches
    {
        [HarmonyPatch("Initialized")]
        [HarmonyPostfix]
        static void InitializedPostfix(UIInvitationInfoPanel __instance)
        {
            __instance.transform.Find("[Script]EnemyStageInfoPanel/[Root]ShowStoryPanel").gameObject.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(UILibrarySliderPanel))]
    internal class UILibrarySliderPanelPatches
    {
        [HarmonyPatch(nameof(UILibrarySliderPanel.SetData))]
        [HarmonyPrefix]
        static bool SetDataPrefix(UILibrarySliderPanel __instance)
        {
            LibraryModel instance = LibraryModel.Instance;
            int chapter = instance.GetChapter();
            Traverse.Create(__instance).Field<Image>("img_CityIcon").Value.sprite = UISpriteDataManager.instance._bookGradeFilterIcon[chapter - 1].icon;
            Traverse.Create(__instance).Field<Image>("img_CityIconGlow").Value.sprite = UISpriteDataManager.instance._bookGradeFilterIcon[chapter - 1].iconGlow;

            float num = LORAP.Instance.GetFoundLocationAmount();
            float num2 = LORAP.Instance.GetTotalLocationAmount();
            float sliderLength = Traverse.Create(__instance).Field<float>("sliderLength").Value;
            float x = sliderLength * (num / num2);

            var txt_leveltxt = Traverse.Create(__instance).Field<TextMeshProUGUI>("txt_leveltxt").Value;
            var img_SliderMaskGauge = Traverse.Create(__instance).Field<Image>("img_SliderMaskGauge").Value;

            if (num >= num2)
            {
                txt_leveltxt.text = "MAX";
                img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(sliderLength, img_SliderMaskGauge.rectTransform.sizeDelta.y);
                return false;
            }

            txt_leveltxt.text = "Lv" + num + "/Lv" + num2;
            img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(x, img_SliderMaskGauge.rectTransform.sizeDelta.y);

            return false;
        }
    }
    

    [HarmonyPatch(typeof(UITitlePanel))]
    internal class UITitlePanelPatches
    {
        [HarmonyPatch("SetMainTitle")]
        [HarmonyPrefix]
        static bool SetMainTitlePrefix(UITitlePanel __instance, string key)
        {
            Traverse.Create(__instance).Field<TextMeshProUGUI>("mainTitleText").Value.GetComponent<TextMeshProMaterialSetter>().underlayColor = new Color(0f, 0.89453125f, 0.46484375f);
            Traverse.Create(__instance).Field<CanvasGroup>("mainTitle").Value.alpha = 1f;
            Traverse.Create(__instance).Field<CanvasGroup>("subTitle").Value.alpha = 0f;
            Traverse.Create(__instance).Field<TextMeshProUGUI>("mainTitleText").Value.text = "Archipelago Progress";
            Traverse.Create(__instance).Field<UILibrarySliderPanel>("sliderPanel").Value.SetData();

            return false;
        }
    }

    [HarmonyPatch(typeof(DropBookXmlInfo))]
    internal class DropBookXmlInfoPatches
    {
        [HarmonyPatch("Name", MethodType.Getter)]
        [HarmonyPrefix]
        static bool NamePrefix(DropBookXmlInfo __instance, ref string __result)
        {
            if (__instance._id == 123456)
            {
                __result = "Objet d'art Page";
                return false;
            } 

            return true;
        }
    }

    [HarmonyPatch(typeof(UIShowUsingBookInfoPanel))]
    internal class UIShowUsingBookInfoPanelPatches
    {
        [HarmonyPatch(nameof(UIShowUsingBookInfoPanel.ShowBookInfoData))]
        [HarmonyPrefix]
        static bool ShowBookInfoDataPrefix(UIShowUsingBookInfoPanel __instance, DropBookXmlInfo dropBookInfo)
        {
            __instance.gameObject.SetActive(value: true);
            Traverse.Create(__instance).Field("xmlinfo").SetValue(dropBookInfo);
            __instance.SetActivePanel(show: true);
            var xmlinfo = Traverse.Create(__instance).Field<DropBookXmlInfo>("xmlinfo").Value;
            if (xmlinfo == null)
            {
                return false;
            }
            Traverse.Create(__instance).Field<UIBookSlot>("currentDropBookSlot").Value.SetData_DropBook(xmlinfo.id);
            Traverse.Create(__instance).Field<TextMeshProUGUI>("txt_bookName").Value.text = xmlinfo.Name;
            List<UIRewardBookData> list = new List<UIRewardBookData>();
            List<UIRewardCardData> list2 = new List<UIRewardCardData>();

            BookXmlInfo fakeInfo = new BookXmlInfo();
            fakeInfo._id = 727272;
            fakeInfo.workshopID = "Archipelago";
            switch (dropBookInfo.id.id)
            {
                case 123456:
                    fakeInfo.InnerName = "Random Objet d'art Key Page";
                    fakeInfo.Rarity = Rarity.Unique;
                    break;
                default:
                    fakeInfo.InnerName = "Archipelago Check";
                    fakeInfo.Rarity = Rarity.Unique;
                    break;
            }

            list.Add(new UIRewardBookData(fakeInfo, 0, 0));
            Traverse.Create(__instance).Field<UIRewardItemList>("rewardItemList").Value.SetItemsData(list, list2);
            __instance.SetColor(UIColorManager.Manager.GetUIColor(UIColor.Default));
            Traverse.Create(__instance).Field<Image>("img_BookIcon").Value.color = Color.white;
            Traverse.Create(__instance).Field<Image>("img_BookIcon").Value.sprite = xmlinfo.bookIcon;
            Traverse.Create(__instance).Field<Image>("img_BookIconGlow").Value.sprite = xmlinfo.bookIconGlow;
            Traverse.Create(__instance).Field<UICustomGraphicObject>("button_rewardResetButton").Value.interactable = false;

            return false;
        }
    }

    [HarmonyPatch(typeof(UIMainPanel))]
    internal class UIMainPanelPatches
    {
        [HarmonyPatch(nameof(UIMainPanel.OnClickLevelUp))]
        [HarmonyPrefix]
        static bool OnClickLevelUpPrefix(UIMainPanel __instance, int index)
        {
            SephirahType targetSephirah = (SephirahType)(index + 1);
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(targetSephirah);
            FloorLevelXmlInfo data = Singleton<FloorLevelXmlList>.Instance.GetData(targetSephirah, APPlaythruManager.AbnoProgress[targetSephirah]);
            string param = string.Empty;
            UIAlarmType alarmtype = UIAlarmType.StartCreatureBattle;

            if (LibraryModel.Instance.CheckCreatureBossBattle(floor))
            {
                string id = "";
                switch (floor.Sephirah)
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
                param = TextDataModel.GetText(id);
                alarmtype = UIAlarmType.StartCreatureBattleInBoss;
            }
            else if (data != null)
            {
                StageClassInfo data2 = Singleton<StageClassInfoList>.Instance.GetData(data.stageId);
                if (data2 != null)
                {
                    param = Singleton<StageNameXmlList>.Instance.GetName(data2);
                }
            }

            UIAlarmPopup.instance.SetAlarmText(alarmtype, UIAlarmButtonType.YesNo, delegate (bool b)
            {
                if (LibraryModel.Instance.PlayHistory.Start_TheBlueReverberationPrimaryBattle != 1 && b)
                {
                    UIController.Instance.SetCurrentSephirah(targetSephirah);
                    if (targetSephirah == SephirahType.Keter && LibraryModel.Instance.GetFloor(targetSephirah).Level == 5)
                    {
                        int keterCompleteOpenPhase = LibraryModel.Instance.GetKeterCompleteOpenPhase();
                        switch (keterCompleteOpenPhase)
                        {
                            case 1:
                                UIAlarmPopup.instance.StartEndContentsStage(EndContentsStageId.KeterCompleteOpen);
                                break;
                            case 2:
                                UIAlarmPopup.instance.StartEndContentsStage(EndContentsStageId.KeterCompleteOpen2, showstory: false);
                                break;
                            case 3:
                                UIAlarmPopup.instance.StartEndContentsStage(EndContentsStageId.KeterCompleteOpen3);
                                break;
                            case 4:
                                UIAlarmPopup.instance.StartEndContentsStage(EndContentsStageId.KeterCompleteOpen4);
                                break;
                            case 5:
                                UIAlarmPopup.instance.StartEndContentsStage(EndContentsStageId.KeterCompleteOpen5);
                                break;
                            default:
                                Debug.LogError("Phase Count Error, Phase : " + keterCompleteOpenPhase);
                                break;
                        }
                    }
                    else
                    {
                        UIController.Instance.OnClickStartCreatureStage(targetSephirah);
                    }
                }
            }, param);

            return false;
        }
    }

    [HarmonyPatch(typeof(UIInvitationRightMainPanel))]
    internal class UIInvitationRightMainpanelPatches
    {
        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SetInvBookApplyState))]
        [HarmonyPrefix]
        static bool SetInvBookApplyStatePrefix(UIInvitationRightMainPanel __instance, ref InvitationApply_State state)
        {
            if (state == InvitationApply_State.Normal || state == InvitationApply_State.Fixed)
            {
                __instance.currentinvState = state;
                __instance.SetActiveEndEffect(on: false);

                foreach (var slot in __instance.invitationbookSlots)
                {
                    slot.SetDisabledSlot();
                }

                typeof(UIInvitationRightMainPanel).GetMethod("SetUpdatePanel", BindingFlags.NonPublic | BindingFlags.Instance).Invoke(__instance, null);

                return false;
            }

            return true;
        }

        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SetSendButton))]
        [HarmonyPrefix]
        static bool SetSendButtonPrefix(UIInvitationRightMainPanel __instance)
        {
            ((GameObject)Traverse.Create(__instance).Field("ob_tutorialSendButtonhighlight").GetValue()).SetActive(value: false);
            SingletonBehavior<UIMainAutoTooltipManager>.Instance.AllCloseTooltip();
            __instance.button_SendButton.gameObject.SetActive(value: true);
            //__instance.txt_SendButton.text = TextDataModel.GetText("ui_invitation_send");
            __instance.confirmAreaRoot.SetActive(value: false);

            if (((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage == null || ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentApplyState == InvitationApply_State.Normal)
            {
                Traverse.Create(__instance).Field("ispossiblesend").SetValue(false);
                ((Animator)Traverse.Create(__instance).Field("ButtonFrameHighlight").GetValue()).enabled = false;
                __instance.button_SendButton.interactable = false;
                __instance.SetColorAllFrames(UIColorManager.Manager.GetUIColor(UIColor.Default));
                __instance.SetColorInvitationSlots(UIColorManager.Manager.GetUIColor(UIColor.Default));
            }
            else
            {
                Traverse.Create(__instance).Field("ispossiblesend").SetValue(true);
                ((Animator)Traverse.Create(__instance).Field("ButtonFrameHighlight").GetValue()).enabled = true;
                __instance.button_SendButton.interactable = true;
                __instance.SetColorAllFrames(__instance.Color_Selectedcolor);
                __instance.SetColorInvitationSlots(__instance.Color_Selectedcolor);
                __instance.confirmAreaRoot.SetActive(value: false);
            }

            return false;
        }

        [HarmonyPatch(nameof(UIInvitationRightMainPanel.GetAppliedBookModel))]
        [HarmonyPrefix]
        static bool GetAppliedBookModelPrefix(UIInvitationRightMainPanel __instance, ref List<DropBookXmlInfo> __result)
        {
            if (((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage == null || ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentApplyState == InvitationApply_State.Normal)
                return true;

            List<DropBookXmlInfo> list = new List<DropBookXmlInfo>();
            foreach (var id in ((UIInvitationPanel)Traverse.Create(__instance).Field("invPanel").GetValue()).CurrentStage.invitationInfo.needsBooks)
            {
                list.Add(Singleton<DropBookXmlList>.Instance.GetData(id));
            }
            __result = list;

            return false;
        }

        [HarmonyPatch(nameof(UIInvitationRightMainPanel.SendInvitation))]
        [HarmonyPrefix]
        static bool Prefix(UIInvitationRightMainPanel __instance)
        {
            if (__instance.GetBookRecipe() != null)
                __instance.confirmAreaRoot.SetActive(value: true);

            return false;
        }
    }

    [HarmonyPatch(typeof(StageLibraryFloorModel))]
    internal class StageLibraryFloorModelPatches
    {
        // Copy of StageLibraryFloorModel.CreateSelectableList, except for few things
        [HarmonyPatch("CreateSelectableList")]
        [HarmonyPrefix]
        static bool CreateSelectableListPrefix(StageLibraryFloorModel __instance, int emotionLevel, ref List<EmotionCardXmlInfo> __result)
        {
            int num = 0;
            int num2 = 0;
            foreach (UnitBattleDataModel unit in Traverse.Create(__instance).Field<List<UnitBattleDataModel>>("_unitList").Value)
            {
                if (unit.IsAddedBattle)
                {
                    num += unit.emotionDetail.totalPositiveCoins.Count;
                    num2 += unit.emotionDetail.totalNegativeCoins.Count;
                }
            }

            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            int num3 = 1;
            num3 = ((emotionLevel <= 2) ? 1 : ((emotionLevel > 4) ? 3 : 2));
            List<EmotionCardXmlInfo> dataList = Singleton<EmotionCardXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor, APPlaythruManager.EmotionCardAmounts[floor.Sephirah]+1, num3);
            foreach (EmotionCardXmlInfo selected in Traverse.Create(__instance).Field<List<EmotionCardXmlInfo>>("_selectedList").Value)
            {
                dataList.Remove(selected);
            }
            int center = 0;
            int num4 = num + num2;
            float num5 = 0.5f;
            if (num4 > 0)
            {
                num5 = (float)(num - num2) / (float)num4;
            }
            float num6 = num5 / ((11f - (float)emotionLevel) / 10f);
            if ((double)Mathf.Abs(num6) < 0.1)
            {
                center = 0;
            }
            else if ((double)Mathf.Abs(num6) < 0.3)
            {
                if (num6 > 0f)
                {
                    center = 1;
                }
                else
                {
                    center = -1;
                }
            }
            else if (num6 > 0f)
            {
                center = 2;
            }
            else
            {
                center = -2;
            }
            dataList.Sort((EmotionCardXmlInfo x, EmotionCardXmlInfo y) => Mathf.Abs(x.EmotionRate - center) - Mathf.Abs(y.EmotionRate - center));
            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            new List<EmotionCardXmlInfo>();
            new List<EmotionCardXmlInfo>();
            new List<EmotionCardXmlInfo>();
            while (dataList.Count > 0 && list.Count < 3)
            {
                int er = Mathf.Abs(dataList[0].EmotionRate - center);
                List<EmotionCardXmlInfo> list2 = dataList.FindAll((EmotionCardXmlInfo x) => Mathf.Abs(x.EmotionRate - center) == er);
                if (list2.Count + list.Count <= 3)
                {
                    list.AddRange(list2);
                    foreach (EmotionCardXmlInfo item2 in list2)
                    {
                        dataList.Remove(item2);
                    }
                    continue;
                }
                int num7 = 3 - list.Count;
                for (int i = 0; i < num7; i++)
                {
                    if (list2.Count == 0)
                    {
                        break;
                    }
                    EmotionCardXmlInfo item = RandomUtil.SelectOne(list2);
                    list2.Remove(item);
                    dataList.Remove(item);
                    list.Add(item);
                }
            }
            __result = list;

            return false;
        }

        // Copy of StageLibraryFloorModel.RandomSelect, except for few things
        [HarmonyPatch("RandomSelect")]
        [HarmonyPrefix]
        static bool RandomSelectPrefix(StageLibraryFloorModel __instance, List<EmotionCardXmlInfo> duplicated, ref EmotionCardXmlInfo __result)
        {
            List<EmotionCardXmlInfo> list = new List<EmotionCardXmlInfo>();
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            list = Singleton<EmotionCardXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor, APPlaythruManager.EmotionCardAmounts[floor.Sephirah]+1, 1);
            foreach (EmotionCardXmlInfo item in duplicated)
            {
                list.Remove(item);
            }
            if (list.Count > 0)
            {
                __result = RandomUtil.SelectOne(list);
            }
            __result = null;

            return false;
        }

        // Copy of StageLibraryFloorModel.RandomSelectEgo, except for few things
        [HarmonyPatch("RandomSelectEgo")]
        [HarmonyPrefix]
        static bool RandomSelectEgoPrefix(StageLibraryFloorModel __instance, List<EmotionEgoXmlInfo> duplicated, ref EmotionEgoXmlInfo __result)
        {
            List<EmotionEgoXmlInfo> list = new List<EmotionEgoXmlInfo>();
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor(Singleton<StageController>.Instance.CurrentFloor);
            foreach (EmotionEgoXmlInfo data in Singleton<EmotionEgoXmlList>.Instance.GetDataList(Singleton<StageController>.Instance.CurrentFloor).GetRange(0, APPlaythruManager.EGOAmounts[floor.Sephirah]))
            {
                if (!data.isLock)
                {
                    list.Add(data);
                }
            }
            foreach (EmotionEgoXmlInfo item in duplicated)
            {
                list.Remove(item);
            }
            if (list.Count > 0)
            {
                __result = RandomUtil.SelectOne(list);
            }
            __result = null;

            return false;
        }
    }
}
