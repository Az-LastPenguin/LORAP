using HarmonyLib;
using UI;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using LORAP.Playthru;
using LORAP.Archipelago;
using System;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using static StageController;

namespace LORAP.Patches
{
    internal class SuppressionsAndReceptions
    {
        // StageController patches. Give checks and do other stuff when ending a reception/suppression. //
        // On Abno Suppression or Floor Realization end, send the checks & progress current abno fight #
        // Fully overriden because too many patches with changes needed to get desired result.
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
                    PlaythruManager.ProgressSuppression(currentFloor.Sephirah);

                    switch (stageModel.ClassInfo.id.id)
                    {
                        // Black Silence I-IV
                        case 210005:
                        case 210006:
                        case 210007:
                        case 210008:
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
                            var data = FloorLevelXmlList.Instance.GetData(seph, seph.FloorModel().GetCurrentAbnoStage());
                            StageClassInfo stageInfo = StageClassInfoList.Instance.GetData(data.stageId);

                            // InitStageForKeterCompleteOpen
                            controller.InitStageByCreature(stageInfo);
                            controller.firstStartState = true;
                            controller._isEndContentsStage = true;

                            StageLibraryFloorModel _floor = controller._stageModel.GetFloor(seph);
                            if (_floor == null)
                                return false;

                            controller.SetCurrentSephirah(SephirahType.Keter);

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

                            Gameplay.SaveManager.SaveGame();

                            GlobalGameManager.Instance.LoadBattleScene();

                            break;
                        // Black Silence V
                        case 210009:
                        // Every other Suppression/Realization
                        default:
                            CheckManager.ClearCheck(stageId);
                            CheckManager.AbnoChecks(currentFloor.Sephirah); // Remove in v0.4
                            PlaythruManager.ReceptionsCompleted.Add(stageId); // Update in v0.4 (?)

                            PlaythruManager.CheckEndConditions();

                            controller.battleState = BattleState.None;
                            GameSceneManager.Instance.ActivateUIController();
                            UIFloorPanel.firstSelectableState = FirstSelectableState.Center;
                            UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);

                            Gameplay.SaveManager.SaveGame();

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
                    controller.GameOver(iswin: false);
                    UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
                }
            }

            return false;
        }

        // Remove giving of bonus pages when ending certain receptions
        // Fully overriden because too many patches with changes needed to get desired result.
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

                if (won && !PlaythruManager.ReceptionsCompleted.Contains(stageModel.ClassInfo._id))
                {
                    // Clear check here
                    PlaythruManager.ReceptionsCompleted.Add(stageModel.ClassInfo._id);

                    CheckManager.ClearCheck(stageModel.ClassInfo._id);

                    PlaythruManager.CheckEndConditions();
                }

                switch (stageModel.ClassInfo._id)
                {
                    case 60003:
                    case 60004:
                        GameSceneManager.Instance.ActivateUIController();
                        UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);

                        Gameplay.SaveManager.SaveGame();
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

                        Gameplay.SaveManager.SaveGame();
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

        // Remove giving book of distortion and book of LC, since they're not used anyway
        [HarmonyPatch(typeof(StageController), nameof(StageController.BonusRewardWithPopup))]
        [HarmonyPrefix]
        static bool ResolveableRewardsPatch() => false;



        // UIBattleResultLeftPanel patch. Remove "Books Lost" UI because you lose literally nothing in this mod. //
        [HarmonyPatch(typeof(UIBattleResultLeftPanel), nameof(UIBattleResultLeftPanel.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> LostBooksRemove(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            CodeMatcher codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Ldarg_0, OpCodes.Ldfld)
                .RemoveInstructions(19);

            return codeMatcher.Instructions();
        }



        // BattleUnitModel patch. Replace BattleUnitModel.OnDie mechanism of giving books on enemy death with a custom one. Gives only one book from the pool of every book that should drop from enemy //
        [HarmonyPatch(typeof(BattleUnitModel), nameof(BattleUnitModel.OnDie))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EnemyBookDropLimit(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Call, OpCodes.Callvirt, OpCodes.Stloc_S, OpCodes.Ldloc_S)
                .RemoveInstructions(47)
                .Insert(Transpilers.EmitDelegate<Action<BattleUnitModel>>((unit) => {
                    var drops = unit.UnitData.unitData.DropTable.Select(d => d.Value).SelectMany(t => t.Ids).Where(id => !PlaythruManager.FoundBooks.Contains(id.id)).Distinct().ToList();

                    if (drops.Count > 0)
                    {
                        DropBookDataForAddedReward drop = new DropBookDataForAddedReward(drops.ElementAt(new System.Random().Next(drops.Count)));

                        // Also mark the book as found
                        PlaythruManager.FoundBooks.Add(drop.id.id);

                        StageController.Instance.OnEnemyDropBookForAdded(drop);
                        unit.view.OnEnemyDropBook(drop.GetLorId());
                    }
                }));

            return codeMatcher.Instructions();
        }



        // BattleEmotionRewardSlotUI patch. Show what books enemies still didn't drop. //
        [HarmonyPatch(typeof(BattleEmotionRewardSlotUI), nameof(BattleEmotionRewardSlotUI.SetData))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> BookDropInfo(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Ldloc_0, OpCodes.Stloc_S)
                .RemoveInstructions(168)
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1)) // Load current i into stack
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_0)) // Load self into stack
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldarg_1)) // Load unit into stack
                .Insert(Transpilers.EmitDelegate<Action<int, BattleEmotionRewardSlotUI, UnitBattleDataModel>>((i, slot, unit) =>
                {
                    var drops = unit.unitData.DropTable.Select(d => d.Value).SelectMany(t => t.Ids).Where(id => !PlaythruManager.FoundBooks.Contains(id.id)).Distinct().ToList();

                    for (int k = 0; k < drops.Count; k++)
                    {
                        if (slot.rewardtexts.Count <= i)
                            break;

                        slot.rewardtexts[i].text = $"{Singleton<DropBookXmlList>.Instance.GetData(drops[k]).Name} - 1 Copy";
                        slot.rewardtexts[i].gameObject.SetActive(true);
                        slot.SetSizeByText(slot.rewardtexts[i]);
                    }
                }))
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldloc_1)) // Load current i into stack
                .InsertAndAdvance(new CodeInstruction(OpCodes.Ldc_I4_1)) // Load 1 into stack
                .InsertAndAdvance(new CodeInstruction(OpCodes.Add)) // Add 1 to i
                .InsertAndAdvance(new CodeInstruction(OpCodes.Stloc_1)); // Save i from stack

            return codeMatcher.Instructions();
        }



        // EmotionCardAbility_freischutz1 patch. Patch Request abno page to not give bonus books. //
        [HarmonyPatch(typeof(EmotionCardAbility_freischutz1), nameof(EmotionCardAbility_freischutz1.OnKill))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> RequestNoBonusBooks(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Ldarg_1, OpCodes.Callvirt, OpCodes.Callvirt, OpCodes.Stloc_0)
                .SetAndAdvance(OpCodes.Nop, null)
                .RemoveInstructions(41);

            return codeMatcher.Instructions();
        }



        // EmotionCardAbility_whitenight2 patch. Patch Sentinel abno page to not give bonus books. //
        [HarmonyPatch(typeof(EmotionCardAbility_whitenight2), nameof(EmotionCardAbility_whitenight2.OnBattleEnd_alive))]
        [HarmonyPrefix]
        static bool SentinelNoBonusBooks() => false;



        // StageLibraryFloorModel patch. Make Angela replace any Patron Librarian for Keter Realization. //
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



        // UIBattleSettingPanel patch. Return Angela her light in the battle prepare screen. //
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



        // UIMainPanel patch. Check what abno is next when clicking on !. //
        // Fully overriden because too many patches with changes needed to get desired result.
        [HarmonyPatch(typeof(UIMainPanel), nameof(UIMainPanel.OnClickLevelUp))]
        [HarmonyPrefix]
        static bool ClickSuppression(UIMainPanel __instance, int index)
        {
            LibraryFloorModel floor = LibraryModel.Instance.GetFloor((SephirahType)(index + 1));
            FloorLevelXmlInfo data = FloorLevelXmlList.Instance.GetData(floor.Sephirah, floor.GetCurrentAbnoStage());

            if (data == null)
                return false;

            string stageName = "";
            UIAlarmType alarmtype = UIAlarmType.StartCreatureBattle;
            if ((floor.Sephirah == SephirahType.Binah || floor.Sephirah == SephirahType.Hokma) && floor.GetCurrentAbnoStage() >= 4 || floor.GetCurrentAbnoStage() >= 5)
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
                stageName = TextDataModel.GetText(id);
                alarmtype = UIAlarmType.StartCreatureBattleInBoss;
            }
            else
            {
                StageClassInfo data2 = StageClassInfoList.Instance.GetData(data.stageId);
                if (data2 != null)
                    stageName = StageNameXmlList.Instance.GetName(data2);
            }

            UIAlarmPopup.instance.SetAlarmText(alarmtype, UIAlarmButtonType.YesNo, delegate (bool b)
            {
                if (!b)
                    return;

                UI.UIController UIController = UI.UIController.Instance;
                StageController StageController = StageController.Instance;

                UI.UIController.Instance.SetCurrentSephirah(floor.Sephirah);

                if (data.stageId >= 210005 && data.stageId <= 210009)
                {
                    // StartEndContentsStage
                    StageClassInfo stageInfo = StageClassInfoList.Instance.GetData(data.stageId);

                    if (stageInfo == null)
                        return;

                    UIController.SetStageInfo(stageInfo);

                    // InitStageForKeterCompleteOpen
                    StageController.InitStageByCreature(stageInfo);
                    StageController.firstStartState = true;
                    StageController._isEndContentsStage = true;

                    StageLibraryFloorModel _floor = StageController._stageModel.GetFloor(floor.Sephirah);
                    if (_floor == null)
                        return;

                    StageController.SetCurrentSephirah(SephirahType.Keter);

                    int num = 0;
                    foreach (UnitBattleDataModel unitBattleData in _floor.GetUnitBattleDataList())
                    {
                        unitBattleData.IsAddedBattle = false;
                        if (!unitBattleData.isDead && num < StageController._stageModel.GetWave(StageController._currentWave).AvailableUnitNumber)
                        {
                            unitBattleData.IsAddedBattle = true;
                            num++;
                        }
                    }

                    // Continuation of StartEndContentsStage
                    UIController.OpenBattlePrepare();
                }
                else
                {
                    // OnClickStartCreatureStage
                    StageController.SetCurrentSephirah(floor.Sephirah);

                    StageClassInfo stageInfo = StageClassInfoList.Instance.GetData(data.stageId);

                    if (stageInfo == null)
                        return;

                    StageController.InitStageByCreature(stageInfo);

                    foreach (UnitBattleDataModel unitBattleData in StageController.GetCurrentStageFloorModel().GetUnitBattleDataList())
                    {
                        if (floor.Sephirah == SephirahType.Binah && LibraryModel.Instance.IsBinahLockedInLibrary() && unitBattleData.unitData.isSephirah)
                        {
                            unitBattleData.IsAddedBattle = false;
                        }
                        else
                        {
                            unitBattleData.IsAddedBattle = true;
                        }
                    }

                    GlobalGameManager.Instance.LoadBattleScene();
                }
            }, stageName);

            return false;
        }



        // EnemyTeamStageManager_TheCrying patch. Limit books given from crying children reception. //
        [HarmonyPatch(typeof(EnemyTeamStageManager_TheCrying), nameof(EnemyTeamStageManager_TheCrying.OnStageClear))]
        [HarmonyPrefix]
        static bool CryingChildrenBooks(EnemyTeamStageManager_TheCrying __instance)
        {
            if (PlaythruManager.FoundBooks.Contains(240023))
                return false;

            DropBookDataForAddedReward drop = new DropBookDataForAddedReward(240023);

            // Also mark the book as found
            PlaythruManager.FoundBooks.Add(drop.id.id);

            StageController.Instance.OnEnemyDropBookForAdded(drop);

            return false;
        }



        // UIRewardDropBookList patch. Hide already found books from reception description. //
        [HarmonyPatch(typeof(UIRewardDropBookList), nameof(UIRewardDropBookList.SetData))]
        [HarmonyPrefix]
        static bool ResolveableRewardsPatch(UIRewardDropBookList __instance, ref List<LorId> bookids)
        {
            bookids = bookids.Where(id => !PlaythruManager.FoundBooks.Contains(id.id)).ToList();

            return true;
        }



        // LibraryModel patches. Check if there is an available abno fight or a realization. //
        // Check if player should be doing a realization
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.CheckCreatureBossBattle))]
        [HarmonyPrefix]
        static bool CheckRealization(LibraryModel __instance, LibraryFloorModel floor, ref bool __result)
        {
            __result = (floor.Sephirah == SephirahType.Binah || floor.Sephirah == SephirahType.Hokma) && floor.GetCurrentAbnoStage() >= 4 || floor.GetCurrentAbnoStage() >= 5;

            return false;
        }

        // Check what abno player should fight
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.CanLevelUpSephirah))]
        [HarmonyPrefix]
        static bool CheckSuppression(LibraryModel __instance, SephirahType sep, ref bool __result)
        {
            __result = false;

            if (!__instance._openedSephirah.Contains(sep)) 
                return false;

            __result = FloorLevelXmlList.Instance.GetData(sep.FloorModel().Sephirah, sep.FloorModel().GetCurrentAbnoStage()) != null;

            return false;
        }



        // UIController patch. Make UI show what abno/realization is to fight. //
        [HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.OnClickStartCreatureStage))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> UIAbnoOrRealizationPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Callvirt, OpCodes.Stloc_0, OpCodes.Call, OpCodes.Ldarg_1)
                .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(LORClassExtensions), nameof(LORClassExtensions.GetCurrentAbnoStage))));

            return codeMatcher.Instructions();
        }

        // Change "Leave" button on floor selection when reception starts
        [HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.BackBattlePrepare))]
        [HarmonyPrefix]
        static bool ReplaceButton()
        {
            UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, (bool yes) =>
            {
                if (!yes) return;

                StageController.Instance.GameOver(false, true);
                GameSceneManager.Instance.ActivateUIController();
                UIBgScreenChangeAnim.Instance.StartBg(UIScreenChangeType.BackInvitation);
            });

            UIAlarmPopup.instance.txt_alarm.text = "Are you sure you want to forfeit the battle?";

            return false;
        }



        // UIEscPanel patch. Add "Forfeit" and change "Return to Title" behaviour. //
        // Change "Manual" to "Forfeit"
        [HarmonyPatch(typeof(UIEscPanel), nameof(UIEscPanel.Open))]
        [HarmonyPostfix]
        static void EscMenuButtonRename(UIEscPanel __instance)
        {
            __instance.buttons.ElementAt(1).GetComponentInChildren<TextMeshProUGUI>().text = "Forfeit";
        }

        // Make "Forfeit" button disabled if Esc menu is opened when not in battle
        [HarmonyPatch(typeof(UIEscPanel), nameof(UIEscPanel.Open))]
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

        // Make clicking on "Forfeit" ask if you really want to forfet the battle. Also when leaving asking if sure for leaving to title
        [HarmonyPatch(typeof(UIEscPanel), nameof(UIEscPanel.OnClickEvent))]
        [HarmonyTranspiler]
        static IEnumerable<CodeInstruction> EscapeMenuPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        {
            var codeMatcher = new CodeMatcher(instructions, generator);

            codeMatcher.MatchStartForward(OpCodes.Call, OpCodes.Ldc_I4_2, OpCodes.Callvirt)
                .ThrowIfInvalid("Couldn't find Instrcutions.")
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
                .ThrowIfInvalid("Couldn't find Instrcutions. (2)")
                .CreateLabel(out Label noPenalty)
                .Start().MatchStartForward(OpCodes.Brtrue)
                .ThrowIfInvalid("Couldn't find Instrcutions. (3)")
                .SetOperandAndAdvance(noPenalty);

            return codeMatcher.Instructions();
        }



        // StageClassInfo patch. Check if player is able to access a reception. //
        [HarmonyPatch(typeof(StageClassInfo), nameof(StageClassInfo.currentState), MethodType.Getter)]
        [HarmonyPrefix]
        static bool CheckReceptionAvailability(StageClassInfo __instance, ref StoryState __result)
        {
            __result = StoryState.Close;

            if (PlaythruManager.IsReceptionOpened(__instance.id.id))
                __result = StoryState.Clear;

            return false;
        }



        // UIStoryProgressPanel patch. Update map. //
        [HarmonyPatch(typeof(UIStoryProgressPanel), nameof(UIStoryProgressPanel.SetStoryLine))]
        [HarmonyPrefix]
        static bool MapUpdate(UIStoryProgressPanel __instance)
        {
            __instance.currentSlot = null;
            StoryTotal.instance.SetData(); // What's it for?
            __instance.chapterList.ForEach(c => c.SetActive(true)); // Show all chapters and receptions
            __instance.blockChapterList.ForEach(b => b.root.gameObject.SetActive(false)); // Hide all chapter block things

            List<int> ensembleIds = new List<int>() { 70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010 };
            List<int> hideIDs = new List<int>() { 610000, 60007 };
            foreach (var icon in __instance.iconList) // Set all receptions info and icons
            {
                List<StageClassInfo> storyData = StoryTotal.instance._lineList.Find((StoryLineData x) => x.currentstory == icon.currentStory)?.stageList ?? icon.storyData;
                icon.SetSlotData(storyData);

                if (hideIDs.Contains(storyData[0]._id)) // Hide some receptions
                    icon.SetActiveStory(false);
                else
                    icon.SetActiveStory(true);

                if (ensembleIds.Contains(storyData[0]._id)) // Set custom ensemble icons
                {
                    if (storyData[0].currentState == StoryState.Clear)
                        icon.SetIcon(UISpriteDataManager.instance._floorIconSet[storyData[0]._id - 70000]);
                    else
                        icon.SetIcon(UISpriteDataManager.instance._questionicon[1]);
                }


                // Show checkmark for the completed receptions that have every book collected
                if (icon.transform.Find("Checkmark") == null)
                    continue;

                var notFound = storyData.SelectMany(s => s.waveList).SelectMany(w => w.enemyUnitIdList).SelectMany(u => EnemyUnitClassInfoList.Instance.GetData(u).dropTableList).SelectMany(t => t.dropItemList).Where(i => !PlaythruManager.FoundBooks.Contains(i.bookId)).Count();

                if (PlaythruManager.ReceptionsCompleted.Contains(storyData[0]._id) && notFound == 0)
                    icon.transform.Find("Checkmark").gameObject.SetActive(true);
            }

            foreach (UIStoryProgressIconSlot chapterIcon in __instance.chapterIconList) // Make chapter buttons not interactable (and some other default stuff)
            {
                chapterIcon.SetChapterStoryIcon();
                chapterIcon.SetChapterStoryIconDefault();
                chapterIcon.enabled = false;
                chapterIcon.isDisabled = true;
                chapterIcon.transform.Find("[Rect]ChapterTitle/[Rect]Close (1)/[Xbox]SelectableTarget").gameObject.GetComponent<UICustomSelectable>().interactable = false;
            }

            return false;
        }



        // UIStoryProgressIconSlot patches. //
        // Change Ensemble receptions icons highlights
        [HarmonyPatch(typeof(UIStoryProgressIconSlot), nameof(UIStoryProgressIconSlot.SetHighlighted))]
        [HarmonyPrefix]
        static bool EnsembleIcons(UIStoryProgressIconSlot __instance, bool on)
        {
            Dictionary<UIStoryLine, Color> CustomDefaultColors = new Dictionary<UIStoryLine, Color>()
            {
                [(UIStoryLine)151] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)152] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)153] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)154] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)155] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)156] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)157] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)158] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)159] = new Color(0.8f, 0.8f, 0.8f, 1),
                [(UIStoryLine)160] = new Color(0.8f, 0.8f, 0.8f, 1),
            };

            Dictionary<UIStoryLine, Color> CustomHighlightColors = new Dictionary<UIStoryLine, Color>()
            {
                [(UIStoryLine)151] = new Color(1, 1, 1, 1),
                [(UIStoryLine)152] = new Color(1, 1, 1, 1),
                [(UIStoryLine)153] = new Color(1, 1, 1, 1),
                [(UIStoryLine)154] = new Color(1, 1, 1, 1),
                [(UIStoryLine)155] = new Color(1, 1, 1, 1),
                [(UIStoryLine)156] = new Color(1, 1, 1, 1),
                [(UIStoryLine)157] = new Color(1, 1, 1, 1),
                [(UIStoryLine)158] = new Color(1, 1, 1, 1),
                [(UIStoryLine)159] = new Color(1, 1, 1, 1),
                [(UIStoryLine)160] = new Color(1, 1, 1, 1),
            };

            if (!CustomHighlightColors.ContainsKey(__instance.currentStory)) return true;

            var isChapterIcon = __instance.isChapterIcon;
            var originalcolor = __instance.originalcolor;

            var highlightColor = CustomHighlightColors[__instance.currentStory];
            var defaultColor = CustomDefaultColors[__instance.currentStory];


            Color color = ((!isChapterIcon) ? originalcolor : (on ? highlightColor : defaultColor));
            Color color2 = (on ? highlightColor : UIColorManager.Manager.DefaultGlowColor);

            __instance.transform.Find("[Rect]Close/[Rect]Icon/[Image]Icon_content").gameObject.GetComponent<Image>().color = color;
            __instance.transform.Find("[Rect]Close/[Rect]Icon/[Image]Icon_bg").gameObject.GetComponent<Image>().color = color2;
            __instance.transform.Find("[Rect]Close/[Rect]Icon/[Image]Icon_Frame").gameObject.GetComponent<Image>().color = color2;

            __instance.transform.Find("[Rect]Open/[Rect]OpenIcon/[Image]Icon_content").gameObject.GetComponent<Image>().color = (isChapterIcon ? defaultColor : originalcolor);
            __instance.transform.Find("[Rect]Open/[Rect]OpenIcon/[Image]Icon_bg").gameObject.GetComponent<Image>().color = UIColorManager.Manager.DefaultGlowColor;

            return false;
        }



        // UIInvitationRightMainPanel patches. Make receptions not require books. //
        // Hide "Workshop" checkbox (Can't set custom recipes anyway)
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.OpenInit))]
        [HarmonyPostfix]
        static void NoWorkshop(UIInvitationRightMainPanel __instance)
        {
            __instance.ob_customMode.gameObject.SetActive(false);
        }

        // Set the UI Red as if all the needed books are selected, also make books unable to be selected
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.SetInvBookApplyState))]
        [HarmonyPrefix]
        static bool FakeSelectedBooks(UIInvitationRightMainPanel __instance, ref InvitationApply_State state)
        {
            if (state == InvitationApply_State.Normal || state == InvitationApply_State.Fixed)
            {
                __instance.currentinvState = state;
                __instance.SetActiveEndEffect(on: false);
                __instance.invitationbookSlots.ForEach(s => s.SetDisabledSlot());
                __instance.SetUpdatePanel();

                return false;
            }

            return true;
        }

        // Make "Send Invitation" button clickable. Next Patch is related
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.SetSendButton))]
        [HarmonyPrefix]
        static bool SendInvitationClickable(UIInvitationRightMainPanel __instance)
        {
            __instance.button_SendButton.gameObject.SetActive(value: true);
            __instance.confirmAreaRoot.SetActive(value: false);

            __instance.ispossibleSend = __instance.invPanel.CurrentStage != null && __instance.invPanel.CurrentApplyState != InvitationApply_State.Normal;
            __instance.ButtonFrameHighlight.enabled = __instance.ispossibleSend;
            __instance.button_SendButton.interactable = __instance.ispossibleSend;
            __instance.SetColorAllFrames(__instance.ispossibleSend ? __instance.Color_Selectedcolor : UIColorManager.Manager.GetUIColor(UIColor.Default));
            __instance.SetColorInvitationSlots(__instance.ispossibleSend ? __instance.Color_Selectedcolor : UIColorManager.Manager.GetUIColor(UIColor.Default));

            return false;
        }

        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.SendInvitation))]
        [HarmonyPrefix]
        static bool SendButtonClickable(UIInvitationRightMainPanel __instance)
        {
            if (__instance.GetBookRecipe() != null)
                __instance.confirmAreaRoot.SetActive(value: true);

            return false;
        }


        // Make game think player has selected all the needed books. Next Patch is related, it's for general receptions
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.GetAppliedBookModel))]
        [HarmonyPrefix]
        static bool FakeMoreBooks(UIInvitationRightMainPanel __instance, ref List<DropBookXmlInfo> __result)
        {
            if (__instance.invPanel.CurrentStage == null || __instance.invPanel.CurrentApplyState == InvitationApply_State.Normal)
                return true;

            __result = __instance.invPanel.CurrentStage.invitationInfo.needsBooks.Select(id => DropBookXmlList.Instance.GetData(id)).ToList();

            return false;
        }

        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.GetBookRecipe))]
        [HarmonyPrefix]
        static bool FakeEvenMoreBooks(UIInvitationRightMainPanel __instance, ref StageClassInfo __result)
        {
            var cur = __instance.invPanel.CurrentStage;
            if (cur != null)
            {
                __result = cur;

                return false;
            }

            return true;
        }
    }
}
