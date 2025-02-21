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
    [HarmonyPatch(typeof(UIFloorQuestPanel))]
    internal class QuestToHintsPatch
    {
        // Change Quest info to hints
        [HarmonyPatch("SetData")]
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
    }

    [HarmonyPatch(typeof(ItemXmlDataList))]
    internal class CombatPageExclusivenessPatch
    {
        // Remove combat page exclusiveness
        [HarmonyPatch("InitCardInfo")]
        [HarmonyPrefix]
        static void RemoveCombatPageExclusiveness(ItemXmlDataList __instance, ref List<DiceCardXmlInfo> list)
        {
            list.ForEach(c => c.optionList.Remove(CardOption.OnlyPage));
        }
    }

    [HarmonyPatch(typeof(StageClearInfoListModel))]
    internal class FakeClearCount
    {
        // Force game to think every reception was cleared once, so no tutorial and other useless stuff
        [HarmonyPatch("GetClearCount", typeof(LorId))]
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
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class Unlocks
    {
        // Self explainatory
        [HarmonyPatch(nameof(LibraryModel.IsBinahLockedInLibrary))]
        [HarmonyPrefix]
        static bool IsBinahLockedInLibraryPrefix(LibraryModel __instance, ref bool __result)
        {
            __result = !PlaythruManager.BinahUnlocked;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsBlackSilenceLockedInLibrary))]
        [HarmonyPrefix]
        static bool IsBlackSilenceLockedInLibraryPrefix(LibraryModel __instance, ref bool __result)
        {
            __result = !PlaythruManager.BlackSilenceUnlocked;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsBinahLockedInStage))]
        [HarmonyPrefix]
        static bool IsBinahLockedInStage(LibraryModel __instance, StageClassInfo stageInfo, ref bool __result)
        {
            __result = !PlaythruManager.BinahUnlocked;

            return false;
        }

        [HarmonyPatch(nameof(LibraryModel.IsBlackSilenceLockedInStage))]
        [HarmonyPrefix]
        static bool IsBlackSilenceLockedInStage(LibraryModel __instance, StageClassInfo stageInfo, ref bool __result)
        {
            __result = !PlaythruManager.BlackSilenceUnlocked;

            return false;
        }
    }

    [HarmonyPatch(typeof(LibraryFloorModel))]
    internal class Something
    {
        // Custom floor unit count
        [HarmonyPatch(nameof(LibraryFloorModel.UpdateOpenedCount), typeof(int))]
        [HarmonyPrefix]
        static bool UpdateOpenedCountPrefix(LibraryFloorModel __instance)
        {
            __instance._opendUnitCount = Math.Max(1, __instance._opendUnitCount);

            return false;
        }
    }

    [HarmonyPatch(typeof(UI.UIController))]
    internal class APMessagesPos
    {
        // Change position of AP Messages
        [HarmonyPatch(nameof(UI.UIController.CallUIPhase), typeof(UIPhase))]
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
    }

    [HarmonyPatch(typeof(UILibrarySliderPanel))]
    internal class APProgressPatch1
    {
        // Replace library level with the AP Progress
        [HarmonyPatch(nameof(UILibrarySliderPanel.SetData))]
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
    }

    [HarmonyPatch(typeof(UITitlePanel))]
    internal class APProgressPatch2
    {
        // Replace library level text with the AP Progress
        [HarmonyPatch("SetMainTitle")]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> APProgressText(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instr = instructions.ToList();
            var pos = instr.FindIndex(i => i.opcode == OpCodes.Ldarg_1);

            instr.RemoveRange(pos, 3);
            instr.Insert(pos, new CodeInstruction(OpCodes.Ldstr, "Archipelago Progress"));

            return instr;
        }
    }

    [HarmonyPatch(typeof(BookModel))]
    internal class PassiveCostPatch
    {
        // Custom max passive Cost
        [HarmonyPatch(nameof(BookModel.GetMaxPassiveCost))]
        [HarmonyPrefix]
        static bool CustomMaxPassiveCost(DropBookXmlInfo __instance, ref int __result)
        {
            __result = PlaythruManager.MaxPassiveCost;

            return false;
        }
    }

    [HarmonyPatch(typeof(GameSave.SaveManager))]
    internal class CustomSaveGame
    {
        // When game tries to save, instead save the game with custom save system
        [HarmonyPatch(nameof(GameSave.SaveManager.SavePlayData))]
        [HarmonyPrefix]
        static bool SaveGame(GameSave.SaveManager __instance)
        {
            Gameplay.SaveManager.SaveGame();

            return false;
        }
    }

    [HarmonyPatch(typeof(UIBattleResultLeftPanel))]
    internal class LostBooksRemovePatch
    {
        // Remove "Books Lost" UI because you lose literally nothing in this mod
        [HarmonyPatch(nameof(UIBattleResultLeftPanel.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> LostBooksRemove(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instr = instructions.ToList();
            var cur = instr.FindIndex(i => i.opcode == OpCodes.Stfld) + 1;

            instr.RemoveRange(cur, 19);

            return instr;
        }
    }

    [HarmonyPatch(typeof(UI.UIController))]
    internal class ForfeitFloorSelection
    {
        public static void ForfeitClick()
        {
            UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, (bool yes) =>
            {
                if (!yes) return;

                Singleton<StageController>.Instance.GameOver(iswin: false, isbackbutton: true);
                GameSceneManager.Instance.ActivateUIController();
                SingletonBehavior<UIBgScreenChangeAnim>.Instance.StartBg(UIScreenChangeType.BackInvitation);
            });

            UIAlarmPopup.instance.txt_alarm.text = "Are you sure you want to forfeit the battle?";
        }

        // Change "Forfeit" and "Return to Title" buttons' behaviour
        [HarmonyPatch(nameof(UI.UIController.BackBattlePrepare))] // TODO: Also replace that text for endgame content thing
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReplaceButton(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var instr = instructions.ToList();
            var pos = instr.IndexOf(instr.Where(i => i.opcode == OpCodes.Ldc_I4_S).ElementAt(1)) - 1;

            var l = instr[pos].labels.ElementAt(0);

            instr.RemoveRange(pos, 15);

            instr.Insert(pos, new CodeInstruction(OpCodes.Callvirt, AccessTools.Method(typeof(ForfeitFloorSelection), nameof(ForfeitFloorSelection.ForfeitClick))).WithLabels(l));

            return instr;
        }
    }

    [HarmonyPatch(typeof(UIEscPanel))]
    internal class ToTitleAndForfeit
    {
        // Change "Manual" to "Forfeit"
        [HarmonyPatch(nameof(UIEscPanel.Open))]
        [HarmonyPostfix]
        static void EscMenuButtonRename(UIEscPanel __instance)
        {
            __instance.buttons.ElementAt(1).GetComponentInChildren<TextMeshProUGUI>().text = "Forfeit";
        }

        // Make "Forfeit" button disabled if Esc menu is opened when not in battle
        [HarmonyPatch(nameof(UIEscPanel.Open))]
        [HarmonyPostfix]
        static void EscMenuButtonDisable(UIEscPanel __instance)
        {
            if (StageController.Instance._state == StageState.None || (StageController.Instance.Phase != StagePhase.ApplyLibrarianCardPhase && StageController.Instance.Phase != StagePhase.RoundStartPhase_System))
            {
                __instance.buttons.ElementAt(1).SetDisabled();
                __instance.buttons.ElementAt(1).selectable.interactable = false;
            }
            else
            {
                __instance.buttons.ElementAt(1).SetDefault();
                __instance.buttons.ElementAt(1).selectable.interactable = true;
            }
        }

        public static void EndBattle()
        {
            UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, (bool yes) =>
            {
                if (!yes) return;

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);
                SingletonBehavior<UIPopupWindowManager>.Instance.CloseUI(UIPopupType.Esc);
                StageController.Instance.SetUnequipCardAll();

                foreach (var floor in StageController.Instance._stageModel._floorList)
                {
                    floor.Defeat();
                }

                StageController.Instance.EndBattle();
            });

            UIAlarmPopup.instance.txt_alarm.text = "Are you sure you want to forfeit the battle?";
        }

        [HarmonyPatch(nameof(UIEscPanel.OnClickEvent))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EscapeMenuPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            // Change "Forfeit" and "Return to Title" buttons' behaviour
            CIWriter Writer = new CIWriter(instructions, generator);

            Writer.ToPattern(OpCodes.Call, OpCodes.Ldc_I4_2, OpCodes.Callvirt);

            Writer.Nop(); // Save a label
            Writer.Remove(2);

            Writer.ToPattern(OpCodes.Call, OpCodes.Ldc_I4_S, OpCodes.Ldc_I4_1, OpCodes.Ldarg_0);

            Label TitleWarnLabel = Writer.AddLabel();

            Writer.ToPattern(OpCodes.Brtrue);

            Writer.Remove();

            Writer.Insert(new CodeInstruction(OpCodes.Brtrue_S, TitleWarnLabel));

            return Writer.Instructions;
        }
    }

    [HarmonyPatch(typeof(UIBgScreenChangeAnim))]
    internal class ToTitleDisconnect
    {
        // Disconnect from AP when going to title
        [HarmonyPatch(nameof(UIBgScreenChangeAnim.StartBg))]
        [HarmonyPrefix]
        static void ToTitleDisconnectAP(UIBgScreenChangeAnim __instance, UIScreenChangeType cType)
        {
            if (cType == UIScreenChangeType.ReturnTitle)
                ConnectionManager.APDisconnect();
        }
    }

    [HarmonyPatch(typeof(PassiveModel))]
    internal class PassiveAttributionFix
    {
        // Fix saving passive attribution because PM Code
        [HarmonyPatch(nameof(PassiveModel.LoadFromSaveData))]
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
    }

    [HarmonyPatch(typeof(UIFloorPanel))]
    internal class FloorOpenAndUpgradePatch
    {
        // Remove binah and black silence open messages and floor stories on open and etc.
        [HarmonyPatch(nameof(UIFloorPanel.CheckOpenFloor))]
        [HarmonyPrefix]
        static bool CheckOpenFloorPatch(UIFloorPanel __instance)
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(GameSceneManager))]
    internal class AddCustomContentPatch
    {
        // Add custom content when game starts
        [HarmonyPatch(nameof(GameSceneManager.Start))]
        [HarmonyPostfix]
        static void AddCustomContent()
        {
            ContentManager.AddCustomContent();
            APConnectWindow.Init();
        }
    }

    [HarmonyPatch(typeof(PlatformManager))]
    internal class NoAchievementsPatch
    {
        // Make game unable to grant steam achievements
        [HarmonyPatch(nameof(PlatformManager.UnlockAchievement))]
        [HarmonyPrefix]
        static bool NoAchievements()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UIMainAutoTooltipManager))]
    internal class NoTooltipsPatch
    {
        // Remove tutorial tooltips
        [HarmonyPatch(nameof(UIMainAutoTooltipManager.OpenTooltip))]
        [HarmonyPrefix]
        static bool NoTooltips()
        {
            return false;
        }
    }

    [HarmonyPatch(typeof(UIInvenFeedBookList))]
    internal class NoFeedBookHighlightPatch
    {
        // Remove highlight of "none" book in feed book menu
        [HarmonyPatch(nameof(UIInvenFeedBookList.OnOpen))]
        [HarmonyPostfix]
        static void NoFeedBookHighlight(UIInvenFeedBookList __instance)
        {
            (__instance.bookSlotList[0] as UIInvenFeedBookSlot).ob_tutorialHighlightFrame.SetActive(false);
        }
    }

    [HarmonyPatch(typeof(VersionViewer))]
    internal class VersionPatch
    {
        // Add "LORAP vX.X" to version number because why not?
        [HarmonyPatch(nameof(VersionViewer.Start))]
        [HarmonyPostfix]
        static void Version(VersionViewer __instance)
        {
            __instance.GetComponent<Text>().text += $"\nLORAP {LORAP.ModVersion}";
        }
    }
}
