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
using static StageController;
using Archipelago.MultiClient.Net.Enums;
using LORAP.Gameplay;
using LORAP.Utils;
using LORAP.CustomUI;

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
                            LocationManager.SendStageChecks(stageId);

                            // PlaythruManager.CheckEndConditions();

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

                if (won && !PlaythruManager.ReceptionsCompleted.Contains(stageModel.ClassInfo._id))
                {
                    // Clear check here
                    PlaythruManager.ReceptionsCompleted.Add(stageModel.ClassInfo._id);

                    if (!SlotDataManager.EnemiesTurnIntoChecks)
                        LocationManager.SendStageChecks(stageModel.ClassInfo._id);

                    //PlaythruManager.CheckEndConditions();
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
                        MessagePopup.Open();
                        AbnoEgoPagePopup.Open();

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
                    if (SlotDataManager.EnemiesTurnIntoChecks && StageController.Instance.stageType == StageType.Invitation)
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
            if (faction == Faction.Player)
                return true;

            foreach (BattleEmotionRewardSlotUI s in __instance.slots)
            {
                s.gameObject.SetActive(false);
            }
            __instance.slots[0].gameObject.SetActive(true);

            int stageId = StageController.Instance._stageModel.ClassInfo.id.id;

            List<long> locations = LocationManager.GetUncheckedReceptionLocations(stageId);

            BattleEmotionRewardSlotUI slot = __instance.slots.First();

            slot.txt_Name.text = locations.Count > 0 ? $"Items remaining: {locations.Count}" : "All items collected!";
            slot.img_emotionlevel.sprite = UISpriteDataManager.instance.EmotionLevelIcon[locations.Count < 6 ? locations.Count : 5];

            // Set texts
            for (int i = 0; i < slot.rewardtexts.Count; i++) // TODO: Increase number of reward texts to 10 (vanilla is 4)
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



        // Patch Request abno page to not give bonus books.
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



        // Patch Sentinel abno page to not give bonus books. I HATE free stuff.
        [HarmonyPatch(typeof(EmotionCardAbility_whitenight2), nameof(EmotionCardAbility_whitenight2.OnBattleEnd_alive))]
        [HarmonyPrefix]
        static bool SentinelNoBonusBooks() => false;



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



        // Check what abno is next when clicking on !.
        // Fully overriden because too many patches with changes needed to get desired result.
        [HarmonyPatch(typeof(UIMainPanel), nameof(UIMainPanel.OnClickLevelUp))]
        [HarmonyPrefix]
        static bool ClickSuppression(UIMainPanel __instance, int index)
        {
            SephirahType seph = (SephirahType)(index + 1);

            FloorLevelXmlInfo data = FloorLevelXmlList.Instance.GetData(seph, seph.GetCurrentAbnoStage());

            if (data == null)
                return false;

            List<int> bossStages = new List<int>()
            {
                201005, 202005, 203005, 204005, 205005, 206005, 207005, 208005, 209005, 210005, 210006, 210007, 210008, 210009
            };

            string stageName = "";
            UIAlarmType alarmtype = UIAlarmType.StartCreatureBattle;

            if (bossStages.Contains(data.stageId))
            {
                stageName = TextDataModel.GetText(seph.FloorTextId());
                alarmtype = UIAlarmType.StartCreatureBattleInBoss;
            }
            else
            {
                StageClassInfo data2 = StageClassInfoList.Instance.GetData(data.stageId);
                if (data2 != null)
                    stageName = StageNameXmlList.Instance.GetName(data2);
            }

            UIAlarmPopup.instance.SetAlarmText(alarmtype, UIAlarmButtonType.YesNo, delegate (bool b) // TODO: Refactor?
            {
                if (!b)
                    return;

                UI.UIController UIController = UI.UIController.Instance;
                StageController StageController = StageController.Instance;

                UI.UIController.Instance.SetCurrentSephirah(seph);

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

                    StageLibraryFloorModel _floor = StageController._stageModel.GetFloor(seph);
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
                    StageController.SetCurrentSephirah(seph);

                    StageClassInfo stageInfo = StageClassInfoList.Instance.GetData(data.stageId);

                    if (stageInfo == null)
                        return;

                    StageController.InitStageByCreature(stageInfo);

                    foreach (UnitBattleDataModel unitBattleData in StageController.GetCurrentStageFloorModel().GetUnitBattleDataList())
                    {
                        if (seph == SephirahType.Binah && LibraryModel.Instance.IsBinahLockedInLibrary() && unitBattleData.unitData.isSephirah)
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



        // Don't give crying children books after the reception
        [HarmonyPatch(typeof(EnemyTeamStageManager_TheCrying), nameof(EnemyTeamStageManager_TheCrying.OnStageClear))]
        [HarmonyPrefix]
        static bool CryingChildrenBooks() => false;



        // Check if there is an available abno fight or a realization.
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.CheckCreatureBossBattle))]
        [HarmonyPrefix]
        static bool CheckRealization(LibraryModel __instance, LibraryFloorModel floor, ref bool __result)
        {
            __result = false;

            if (!floor.Sephirah.IsOpen())
                return false;

            List<int> bossStages = new List<int>()
            {
                201005, 202005, 203005, 204005, 205005, 206005, 207005, 208005, 209005, 210005, 210006, 210007, 210008, 210009
            };

            FloorLevelXmlInfo data = FloorLevelXmlList.Instance.GetData(floor.Sephirah, floor.GetCurrentAbnoStage());

            // Check if player has all the required books
            List<int> books = SlotDataManager.AbnoBookRequirements[floor.Sephirah][floor.GetCurrentAbnoStage() - 1];
            bool hasBooks = true;

            foreach (int book in books)
            {
                if (DropBookInventoryModel.Instance.GetBookCount(book) == 0)
                {
                    hasBooks = false;
                    break;
                }
            }

            __result = bossStages.Contains(data.stageId) && hasBooks;

            return false;
        }



        // Check what abno player should fight
        [HarmonyPatch(typeof(LibraryModel), nameof(LibraryModel.CanLevelUpSephirah))]
        [HarmonyPrefix]
        static bool CheckSuppression(LibraryModel __instance, SephirahType sep, ref bool __result)
        {
            __result = false;

            if (!sep.IsOpen()) 
                return false;

            // Check if player has all the required books
            List<int> books = SlotDataManager.AbnoBookRequirements[sep][sep.GetCurrentAbnoStage() - 1];
            bool hasBooks = true;

            foreach (int book in books)
            {
                if (DropBookInventoryModel.Instance.GetBookCount(book) == 0)
                {
                    hasBooks = false;
                    break;
                }
            }

            __result = FloorLevelXmlList.Instance.GetData(sep, sep.GetCurrentAbnoStage()) != null && hasBooks;

            return false;
        }



        // Make UI show what abno/realization is to fight.
        //[HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.OnClickStartCreatureStage))]
        //[HarmonyTranspiler]
        //static IEnumerable<CodeInstruction> UIAbnoOrRealizationPatch(IEnumerable<CodeInstruction> instructions, ILGenerator generator)
        //{
        //    var codeMatcher = new CodeMatcher(instructions, generator);
        //
        //    codeMatcher.MatchStartForward(OpCodes.Callvirt, OpCodes.Stloc_0, OpCodes.Call, OpCodes.Ldarg_1)
        //        .SetInstruction(new CodeInstruction(OpCodes.Call, AccessTools.Method(typeof(ClassExtensions), nameof(ClassExtensions.GetCurrentAbnoStage))));
        //
        //    return codeMatcher.Instructions();
        //}



        // Change "Leave" button on floor selection when reception starts
        [HarmonyPatch(typeof(UI.UIController), nameof(UI.UIController.BackBattlePrepare))]
        [HarmonyPrefix]
        static bool ReplaceLeaveButton()
        {
            UIAlarmPopup.instance.SetAlarmText(UIAlarmType.ReturnToTitleWarn_NoPenalty, UIAlarmButtonType.YesNo, (bool yes) =>
            {
                if (!yes) return;

                StageController.Instance.GameOver(false, true);
                GameSceneManager.Instance.ActivateUIController();
                UIBgScreenChangeAnim.Instance.StartBg(UIScreenChangeType.BackInvitation);
            });

            UIAlarmPopup.instance.txt_alarm.text = "Are you sure you want to leave?";

            return false;
        }



        // Change "Manual" button text to "Forfeit"
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



        // Set receptions availability/state based on what receptions were completed
        [HarmonyPatch(typeof(StageClassInfo), nameof(StageClassInfo.currentState), MethodType.Getter)]
        [HarmonyPrefix]
        static bool GetReceptionAvailability(StageClassInfo __instance, ref StoryState __result)
        {
            __result = StoryState.Close;

            // If there is any hint for this location, show it
            if (LocationManager.KnownHints.Any(h => LocationManager.GetReceptionIdFromLocationId(h.LocationId) == __instance.id.id))
                __result = StoryState.Clear;

            // If any of the previous locations was cleared, show it
            List<ReceptionNode> prev = SlotDataManager.ReceptionTree.GetPrevNodes(__instance.id.id);

            if (prev.Count == 0 || prev.Any(n => PlaythruManager.ReceptionsCompleted.Contains(n.id)))
                __result = StoryState.Clear;

            return false;
        }



        // UIStoryProgressPanel patch. Update map. //
        [HarmonyPatch(typeof(UIStoryProgressPanel), nameof(UIStoryProgressPanel.SetStoryLine))]
        [HarmonyPrefix]
        static bool MapUpdate(UIStoryProgressPanel __instance)
        {
            // TODO:
            // Hide chapter lines if either randomizing tree or no reception of chapter yet gotten to
            __instance.currentSlot = null;
            StoryTotal.instance.SetData(); // Get all the recipes and receptions
            __instance.blockChapterList.ForEach(b => b.root.gameObject.SetActive(false)); // Hide all chapter block things
            __instance.chapterList.ForEach(c => c.SetActive(true)); // Show all chapters (groups of receptions)

            if (SlotDataManager.RandomizeReceptionTree) // For randomized reception tree or Unlocked/Books progression
            {
                // Hide all chapter lines
                foreach (var chapter in __instance.chapterIconList)
                {
                    chapter.gameObject.SetActive(false);
                }
            }

            List<int> hideIDs = new List<int>() { 60007 };
            foreach (var icon in __instance.iconList) // Set all receptions info and icons
            {
                List<StageClassInfo> storyData = SlotDataManager.RandomizeReceptionTree ? icon.storyData : StoryTotal.instance._lineList.Find((StoryLineData x) => x.currentstory == icon.currentStory)?.stageList ?? icon.storyData;

                icon.SetSlotData(storyData);

                // Hide some receptions
                icon.SetActiveStory(!hideIDs.Contains(storyData[0]._id));

                /*if (SlotDataManager.RandomizeReceptionTree) // For randomized reception tree
                {
                    icon.SetSlotData(icon.storyData);
                    icon.SetActiveStory(true);
                }
                else // For Vanilla reception tree
                {
                    icon.SetSlotData(storyData);

                    // Hide some receptions
                    icon.SetActiveStory(hideIDs.Contains(storyData[0]._id));
                }*/

                // Show checkmark for completed receptions
                if (icon.transform.Find("Checkmark") == null)
                    continue;

                if (storyData.All(d => PlaythruManager.ReceptionsCompleted.Contains(d._id)))
                  icon.transform.Find("Checkmark").gameObject.SetActive(true);


                // Show checkmark for the completed receptions that have every book collected
                //if (icon.transform.Find("Checkmark") == null)
                //    continue;

                //var notFound = storyData.SelectMany(s => s.waveList).SelectMany(w => w.enemyUnitIdList).SelectMany(u => EnemyUnitClassInfoList.Instance.GetData(u).dropTableList).SelectMany(t => t.dropItemList).Where(i => !PlaythruManager.FoundBooks.Contains(i.bookId)).Count();

                //if (PlaythruManager.ReceptionsCompleted.Contains(storyData[0]._id) && notFound == 0)
                //  icon.transform.Find("Checkmark").gameObject.SetActive(true);
            }


            // TODO: Find a better way?
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



        // UIInvitationStageInfoPanel patch. Show AP items in "Resolvable Rewards"
        [HarmonyPatch(typeof(UIInvitationStageInfoPanel), nameof(UIInvitationStageInfoPanel.SetData))]
        [HarmonyPostfix]
        static void OnSelectStage(UIInvitationStageInfoPanel __instance, StageClassInfo stage, UIStoryLine story = UIStoryLine.None)
        {
            List<long> receptionLocations = LocationManager.GetUncheckedReceptionLocations(stage._id);
            List<long> locationsWithHints = receptionLocations.Where(l => LocationManager.KnownHints.Any(h => !h.Found && h.LocationId == l)).ToList();

            // Scout all unchecked locations (they're already known, but this time we scout for hints.) // TODO: Make an option to toggle the hint scouting
            // If this recepion is visible because of a hint, don't scout.
            if (locationsWithHints.Count == 0)
                SessionManager.Locations.ScoutLocationsAsync(HintCreationPolicy.CreateAndAnnounceOnce, receptionLocations.ToArray());

            // Fill the item list
            for (int i = 0; i < 8; i++)
            {
                UIBookSlot slot = __instance.rewardBookList.bookSlotList[i];
                if (i >= receptionLocations.Count)
                {
                    slot.SetActivatedSlot(false);
                    continue;
                }

                slot.SetActivatedSlot(true);
                slot.gameObject.SetActive(true);
                slot.isDisabled = false;

                if (i == 7 && receptionLocations.Count > 8)
                {
                    slot.BookName.text = $"+{receptionLocations.Count - 7} more items!";
                    slot.Icon.sprite = UIUtils.FillerSprite;
                }
                else
                {
                    if (locationsWithHints.Count == 0 || locationsWithHints.Contains(receptionLocations[i]))
                    {
                        ItemLocationPair pair = LocationManager.GetLocationPair(receptionLocations[i]);
                        slot.BookName.text = LocationManager.FormatPairItem(pair);

                        if (pair.Item.Flags.HasFlag(ItemFlags.Advancement))
                            slot.Icon.sprite = UIUtils.ProgSprite;
                        else if (pair.Item.Flags.HasFlag(ItemFlags.NeverExclude))
                            slot.Icon.sprite = UIUtils.UsefulSprite;
                        else
                            slot.Icon.sprite = UIUtils.FillerSprite;
                    }
                    else
                    {
                        slot.BookName.text = "???";
                        slot.Icon.sprite = UIUtils.FillerSprite;
                    }
                }

                slot.IconGlow.enabled = false;
                slot.SetHighlighted(false);
                slot.originSiblingIdx = slot.transform.GetSiblingIndex();
            }
        }



        // Hide "Workshop" checkbox
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.OpenInit))]
        [HarmonyPostfix]
        static void NoWorkshopReceptions(UIInvitationRightMainPanel __instance)
        {
            __instance.ob_customMode.gameObject.SetActive(false);
            __instance._workshopInvitationToggle.isOn = false;
        }



        // Make receptions without book requirements be able to be started  // TODO: Disallow starting reception without completing atleast one of previous receptions
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.GetBookRecipe))] // TODO: Disable showing reception by putting books without completing one of previous receptions
        [HarmonyPrefix]
        static bool FakeBooks(UIInvitationRightMainPanel __instance, ref StageClassInfo __result)
        {
            var cur = __instance.invPanel.CurrentStage;
            if (cur != null && __instance.invPanel.currentSelectedStorySlot != null && cur == __instance.invPanel.currentSelectedStorySlot.storyData[__instance.invPanel.currentStoryidx] && cur.invitationInfo.needsBooks.Count == 0)
            {
                __result = cur;

                return false;
            }

            return true;
        }


        // Make amount of books for invitations show as infinite
        [HarmonyPatch(typeof(UIInvitationDropBookSlot), nameof(UIInvitationDropBookSlot.SetData_DropBook))]
        [HarmonyPostfix]
        static void FakeInfBooksForInvitation(UIInvitationDropBookSlot __instance, LorId bookId)
        {
            __instance.txt_bookNum.text = "∞";
        }
    }
}
