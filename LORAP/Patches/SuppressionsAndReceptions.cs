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
using System.Runtime.Remoting.Contexts;
using TMPro;
using UI;
using UnityEngine;
using UnityEngine.UI;
using static StageController;

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

            List<long> locations = LocationManager.GetUncheckedStageLocations(stageId);

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



        // Set battle-node visual state. Access is enforced separately by AP recipe/stage-start checks.
        [HarmonyPatch(typeof(StageClassInfo), nameof(StageClassInfo.currentState), MethodType.Getter)]
        [HarmonyPrefix]
        static bool GetBattleNodeAvailability(StageClassInfo __instance, ref StoryState __result)
        {
            //__result = StoryState.Clear;
            //return false;

            int stageId = __instance._id;
            BattleNode battleNode = SlotDataManager.BattleTree?.GetNodeById(stageId);

            if (battleNode == null)
            {
                __result = StoryState.Close;
                return false;
            }

            List<int> endgoals = new List<int>()
            {
                60003, 60004,
                70001, 70002, 70003, 70004, 70005, 70006, 70007, 70008, 70009, 70010
            };
            if (endgoals.Contains(battleNode.Id))
            {
                __result = StoryState.Clear;
                return false;
            }

            __result = battleNode.AreBattleParentsComplete() ? StoryState.Clear : StoryState.Close;
            return false;
        }



        // When player clicks on stage icon which is Black Silence or Distorted Ensemble, check if we should actually show anything or not
        [HarmonyPatch(typeof(UIStoryProgressIconSlot), nameof(UIStoryProgressIconSlot.ClickMainIcon))]
        [HarmonyPrefix]
        static bool DontOpenBSDE(UIStoryProgressIconSlot __instance)
        {
            if (__instance.currentStory == UIStoryLine.BlackSilence || __instance.currentStory == UIStoryLine.TwistedBlue)
            {
                if (__instance.storyData[0].currentState == StoryState.Close)
                    return false;
            }

            return true;
        }



        // Update the reception tree.
        [HarmonyPatch(typeof(UIStoryProgressPanel), nameof(UIStoryProgressPanel.SetStoryLine))]
        [HarmonyPrefix]
        static bool MapUpdate(UIStoryProgressPanel __instance)
        {
            __instance.currentSlot = null;

            // Set info for every icon
            foreach (var icon in __instance.iconList)
            {
                List<StageClassInfo> storyData = icon.storyData;

                // If it's one of the stages stage of keter realization, make it show the current realization stage player is at
                if (storyData[0]._id >= 210005 && storyData[0]._id <= 210009)
                    storyData = new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(210005 + PlaythruManager.KeterRealizationStage) };

                // If the stage is not on the randomized tree, hide it entirely
                BattleNode battleNode = SlotDataManager.BattleTree?.GetNodeById(storyData[0]._id);
                if (battleNode == null)
                {
                    icon.SetActiveStory(false);
                    continue;
                }

                icon.SetSlotData(storyData);

                // If it's a floor stage and it can be seen, set its icon 
                if (battleNode.Kind == BattleNodeKind.Stage && storyData[0].currentState == StoryState.Clear)
                    icon.SetIcon(UIUtils.GetFloorIconSet(storyData[0]._id, battleNode.AssignedFloor));

                if (storyData[0]._id >= 70001 && storyData[0]._id <= 70010)
                    icon.SetIcon(UISpriteDataManager.instance.floorIconSet[(int)storyData[0].floorOnlyList[0]]);

                // Show the icon on the map
                icon.SetActiveStory(true);

                // Show status of the stage (Complete/Clearable)
                if (icon.transform.Find("Status") == null)
                    continue;

                GameObject status = icon.transform.Find("Status").gameObject;

                // If the stage is completed and all checks are collected
                if (PlaythruManager.IsStageComplete(storyData[0]._id) && LocationManager.GetUncheckedStageLocations(storyData[0]._id).Count == 0)
                {
                    status.GetComponent<Image>().sprite = UIUtils.CheckmarkSprite;
                    status.SetActive(true);

                    continue;
                }

                // If the stage isn't completed but can be completed (player has all required books for it)
                if (storyData[0].currentState == StoryState.Clear && storyData[0].invitationInfo.needsBooks.All(b => DropBookInventoryModel.Instance.GetBookCount(b) > 0)
                    && (battleNode.Kind != BattleNodeKind.Stage || battleNode.AssignedFloor.IsOpen()))
                {
                    status.GetComponent<Image>().sprite = UIUtils.ExclamationSprite;
                    status.SetActive(true);

                    continue;
                }

                status.SetActive(false);
            }

            return false;
        }



        // Show AP items in "Resolvable Rewards", also set icon for floor stages
        [HarmonyPatch(typeof(UIInvitationStageInfoPanel), nameof(UIInvitationStageInfoPanel.SetData))]
        [HarmonyPostfix]
        static void OnSelectStage(UIInvitationStageInfoPanel __instance, StageClassInfo stage, UIStoryLine story = UIStoryLine.None)
        {
            // If it's one of the keter realization stages make sure to get the last one since it's the only one that technically has items
            List<long> receptionLocations = LocationManager.GetUncheckedStageLocations(stage._id >= 210005 && stage._id <= 210008 ? 210009 : stage._id);
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
                    slot.Icon.sprite = UIUtils.FillerSmallSprite;
                }
                else
                {
                    if (locationsWithHints.Count == 0 || locationsWithHints.Contains(receptionLocations[i]))
                    {
                        ItemLocationPair pair = LocationManager.GetLocationPair(receptionLocations[i]);
                        slot.BookName.text = LocationManager.FormatPairItem(pair);

                        if (pair.Item.Flags.HasFlag(ItemFlags.Advancement))
                            slot.Icon.sprite = UIUtils.ProgSmallSprite;
                        else if (pair.Item.Flags.HasFlag(ItemFlags.NeverExclude))
                            slot.Icon.sprite = UIUtils.UsefulSmallSprite;
                        else
                            slot.Icon.sprite = UIUtils.FillerSmallSprite;
                    }
                    else
                    {
                        slot.BookName.text = "???";
                        slot.Icon.sprite = UIUtils.FillerSmallSprite;
                    }
                }

                slot.IconGlow.enabled = false;
                slot.SetHighlighted(false);
                slot.originSiblingIdx = slot.transform.GetSiblingIndex();
            }

            __instance.rewardBookList.SetActiveList(true);

            // Set floor icon for stages
            BattleNode battleNode = SlotDataManager.BattleTree?.GetNodeById(stage._id);
            if (battleNode == null || battleNode.Kind != BattleNodeKind.Stage || stage.currentState != StoryState.Clear)
                return;

            UIIconManager.IconSet set = UIUtils.GetFloorIconSet(stage._id, battleNode.AssignedFloor);

            __instance.img_enemyTitleIcon.sprite = set.icon;
            __instance.img_enemyTitleIconBg.sprite = set.iconGlow;
        }



        // Change icon of the stage for floor stages on right invitation panel
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.SetLowerIconData))]
        [HarmonyPostfix]
        static void RightPanelIcon(UIInvitationRightMainPanel __instance, StageClassInfo stage)
        {
            if (stage == null)
                return;

            BattleNode battleNode = SlotDataManager.BattleTree?.GetNodeById(stage._id);
            if (battleNode == null || battleNode.Kind != BattleNodeKind.Stage || stage.currentState != StoryState.Clear)
                return;

            UIIconManager.IconSet set = UIUtils.GetFloorIconSet(stage._id, battleNode.AssignedFloor);

            __instance.LowerIcon.sprite = set.icon;
            __instance.LowerIconGlow.sprite = set.iconGlow;
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



        // Start the floor stage from map
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.ConfirmSendInvitation))]
        [HarmonyPrefix]
        static bool StartFloorStage(UIInvitationRightMainPanel __instance)
        {
            StageClassInfo stage = __instance.GetBookRecipe();

            if (stage == null)
            {
                MessagePopup.ShowMessage($"Stage is null.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            // If it's reverb ensemble stage, check if player has the floor for it
            if (stage._id >= 70001 && stage._id <= 70010 && !stage.floorOnlyList[0].IsOpen())
            {
                MessagePopup.ShowMessage($"{stage.floorOnlyList[0].FloorName()} is not unlocked.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            // If it's one of the keter stages, make sure to get the last one instead since only it technically exists on the tree
            BattleNode battleNode = SlotDataManager.BattleTree?.GetNodeById(stage._id);
            if (battleNode == null || battleNode.Kind != BattleNodeKind.Stage)
                return true;

            __instance.confirmAreaRoot.SetActive(false);

            if (!battleNode.AssignedFloor.IsOpen())
            {
                MessagePopup.ShowMessage($"{battleNode.AssignedFloor.FloorName()} is not unlocked.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Invite);

            UI.UIController UIController = UI.UIController.Instance;
            StageController StageController = StageController.Instance;

            UI.UIController.Instance.SetCurrentSephirah(battleNode.AssignedFloor);

            // If it's one of the stages of keter realization, we start it differently
            if (stage._id >= 210005 && stage._id <= 210009)
            {
                // StartEndContentsStage
                UIController.SetStageInfo(stage);

                // InitStageForKeterCompleteOpen
                StageController.InitStageByCreature(stage);
                StageController.firstStartState = true;
                StageController._isEndContentsStage = true;

                StageLibraryFloorModel _floor = StageController._stageModel.GetFloor(battleNode.AssignedFloor);
                if (_floor == null)
                    return false;

                StageController.SetCurrentSephirah(battleNode.AssignedFloor);

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
                StageController.SetCurrentSephirah(battleNode.AssignedFloor);

                StageController.InitStageByCreature(stage);

                // Add units to the battle (don't add binah if not unlocked)
                foreach (UnitBattleDataModel unitBattleData in StageController.GetCurrentStageFloorModel().GetUnitBattleDataList())
                {
                    if (battleNode.AssignedFloor == SephirahType.Binah && LibraryModel.Instance.IsBinahLockedInLibrary() && unitBattleData.unitData.isSephirah)
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


            return false;
        }


        // Hide "Workshop" checkbox
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.OpenInit))]
        [HarmonyPostfix]
        static void NoWorkshopReceptions(UIInvitationRightMainPanel __instance)
        {
            __instance.ob_customMode.gameObject.SetActive(false);
            __instance._workshopInvitationToggle.isOn = false;
        }



        // Disallow manual placement of books in the invitation
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.SetInvBookApplyState))]
        [HarmonyPrefix]
        static bool SetUnneededSlots(UIInvitationRightMainPanel __instance, ref InvitationApply_State state)
        {
            if (state == InvitationApply_State.Normal)
            {
                __instance.SetActiveEndEffect(false);
                __instance.currentinvState = state;
                __instance.invitationbookSlots.ForEach(s => s.SetDisabledSlot());
                __instance.SetUpdatePanel();

                return false;
            }

            return true;
        }



        // AP battle-tree fixed receptions use randomized book requirements, so vanilla GetDataFromBooks() must not be used for them.
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.GetBookRecipe))]
        [HarmonyPrefix]
        static bool UseSelectedBattleTreeStageAsRecipe(UIInvitationRightMainPanel __instance, ref StageClassInfo __result)
        {
            __result = null;

            StageClassInfo currentStage = __instance.invPanel.CurrentStage;
            if (currentStage == null || __instance.invPanel.currentSelectedStorySlot == null || __instance.invPanel.currentStoryidx < 0)
                return false;

            if (__instance.invPanel.currentSelectedStorySlot.storyData.Count <= __instance.invPanel.currentStoryidx ||
                currentStage != __instance.invPanel.currentSelectedStorySlot.storyData[__instance.invPanel.currentStoryidx])
                return false;

            // Check if the stage is ""clear"" since that is same as checking if battle parents are complete
            if (currentStage.currentState != StoryState.Clear)
                return false;

            List<LorId> requiredBooks = currentStage.invitationInfo.needsBooks ?? new List<LorId>();
            List<LorId> appliedBooks = __instance.GetAppliedBookModel().Select(book => book.id).ToList();

            if (requiredBooks.Count == 0 || requiredBooks.All(book => appliedBooks.Contains(book)))
                __result = currentStage;

            return false;
        }



        [HarmonyPatch(typeof(UIStoryProgressPanel), nameof(UIStoryProgressPanel.SetRectPos))]
        [HarmonyPrefix]
        static bool RemoveBattleTreeMapScrollClamp(UIStoryProgressPanel __instance, Vector2 target)
        {
            float minY = -Math.Max(__instance.posRect.sizeDelta.y, 9000f);
            target.y = Mathf.Clamp(target.y, minY, 1200f);
            __instance.posRect.anchoredPosition = target;
            return false;
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
