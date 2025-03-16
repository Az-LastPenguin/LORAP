using GameSave;
using HarmonyLib;
using LOR_DiceSystem;
using LORAP.Archipelago;
using LORAP.CustomUI;
using LORAP.Gameplay;
using LORAP.Playthru;
using LORAP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using static StageController;

namespace LORAP.Patches
{
    internal class OtherPatches
    {
        // UIFloorQuestPanel patch. Change Quest info to hints. //
        [HarmonyPatch(typeof(UIFloorQuestPanel), nameof(UIFloorQuestPanel.SetData))]
        [HarmonyPrefix]
        static bool QuestToHints(UIFloorQuestPanel __instance, LibraryFloorModel floor)
        {
            var questSlotList = __instance.questSlotList;

            void setQuestText(UIFloorQuestSlot slot, string text, bool complete)
            {
                slot.SetActiveSlot(on: true);
                slot.SetColor(complete ? UIColorManager.Manager.GetUIColor(UIColor.Disabled) : UIColorManager.Manager.GetUIColor(UIColor.Default));

                slot.txt_QuestName.text = text;
                slot.txt_QuestName.ForceMeshUpdate();
                slot.img_BgFrame.rectTransform.sizeDelta = new Vector2(slot.txt_QuestName.preferredWidth + 10f, slot.img_BgFrame.rectTransform.sizeDelta.y);

                slot.txt_QuestName.enabled = false;
                slot.txt_QuestName.enabled = true;

                float x = (slot.txt_QuestName.preferredWidth + 25f > 370f) ? 370f : (slot.txt_QuestName.preferredWidth + 25f);
                slot.img_BgFrame.rectTransform.sizeDelta = new Vector2(x, slot.img_BgFrame.rectTransform.sizeDelta.y);

                slot.txt_QuestProgress.text = "";

                slot.img_Icon.sprite = (complete ? UISpriteDataManager.instance._floorQuestStateIcon[1] : UISpriteDataManager.instance._floorQuestStateIcon[0]);
                slot.img_Icon.enabled = true;
                slot.img_Icon.color = (complete ? UIColorManager.Manager._floorQuestSlotIconColor[1] : UIColorManager.Manager._floorQuestSlotIconColor[0]);
            }

            setQuestText(questSlotList[0], $"Abno Page: Unknown", false);
            setQuestText(questSlotList[1], $"EGO: Unknown", false);
            setQuestText(questSlotList[2], $"Librarian: Unknown", false);
            if (floor.Sephirah == SephirahType.Keter)
                setQuestText(questSlotList[3], $"Black Silence: Unknown", false);
            if (floor.Sephirah == SephirahType.Binah)
                setQuestText(questSlotList[3], $"Binah: Unknown", false);
            else
                questSlotList[3].SetActiveSlot(on: false);
            questSlotList[4].SetActiveSlot(on: false);

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
        // Custom floor unit count
        [HarmonyPatch(typeof(LibraryFloorModel), nameof(LibraryFloorModel.UpdateOpenedCount), typeof(int))]
        [HarmonyPrefix]
        static bool UpdateOpenedCountPrefix(LibraryFloorModel __instance)
        {
            __instance._opendUnitCount = Math.Max(1, __instance._opendUnitCount);

            return false;
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

            float num = ConnectionManager.FoundLocations;
            float num2 = ConnectionManager.TotalLocations;
            float sliderLength = __instance.sliderLength;
            float x = sliderLength * (num / num2);

            var txt_leveltxt = __instance.txt_leveltxt;
            var img_SliderMaskGauge = __instance.img_SliderMaskGauge;

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



        // UITitlePanel patch. Replace library level text with the AP Progress. //
        [HarmonyPatch(typeof(UITitlePanel), nameof(UITitlePanel.SetMainTitle))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> APProgressText(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Ldarg_1);

            instr.RemoveRange(pos, 3);
            instr.Insert(pos, new CodeInstruction(OpCodes.Ldstr, "Archipelago Progress"));

            return instr;
        }



        // BookModel patch. Custom max Passive Cost. //
        [HarmonyPatch(typeof(BookModel), nameof(BookModel.GetMaxPassiveCost))]
        [HarmonyPrefix]
        static bool CustomMaxPassiveCost(DropBookXmlInfo __instance, ref int __result)
        {
            __result = PlaythruManager.MaxPassiveCost;

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



        // UIBgScreenChangeAnim patch. Disconnect from AP when going to title. //
        [HarmonyPatch(typeof(UIBgScreenChangeAnim), nameof(UIBgScreenChangeAnim.StartBg))]
        [HarmonyPrefix]
        static void ToTitleDisconnectAP(UIBgScreenChangeAnim __instance, UIScreenChangeType cType)
        {
            if (cType == UIScreenChangeType.ReturnTitle)
                ConnectionManager.APDisconnect();
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
            ContentManager.AddCustomContent();
            APConnectWindow.Init();
        }



        // PlatformManager patch. Make game unable to grant steam achievements. //
        [HarmonyPatch(typeof(PlatformManager), nameof(PlatformManager.UnlockAchievement))]
        [HarmonyPrefix]
        static bool NoAchievements() => false;



        // UIMainAutoTooltipManager patch. Remove tutorial tooltips. //
        [HarmonyPatch(typeof(UIMainAutoTooltipManager), nameof(UIMainAutoTooltipManager.OpenTooltip))]
        [HarmonyPrefix]
        static bool NoTooltips() => false;



        // UIInvenFeedBookList patch. Remove highlight of "none" book in feed book menu. //
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
    }
}
