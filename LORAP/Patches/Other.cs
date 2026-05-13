using GameSave;
using HarmonyLib;
using LOR_DiceSystem;
using LORAP.Archipelago;
using LORAP.CustomUI;
using LORAP.Gameplay;
using LORAP.Playthru;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using UI;
using UnityEngine;
using UnityEngine.UI;
using static UI.UIMainPanel;

namespace LORAP.Patches
{
    internal class OtherPatches
    {
        // Change Quest info to hints
        [HarmonyPatch(typeof(UIFloorQuestPanel), nameof(UIFloorQuestPanel.SetData))]
        [HarmonyPrefix]
        static bool QuestToHints(UIFloorQuestPanel __instance, LibraryFloorModel floor) // Honestly, this is a mess. Don't think i can do anything about it currently. And don't wanna yet.
        {
            UIFloorQuestSlot[] questSlotList = __instance.questSlotList;
            SephirahType seph = floor.Sephirah;
            FloorInfo floorInfo = PlaythruManager.Floors[seph];

            // Parse all the things we need to put into the list (Use lock icon for upgrades and base exclamation mark for book requirements)
            Sprite lockIcon = UISpriteDataManager.instance._floorCurrentStateIcon[0];
            Sprite exclamMarkIcon = UISpriteDataManager.instance._floorQuestStateIcon[0];
            // Quest Info, Quest Progress, Complete, Sprite
            List<Tuple<string, string, bool, Sprite>> infos = new List<Tuple<string, string, bool, Sprite>>();

            // Abno Pages
            List<ItemLocationPair> pairs = LocationManager.GetPairsWithItemAndHint(ItemManager.GetItemId((int)seph, APItemType.AbnoPages), true);
            infos.Add(new Tuple<string, string, bool, Sprite>(
                $"Abno Pages: {(pairs.Count > 0 ? LocationManager.FormatPairLocation(pairs.First()) : "No Hints")}",
                $"{floorInfo.AbnoPages}/5",
                floorInfo.AbnoPages >= 5,
                lockIcon
            ));

            // EGO Pages
            pairs = LocationManager.GetPairsWithItemAndHint(ItemManager.GetItemId((int)seph, APItemType.EgoPage), true);
            infos.Add(new Tuple<string, string, bool, Sprite>(
                $"EGO Page: {(pairs.Count > 0 ? LocationManager.FormatPairLocation(pairs.First()) : "No Hints")}",
                $"{floorInfo.EGO}/5",
                floorInfo.EGO >= 5,
                lockIcon
            ));

            // Librarians
            pairs = LocationManager.GetPairsWithItemAndHint(ItemManager.GetItemId((int)seph, APItemType.Librarian), true);
            infos.Add(new Tuple<string, string, bool, Sprite>(
                    $"Librarian: {(pairs.Count > 0 ? LocationManager.FormatPairLocation(pairs.First()) : "No Hints")}",
                    $"{floorInfo.Librarians - (seph == SephirahType.Binah ? 2 : 1)}/{(seph == SephirahType.Binah ? 3 : 4)}",
                    floorInfo.Librarians - (seph == SephirahType.Binah ? 2 : 1) >= (seph == SephirahType.Binah ? 3 : 4),
                    lockIcon
            ));

            // Black Silence/Binah
            if (seph == SephirahType.Keter)
            {
                pairs = LocationManager.GetPairsWithItemAndHint(ItemManager.GetOtherItemId(OtherItem.BlackSilence), true);
                infos.Add(new Tuple<string, string, bool, Sprite>(
                    $"Black Silence's Page: {(pairs.Count > 0 ? LocationManager.FormatPairLocation(pairs.First()) : "No Hints")}",
                    $"{(PlaythruManager.BlackSilenceUnlocked ? 1 : 0)}/1",
                    PlaythruManager.BlackSilenceUnlocked,
                    lockIcon
                ));
            }
            else if (seph == SephirahType.Binah)
            {
                pairs = LocationManager.GetPairsWithItemAndHint(ItemManager.GetOtherItemId(OtherItem.Binah), true);
                infos.Add(new Tuple<string, string, bool, Sprite>(
                    $"Binah: {(pairs.Count > 0 ? LocationManager.FormatPairLocation(pairs.First()) : "No Hints")}",
                    $"{(PlaythruManager.BinahUnlocked ? 1 : 0)}/1",
                    PlaythruManager.BinahUnlocked,
                    lockIcon
                ));
            }


            // Book requirements
            int stageIndex = floorInfo.AbnoStage - 1;
            if (SlotDataManager.AbnoBookRequirements.ContainsKey(seph) &&
                stageIndex >= 0 &&
                stageIndex < SlotDataManager.AbnoBookRequirements[seph].Count)
            {
                List<int> books = SlotDataManager.AbnoBookRequirements[seph][stageIndex];
                foreach (int book in books)
                {
                    pairs = LocationManager.GetPairsWithItemAndHint(ItemManager.GetItemId(book, APItemType.Book), true);
                    infos.Add(new Tuple<string, string, bool, Sprite>(
                        $"{DropBookXmlList.Instance.GetData(new LorId(book)).Name}: {(pairs.Count > 0 ? LocationManager.FormatPairLocation(pairs.First()) : "No Hints")}",
                        $"{DropBookInventoryModel.Instance.GetBookCount(book)}/1",
                        DropBookInventoryModel.Instance.GetBookCount(book) > 0,
                        exclamMarkIcon
                    ));
                }
            }


            // Now render this shit!
            for (int i = 0; i < 7; i++)
            {
                UIFloorQuestSlot slot = __instance.questSlotList[i];

                if (i >= infos.Count)
                {
                    slot.SetActiveSlot(false);
                    continue;
                }
                slot.SetActiveSlot(true);

                Tuple<string, string, bool, Sprite> info = infos[i];

                slot.SetColor(info.Item3 ? UIColorManager.Manager.GetUIColor(UIColor.Disabled) : UIColorManager.Manager.GetUIColor(UIColor.Default));

                slot.txt_QuestName.text = info.Item1;
                slot.txt_QuestName.ForceMeshUpdate();
                slot.img_BgFrame.rectTransform.sizeDelta = new Vector2(slot.txt_QuestName.preferredWidth + 10f, slot.img_BgFrame.rectTransform.sizeDelta.y);
                slot.txt_QuestName.enabled = false;
                slot.txt_QuestName.enabled = true;
                float x = (slot.txt_QuestName.preferredWidth + 25f > 370f) ? 370f : (slot.txt_QuestName.preferredWidth + 25f);
                slot.img_BgFrame.rectTransform.sizeDelta = new Vector2(x, slot.img_BgFrame.rectTransform.sizeDelta.y);

                slot.txt_QuestProgress.text = info.Item2;

                slot.img_Icon.sprite = info.Item4;
                slot.img_Icon.enabled = true;
                slot.img_Icon.color = (info.Item3 ? UIColorManager.Manager.GetUIColor(UIColor.Disabled) : UIColorManager.Manager._floorQuestSlotIconColor[0]);
            }

            return false;
        }



        // ItemXmlDataList patch. Remove combat page exclusiveness. //
        [HarmonyPatch(typeof(ItemXmlDataList), nameof(ItemXmlDataList.InitCardInfo))]
        [HarmonyPrefix]
        static void RemoveCombatPageExclusiveness(ItemXmlDataList __instance, ref List<DiceCardXmlInfo> list)
        {
            list.ForEach(c => c.optionList.Remove(CardOption.OnlyPage));
        }



        // StageClearInfoListModel patch. Force game to think every reception was cleared once. //
        [HarmonyPatch(typeof(StageClearInfoListModel), nameof(StageClearInfoListModel.GetClearCount), typeof(LorId))]
        [HarmonyPrefix]
        static bool FakeClear(StageClearInfoListModel __instance, LorId stageId, ref int __result)
        {
            if (stageId.id == 210005 || stageId.id == 210006 || stageId.id == 210007 || stageId.id == 210008 || stageId.id == 210009)
            {
                return true;
            }

            __result = 1;

            return false;
        }



        // LibraryModel patches. Black Silence and Binah unlocks. //
        // Stop library from opening Keter floor from the start.
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.Init))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> GenerateDropsSendChecks(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Callvirt, OpCodes.Ldarg_0, OpCodes.Ldc_I4_S, OpCodes.Call)
                .Advance(1)
                .RemoveInstructions(3);

            return codeMatcher.Instructions();
        }

        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.IsBinahLockedInLibrary))]
        [HarmonyPrefix]
        static bool IsBinahLockedInLibrary(LibraryModel __instance, ref bool __result)
        {
            __result = !PlaythruManager.BinahUnlocked;

            return false;
        }

        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.IsBlackSilenceLockedInLibrary))]
        [HarmonyPrefix]
        static bool IsBlackSilenceLockedInLibrary(LibraryModel __instance, ref bool __result)
        {
            __result = !PlaythruManager.BlackSilenceUnlocked;

            return false;
        }

        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.IsBinahLockedInStage))]
        [HarmonyPrefix]
        static bool IsBinahLockedInStage(LibraryModel __instance, StageClassInfo stageInfo, ref bool __result)
        {
            __result = !PlaythruManager.BinahUnlocked;

            return false;
        }

        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.IsBlackSilenceLockedInStage))]
        [HarmonyPrefix]
        static bool IsBlackSilenceLockedInStage(LibraryModel __instance, StageClassInfo stageInfo, ref bool __result)
        {
            __result = !PlaythruManager.BlackSilenceUnlocked;

            return false;
        }


        // LibraryFloorModel patch. Custom unlocked units amount. //
        [HarmonyPatch(typeof(LibraryFloorModel), nameof(LibraryFloorModel.UpdateOpenedCount), typeof(int))]
        [HarmonyPrefix]
        static bool UpdateOpenedCountPrefix(LibraryFloorModel __instance)
        {
            __instance._opendUnitCount = PlaythruManager.Floors[__instance.Sephirah].Librarians;

            return false;
        }


        // UIMainPanel patch. Fake last selectable sephirah to be hokma
        [HarmonyPatch(typeof(UIMainPanel), nameof(UIMainPanel.GetLastSelectableSephirah))]
        [HarmonyPrefix]
        static bool FakeTransformInfo(UIMainPanel __instance, ref SephirahType __result)
        {
            __result = SephirahType.Hokma;

            return false;
        }

        [HarmonyPatch(typeof(UIMainPanel), nameof(UIMainPanel.SetKetherTransform))]
        [HarmonyPrefix]
        static bool IsBinahLockedInLibrary(UIMainPanel __instance, KetherTransformInfo info, bool isRight)
        {
            isRight = false;

            if (!LibraryModel.Instance.IsOpenedSephirah(SephirahType.Keter))
            {
                __instance.SephirahButtons[10].gameObject.SetActive(false);
                __instance.SephirahButtons[9].gameObject.SetActive(false);

                return false;
            }

            return true;
        }



        // UIController patch. Change position of AP messages when changing ui screens. //
        [HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.CallUIPhase), typeof(UIPhase))]
        [HarmonyPrefix]
        static void APMessagesPosition(UIController __instance, UIPhase phase)
        {
            switch (phase)
            {
                case UIPhase.Sepiroth:
                case UIPhase.Sephirah:
                case UIPhase.Librarian:
                case UIPhase.Librarian_CardList:
                case UIPhase.FloorFeedingBookFixed:
                case UIPhase.GachaResult:
                case UIPhase.Invitation:
                case UIPhase.Main_ItemList:
                    if (APLog.isAtBottom)
                        break;
                    APLog.Show();
                    APLog.SetLogAtBottom(true);
                    break;
                case UIPhase.DUMMY:
                    if (!APLog.isAtBottom)
                        break;
                    APLog.Show();
                    APLog.SetLogAtBottom(false);
                    break;
                case UIPhase.Story:
                case UIPhase.BattleSetting:
                case UIPhase.BattleResult:
                    APLog.Hide();
                    break;
            }
        }



        // UILibrarySliderPanel patch. Replace library level with the AP Progress. //
        [HarmonyPatch(typeof(UILibrarySliderPanel), nameof(UILibrarySliderPanel.SetData))]
        [HarmonyPrefix]
        static bool APProgressBar(UILibrarySliderPanel __instance)
        {
            int chapter = LibraryModel.Instance.GetChapter();
            __instance.img_CityIcon.sprite = UISpriteDataManager.instance._bookGradeFilterIcon[chapter - 1].icon;
            __instance.img_CityIconGlow.sprite = UISpriteDataManager.instance._bookGradeFilterIcon[chapter - 1].iconGlow;

            float num = LocationManager.CheckedLocations.Count;
            float num2 = LocationManager.AllLocations.Count;
            float sliderLength = __instance.sliderLength;

            var txt_leveltxt = __instance.txt_leveltxt;
            var img_SliderMaskGauge = __instance.img_SliderMaskGauge;

            if (num2 <= 0f)
            {
                txt_leveltxt.text = "0/0";
                img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(0f, img_SliderMaskGauge.rectTransform.sizeDelta.y);
                return false;
            }

            float x = sliderLength * (num / num2);

            if (num >= num2)
            {
                txt_leveltxt.text = "MAX";
                img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(sliderLength, img_SliderMaskGauge.rectTransform.sizeDelta.y);
                return false;
            }

            txt_leveltxt.text = $"{num}/{num2}";
            img_SliderMaskGauge.rectTransform.sizeDelta = new Vector2(x, img_SliderMaskGauge.rectTransform.sizeDelta.y);

            return false;
        }



        // UITitlePanel patch. Replace library level text with the AP Progress. //
        [HarmonyPatch(typeof(UITitlePanel), nameof(UITitlePanel.SetMainTitle))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> APProgressText(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Ldarg_1);

            instr.RemoveRange(pos, 3);
            instr.Insert(pos, new CodeInstruction(OpCodes.Ldstr, "AP Run Progress"));

            return instr;
        }



        // BookModel patch. Custom max Passive Cost. //
        [HarmonyPatch(typeof(BookModel), nameof(BookModel.GetMaxPassiveCost))]
        [HarmonyPrefix]
        static bool CustomMaxPassiveCost(DropBookXmlInfo __instance, ref int __result)
        {
            __result = PlaythruManager.MaxAttributionPoints;

            return false;
        }



        // SaveManager patch. When game tries to save, instead save the game with custom save system. //
        [HarmonyPatch(typeof(GameSave.SaveManager), nameof(GameSave.SaveManager.SavePlayData))]
        [HarmonyPrefix]
        static bool SaveGame(GameSave.SaveManager __instance)
        {
            Gameplay.SaveManager.SaveGame();

            return false;
        }


        // When going to title: Disconnect from AP; Destroy randomized reception tree;
        [HarmonyPatch(typeof(UIBgScreenChangeAnim), nameof(UIBgScreenChangeAnim.StartBg))]
        [HarmonyPrefix]
        static void ToTitlePatch(UIBgScreenChangeAnim __instance, UIScreenChangeType cType)
        {
            if (cType != UIScreenChangeType.ReturnTitle)
                return;

            // Clear map
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;
            if (SlotDataManager.HasBattleTree)
            {
                foreach (var icon in MapPanel.iconList)
                {
                    GameObject.Destroy(icon.gameObject);
                    GameObject.Destroy(icon);
                }
            }
            else
            {
                foreach (var icon in MapPanel.iconList)
                {
                    icon.SetActiveStory(false);
                }
            }
            MapPanel.iconList.Clear();

            // Stop Item Manager & Disconnect from AP
            ItemManager.Suspended = true;
            SessionManager.EndSession();
        }



        // PassiveModel patch. Fix saving passive attribution because PM Code. //
        [HarmonyPatch(typeof(PassiveModel), nameof(PassiveModel.LoadFromSaveData))]
        [HarmonyPrefix]
        static bool WhyDoesItEvenBreakBruh(PassiveModel __instance, SaveData data)
        {
            SaveData data2 = data.GetData("passivecurrentid");
            LorId id = LorId.None;
            if (data2 != null)
            {
                id = new LorId(data2.GetInt("_id"));
            }
            SaveData data3 = data.GetData("passiveprevid");
            LorId id2 = LorId.None;
            if (data3 != null)
            {
                id2 = new LorId(data3.GetInt("_id"));
            }
            PassiveXmlInfo data4 = Singleton<PassiveXmlList>.Instance.GetData(id);
            __instance.originpassive = Singleton<PassiveXmlList>.Instance.GetData(id2);
            int @int = data.GetInt("receivebookinstanceid");
            int int2 = data.GetInt("givebookinstanceid");
            __instance.originData = new PassiveModel.PassiveModelSavedData(data4, @int, int2);

            return false;
        }



        // UIFloorPanel patch. Remove binah and black silence open messages and floor stories on open and etc. //
        [HarmonyPatch(typeof(UIFloorPanel), nameof(UIFloorPanel.CheckOpenFloor))]
        [HarmonyPrefix]
        static bool CheckOpenFloorPatch() => false;





        // GameSceneManager patch. Add custom content when game starts. //
        [HarmonyPatch(typeof(GameSceneManager), nameof(GameSceneManager.Start))]
        [HarmonyPostfix]
        static void AddCustomContent()
        {
            ContentManager.Init();
        }



        // PlatformManager patch. Make game unable to grant steam achievements. //
        [HarmonyPatch(typeof(PlatformManager), nameof(PlatformManager.UnlockAchievement))]
        [HarmonyPrefix]
        static bool DisableAchievements() => false;



        // UIMainAutoTooltipManager patch. Remove tutorial tooltips. //
        [HarmonyPatch(typeof(UIMainAutoTooltipManager), nameof(UIMainAutoTooltipManager.OpenTooltip))]
        [HarmonyPrefix]
        static bool DisableTooltips() => false;



        // UIInvenFeedBookList patch. Remove the tutorial highlight of "none" book in feed book menu. //
        [HarmonyPatch(typeof(UIInvenFeedBookList), nameof(UIInvenFeedBookList.OnOpen))]
        [HarmonyPostfix]
        static void NoFeedBookHighlight(UIInvenFeedBookList __instance)
        {
            (__instance.bookSlotList[0] as UIInvenFeedBookSlot).ob_tutorialHighlightFrame.SetActive(false);
        }


        // UIInvenFeedBookList patch. Add "LORAP vX" to version number because why not?. //
        [HarmonyPatch(typeof(VersionViewer), nameof(VersionViewer.Start))]
        [HarmonyPostfix]
        static void Version(VersionViewer __instance)
        {
            __instance.GetComponent<Text>().text += $"\nLORAP {LORAP.ModVersion}";
        }

        // Increase max amount of passive attributed books
        [HarmonyPatch(typeof(BookModel), nameof(BookModel.IsNotFullEquipPassiveBook))]
        [HarmonyPrefix]
        static bool MorePassiveBooks(BookModel __instance, ref bool __result)
        {
            __result = __instance.reservedData.equipedBookIdListInPassive.Count < 16;
            return false;
        }

        // Testing
        [HarmonyPatch(typeof(BookModel), nameof(BookModel.TryGainUniquePassive))]
        [HarmonyPostfix]
        static void MorePassives(BookModel __instance) // Add 8 + amount of passive limit items?
        {
            for (int i = 0; i < 10; i++)
            {
                __instance._activatedAllPassives.Add(new PassiveModel(LorId.None, __instance.instanceId, 1));
            }
        }
    }
}
