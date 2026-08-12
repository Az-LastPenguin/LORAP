using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
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
using static UnityEngine.UI.CanvasScaler;

namespace LORAP.Patches
{
    internal class SuppressionsAndReceptions
    {
        // On Abno Suppression or Floor Realization end, send the checks & progress current abno fight #
        // Entire method is overriden because it's easier that way.
        [HarmonyPatch(typeof(StageController), nameof(StageController.EndBattlePhase_creature))]
        [HarmonyPrefix]
        static bool SuppressionEnd(StageController __instance)
        {
            StageController controller = __instance; // For more readable code

            StageModel stageModel = controller.GetStageModel();
            int stageId = stageModel.ClassInfo.id.id;
            StageWaveModel currentWave = controller._stageModel.GetWave(controller._currentWave);
            StageLibraryFloorModel currentFloor = controller._stageModel.GetFloor(controller._currentFloor);

            // If either no waves left or floor is defeated
            if (stageModel.GetFrontAvailableWave() == null || stageModel.GetFrontAvailableFloor() == null)
            {
                bool won = stageModel.GetFrontAvailableWave() == null;

                if (won)
                {
                    PlaythruManager.MarkStageCompleted(stageId);

                    switch (stageModel.ClassInfo._id)
                    {
                        // Black Silence I-IV
                        case 210005:
                        case 210006:
                        case 210007:
                        case 210008:
                            Debug.Log("[LORAP] Keter Realization Progress");
                            PlaythruManager.ProgressKeterRealization();

                            controller._enemyStageManager.OnStageClear();

                            BattleManagerUI battleManagerUI = BattleManagerUI.Instance;
                            battleManagerUI.ui_TargetArrow.ActiveTargetParent(false);
                            battleManagerUI.ui_TargetArrow.ClearCloneArrows();
                            battleManagerUI.ui_emotionInfoBar.targetingToggle.SetDefault();
                            if (battleManagerUI.ui_emotionInfoBar.autoCardButton != null)
                                battleManagerUI.ui_emotionInfoBar.autoCardButton.SetActivate(false);
                            if (battleManagerUI.ui_emotionInfoBar.unequipcardallButton != null)
                                battleManagerUI.ui_emotionInfoBar.unequipcardallButton.SetActivate(false);

                            controller.firstStartState = false;

                            var seph = controller._currentFloor;
                            StageClassInfo stageInfo = StageClassInfoList.Instance.GetData(210005 + PlaythruManager.KeterRealizationStage);

                            // InitStageForKeterCompleteOpen
                            controller.InitStageByCreature(stageInfo);
                            controller.firstStartState = true;
                            controller._isEndContentsStage = true;

                            StageLibraryFloorModel _floor = controller._stageModel.GetFloor(seph);
                            if (_floor == null)
                                return false;

                            controller.SetCurrentSephirah(seph);

                            int num = 0;
                            foreach (UnitBattleDataModel unitBattleData in _floor.GetUnitBattleDataList())
                            {
                                unitBattleData.IsAddedBattle = false;
                                if (!unitBattleData.isDead && num < controller._stageModel.GetWave(controller._currentWave).AvailableUnitNumber)
                                {
                                    unitBattleData.IsAddedBattle = true;
                                    num++;
                                }
                            }

                            SaveManager.SaveGame();

                            GlobalGameManager.Instance.LoadBattleScene();

                            break;
                        // Black Silence V
                        case 210009:
                        // Every other Suppression/Realization
                        default:
                            if (stageModel.ClassInfo._id == 210009)
                                PlaythruManager.ProgressKeterRealization();

                            Debug.Log("[LORAP] Floor Stage Complete");
                            LocationManager.SendStageChecks(stageId);

                            PlaythruManager.CheckEndConditions();

                            controller.battleState = BattleState.None;
                            GameSceneManager.Instance.ActivateUIController();
                            UIFloorPanel.firstSelectableState = FirstSelectableState.Center;
                            UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);

                            SaveManager.SaveGame();

                            break;
                    }
                }
                else
                {
                    controller.battleState = BattleState.None;
                    GameSceneManager.Instance.ActivateUIController();
                    UIFloorPanel.firstSelectableState = FirstSelectableState.Center;
                    UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);

                    return false;
                }
            }
            else
            {
                controller.battleState = BattleState.Setting;
                if (currentWave.IsUnavailable())
                {
                    Singleton<LibraryQuestManager>.Instance.OnWinWave(currentFloor);
                    controller.SetCurrentWave(controller._currentWave + 1);
                    controller._prevDefeatFloor = SephirahType.None;
                }
                if (currentFloor.IsUnavailable())
                {
                    controller.GameOver(false);
                    UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
                }
            }

            return false;
        }



        // Remove giving of bonus pages when ending certain receptions
        // Entire method is overriden because it's easier that way.
        [HarmonyPatch(typeof(StageController), nameof(StageController.EndBattlePhase_invitation))]
        [HarmonyPrefix]
        static bool ReceptionEnd(StageController __instance)
        {
            StageController controller = __instance;

            StageModel stageModel = controller.GetStageModel();
            StageWaveModel wave = controller._stageModel.GetWave(controller._currentWave);
            StageLibraryFloorModel stageFloorModel = controller._stageModel.GetFloor(controller._currentFloor);

            if (controller._forceFloorChange)
            {
                BattleManagerUI.Instance.ui_TargetArrow.ActiveTargetParent(on: false);
                BattleManagerUI.Instance.ui_TargetArrow.ClearCloneArrows();
                BattleManagerUI.Instance.ui_emotionInfoBar.targetingToggle.SetDefault();
                if (BattleManagerUI.Instance.ui_emotionInfoBar.autoCardButton != null)
                    BattleManagerUI.Instance.ui_emotionInfoBar.autoCardButton.SetActivate(on: false);
                if (BattleManagerUI.Instance.ui_emotionInfoBar.unequipcardallButton != null)
                    BattleManagerUI.Instance.ui_emotionInfoBar.unequipcardallButton.SetActivate(on: false);

                controller.firstStartState = false;

                StageLibraryFloorModel nextFloorModel = controller._stageModel.GetFloor(controller._forceFloorChangeSephirah);
                if (nextFloorModel != null)
                {
                    controller.SetCurrentSephirah(controller._forceFloorChangeSephirah);

                    int num = 0;
                    foreach (UnitBattleDataModel unitBattleData in nextFloorModel.GetUnitBattleDataList())
                    {
                        unitBattleData.IsAddedBattle = false;
                        if (!unitBattleData.isDead && num < wave.AvailableUnitNumber)
                        {
                            unitBattleData.IsAddedBattle = true;
                            num++;
                        }
                    }
                }
                BattleSceneRoot.Instance.StartBattle();
            }
            else if (stageModel.GetFrontAvailableWave() == null || stageModel.GetFrontAvailableFloor() == null)
            {
                bool won = stageModel.GetFrontAvailableWave() == null;

                controller.battleState = BattleState.None;
                controller.GameOver(won);

                if (won)
                {
                    //if (!SlotDataManager.EnemiesTurnIntoChecks)
                    LocationManager.SendStageChecks(stageModel.ClassInfo._id);

                    PlaythruManager.MarkStageCompleted(stageModel.ClassInfo._id);

                    PlaythruManager.CheckEndConditions();
                }

                switch (stageModel.ClassInfo._id)
                {
                    case 60003:
                    case 60004:
                        GameSceneManager.Instance.ActivateUIController();
                        UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);

                        SaveManager.SaveGame();
                        break;
                    default:
                        GameSceneManager.Instance.ActivateUIController();
                        UI.UIController.Instance.CallUIPhase(UIPhase.BattleResult);

                        (UI.UIController.Instance.GetUIPanel(UIPanelType.BattleResult) as UIBattleResultPanel).SetData(new TestBattleResultData
                        {
                            rewardbookdatas = controller._droppedbookdatas,
                            rewardpageResult = new List<BookDropResult>(),
                            iswin = won,
                            loseinvitationbooks = new List<LorId>(),
                            stagemodelInBattle = stageModel,
                            sephirahOrder = new List<SephirahType>(controller._usedFloorList)
                        });

                        // Just in case there were any messages while in battle
                        MessagePopup.Instance.Open();
                        AbnoEgoPagePopup.Instance.Open();

                        SaveManager.SaveGame();
                        break;
                }
            }
            else
            {
                controller.battleState = BattleState.Setting;

                if (wave.IsUnavailable())
                {
                    Singleton<LibraryQuestManager>.Instance.OnWinWave(stageFloorModel);
                    controller.SetCurrentWave(controller._currentWave + 1);
                    controller._prevDefeatFloor = SephirahType.None;
                }
                if (stageFloorModel.IsUnavailable())
                {
                    controller._prevDefeatFloor = stageFloorModel.Sephirah;
                    controller.SetCurrentSephirah(stageModel.GetFrontAvailableFloor().Sephirah);
                }

                BattleManagerUI.Instance.ui_TargetArrow.ActiveTargetParent(on: false);
                BattleManagerUI.Instance.ui_TargetArrow.ClearCloneArrows();
                BattleManagerUI.Instance.ui_emotionInfoBar.targetingToggle.SetDefault();
                if (BattleManagerUI.Instance.ui_emotionInfoBar.autoCardButton != null)
                    BattleManagerUI.Instance.ui_emotionInfoBar.autoCardButton.SetActivate(on: false);
                if (BattleManagerUI.Instance.ui_emotionInfoBar.unequipcardallButton != null)
                    BattleManagerUI.Instance.ui_emotionInfoBar.unequipcardallButton.SetActivate(on: false);

                controller.firstStartState = false;

                GameSceneManager.Instance.OpenBattleSettingUI();
            }

            return false;
        }



        // Disable giving Book of Distortion, Book of LC, Searing Sword and Feather Shield and maybe other stuff that might be given out
        [HarmonyPatch(typeof(StageController), nameof(StageController.BonusRewardWithPopup))]
        [HarmonyPrefix]
        static bool ResolveableRewardsPatch() => false;



        // Remove "Books Lost" UI because you lose literally nothing in this mod. (only your time)
        [HarmonyPatch(typeof(UIBattleResultLeftPanel), nameof(UIBattleResultLeftPanel.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> NoLostBooks(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Ldarg_0, OpCodes.Ldfld)
                .RemoveInstructions(19);

            return codeMatcher.Instructions();
        }



        // Don't remove books on not winning gameover
        [HarmonyPatch(typeof(StageController), nameof(StageController.GameOver))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> DontRemoveBooksOnGameOver(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Ldarg_1)
                .RemoveInstructions(2);

            return codeMatcher.Instructions();
        }



        // Replace vanilla book drops with checks if EnemiesTurnIntoChecks is true.
        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnDie))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EnemyTurnIntoCheck(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Call, OpCodes.Callvirt, OpCodes.Stloc_S, OpCodes.Ldloc_S)
                .RemoveInstructions(47)
                .Insert(Transpilers.EmitDelegate<Action<BattleUnitModel>>((unit) => {
                    if (SettingsManager.EnemiesTurnIntoChecks && StageController.Instance.stageType == StageType.Invitation)
                    {
                        string res = LocationManager.SendRandomReceptionCheck(StageController.Instance._stageModel.ClassInfo.id.id);
                        if (res != "")
                            unit.view._dropBookTexts.Add(res);
                    }
                }));

            return codeMatcher.Instructions();
        }



        // Show what checks can still be acquired from the reception.
        // Entire method is overriden because it's easier that way.
        [HarmonyPatch(typeof(BattleEmotionRewardInfoUI), nameof(BattleEmotionRewardInfoUI.SetData))]
        [HarmonyPrefix]
        static bool EnemiesInfoShowChecks(BattleEmotionRewardInfoUI __instance, List<UnitBattleDataModel> units, Faction faction)
        {
            foreach (BattleEmotionRewardSlotUI s in __instance.slots)
            {
                s.gameObject.SetActive(false);
            }

            if (faction == Faction.Player)
            {
                for (int i = 0; i < units.Count; i++)
                {
                    BattleEmotionRewardSlotUI uslot = __instance.slots[i];
                    UnitBattleDataModel unit = units[i];

                    uslot.gameObject.SetActive(true);
                    uslot.txt_Name.text = unit.unitData.name;
                    uslot.img_emotionlevel.enabled = false;

                    if (unit.emotionDetail.EmotionLevel > 0)
                    {
                        uslot.img_emotionlevel.enabled = true;
                        uslot.img_emotionlevel.sprite = UISpriteDataManager.instance.EmotionLevelIcon[Math.Min(unit.emotionDetail.EmotionLevel, 5)];
                    }

                    foreach (TextMeshProUGUI text in uslot.rewardtexts)
                    {
                        text.gameObject.SetActive(false);
                    }
                }

                return false;
            }

            __instance.slots[0].gameObject.SetActive(true);

            int stageId = StageController.Instance._stageModel.ClassInfo.id.id;

            List<long> locations = LocationManager.GetUncheckedStageLocations(stageId);

            BattleEmotionRewardSlotUI slot = __instance.slots.First();

            slot.txt_Name.text = locations.Count > 0 ? $"Items remaining: {locations.Count}" : "All items collected!";
            slot.img_emotionlevel.sprite = UISpriteDataManager.instance.EmotionLevelIcon[locations.Count < 6 ? locations.Count : 5];

            // Set texts
            for (int i = 0; i < slot.rewardtexts.Count; i++)
            {
                if (i >= locations.Count)
                {
                    slot.rewardtexts[i].gameObject.SetActive(false);
                    continue;
                }

                slot.rewardtexts[i].gameObject.SetActive(true);

                ItemLocationPair pair = LocationManager.GetLocationPair(locations[i]);

                TextMeshProUGUI text = slot.rewardtexts[i];
                text.text = LocationManager.FormatPairItem(pair);
                text.gameObject.SetActive(true);
                slot.SetSizeByText(text);
            }

            // Resize UI
            float num7 = 0f;
            foreach (TextMeshProUGUI rewardtext in slot.rewardtexts)
            {
                if (rewardtext.isActiveAndEnabled)
                {
                    num7 += rewardtext.rectTransform.sizeDelta.y;
                }
            }
            num7 += 40f;
            Vector2 sizeDelta = slot.rect.sizeDelta;
            sizeDelta.y = num7;
            slot.rect.sizeDelta = sizeDelta;
            Vector2 sizeDelta2 = slot.rect_frame.sizeDelta;
            sizeDelta2.y = num7 + 5f;
            slot.rect_frame.sizeDelta = sizeDelta2;
            Vector2 sizeDelta3 = slot.rect_bg.sizeDelta;
            sizeDelta3.y = num7 + 25f;
            slot.rect_bg.sizeDelta = sizeDelta3;

            return false;
        }



        // Make Angela replace any Patron Librarian for Keter Realization.
        [HarmonyPatch(typeof(StageLibraryFloorModel), nameof(StageLibraryFloorModel.InitUnitList))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> AngelaReplace(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Callvirt, OpCodes.Callvirt, OpCodes.Call)
                .ThrowIfInvalid("Couldn't find Instrcutions.")
                .Advance(4)
                .RemoveInstructions(4);

            return codeMatcher.Instructions();
        }



        // Don't drop books from receptions by any means
        [HarmonyPatch(typeof(StageController), nameof(StageController.OnEnemyDropBookForAdded))]
        [HarmonyPrefix]
        static bool NoFreeStuff() => false;



        // Check if there is an available abno fight or a realization.
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.CheckCreatureBossBattle))]
        [HarmonyPrefix]
        static bool CheckRealization(LibraryModel __instance, LibraryFloorModel floor, ref bool __result)
        {
            __result = false;

            return false; // All Realizations are on the map.
        }



        // Check what abno player should fight
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.CanLevelUpSephirah))]
        [HarmonyPrefix]
        static bool CheckSuppression(LibraryModel __instance, SephirahType sep, ref bool __result)
        {
            __result = false;

            return false; // All Suppressions are on the map.
        }



        // Change "Leave" button on floor selection when reception starts
        [HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.BackBattlePrepare))]
        [HarmonyPrefix]
        static bool ReplaceLeaveButton()
        {
            if (StageController.Instance.firstStartState)
            {
                GameSceneManager.Instance.ActivateUIController();
                UIBgScreenChangeAnim.Instance.StartBg(UIScreenChangeType.BackInvitation);

                return false;
            }

            UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, (bool yes) =>
            {
                if (!yes) return;

                StageController.Instance.GameOver(false, true);
                GameSceneManager.Instance.ActivateUIController();
                UIBgScreenChangeAnim.Instance.StartBg(UIScreenChangeType.BackInvitation);
            });

            UIAlarmPopup.instance.txt_alarm.text = "Are you sure you want to abandon the on-going battle?";

            return false;
        }



        // Change "Manual" button text to "Forfeit" // TODO: Add the button, not make one button do other thing
        [HarmonyPatch(typeof(UIEscPanel), nameof(UIEscPanel.Open))]
        [HarmonyPostfix]
        static void EscMenuButtonRename(UIEscPanel __instance)
        {
            __instance.buttons.ElementAt(1).GetComponentInChildren<TextMeshProUGUI>().text = "Forfeit";
        }



        // Make "Forfeit" button disabled if Esc menu is opened when not in battle or when combat pages are being resolved
        [HarmonyPatch(typeof(UIEscPanel), nameof(UIEscPanel.Open))]
        [HarmonyPostfix]
        static void EscMenuButtonState(UIEscPanel __instance)
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



        // Make clicking on "Forfeit" ask if you really want to forfet the battle. Also when leaving to title notify that battle will be lost
        [HarmonyPatch(typeof(UIEscPanel), nameof(UIEscPanel.OnClickEvent))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EscapeMenuPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Call, OpCodes.Ldc_I4_2, OpCodes.Callvirt)
                .SetAndAdvance(OpCodes.Nop, null)
                .RemoveInstructions(2)
                .Insert(Transpilers.EmitDelegate<Action>(() => {
                    UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, (bool yes) =>
                    {
                        if (!yes) return;

                        UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Click);
                        SingletonBehavior<UIPopupWindowManager>.Instance.CloseUI(UIPopupType.Esc);
                        StageController.Instance.SetUnequipCardAll();

                        foreach (var floor in StageController.Instance._stageModel._floorList)
                            floor.Defeat();

                        StageController.Instance.EndBattle();
                    });

                    UIAlarmPopup.instance.txt_alarm.text = "Are you sure you want to forfeit the battle?";
                }))
                .Start().MatchStartForward(OpCodes.Call, OpCodes.Ldc_I4_S, OpCodes.Ldc_I4_1, OpCodes.Ldarg_0)
                .CreateLabel(out Label noPenalty)
                .Start().MatchStartForward(OpCodes.Brtrue)
                .SetOperandAndAdvance(noPenalty);

            return codeMatcher.Instructions();
        }



        // Return Angela her light in the battle prepare screen.
        [HarmonyPatch(typeof(UIBattleSettingPanel), nameof(UIBattleSettingPanel.OnUIPhaseEnter))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> ReturnLight(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Call, OpCodes.Callvirt, OpCodes.Ldc_I4_3, OpCodes.Bne_Un)
                .SetAndAdvance(OpCodes.Nop, null)
                .RemoveInstructions(2)
                .InsertAndAdvance(Transpilers.EmitDelegate<Func<bool>>(() => {
                    int id = StageController.Instance.GetStageModel().ClassInfo.id.id;

                    return id == 210005 || id == 210006 || id == 210007 || id == 210008 || id == 210009;
                }))
                .SetOpcodeAndAdvance(OpCodes.Brfalse_S);

            return codeMatcher.Instructions();
        }
    }
}
