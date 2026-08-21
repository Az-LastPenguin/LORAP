using Archipelago.MultiClient.Net.Enums;
using HarmonyLib;
using LORAP.Archipelago;
using LORAP.CustomUI;
using LORAP.Gameplay;
using LORAP.Playthru;
using LORAP.Utils;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;
using UnityEngine.UI;

namespace LORAP.Patches
{
    internal class MapPatches
    {
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

            if (SettingsManager.EndgoalsAlwaysUnlocked && GameUtils.IsStageEndgoal(battleNode.Id))
            {
                __result = StoryState.Clear;
                return false;
            }

            if (LocationManager.StageHasHintedItems(stageId))
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
                //if (storyData[0]._id >= 210005 && storyData[0]._id <= 210009)
                //    storyData = new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(210005 + PlaythruManager.KeterRealizationStage) };

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

                // If it's an ensemble stage and it can be seen, set its icon
                if (storyData[0]._id >= 70001 && storyData[0]._id <= 70010 && storyData[0].currentState == StoryState.Clear)
                    icon.SetIcon(UISpriteDataManager.instance.floorIconSet[(int)storyData[0].floorOnlyList[0]]);

                // Show the icon on the map
                icon.SetActiveStory(true);

                // Show status of the stage (Complete/Clearable/Hinted)
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

                // If the stage contains a check that is hinted, not collected and is not marked as "Avoid"
                if (LocationManager.StageHasPriorityHintedItems(storyData[0]._id))
                {
                    status.GetComponent<Image>().sprite = UIUtils.StarSprite;
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

            ContentManager.UpdateSphereSeparators();

            return false;
        }



        // Show AP items in "Resolvable Rewards", also set icon for floor stages
        [HarmonyPatch(typeof(UIInvitationStageInfoPanel), nameof(UIInvitationStageInfoPanel.SetData))]
        [HarmonyPostfix]
        static void OnSelectStage(UIInvitationStageInfoPanel __instance, StageClassInfo stage, UIStoryLine story = UIStoryLine.None)
        {
            // If it's one of the keter realization stages make sure to get the last one since it's the only one that technically has items
            List<long> receptionLocations = LocationManager.GetUncheckedStageLocations(stage._id);
            List<long> locationsWithHints = receptionLocations.Where(l => LocationManager.KnownHints.Any(h => !h.Found && h.LocationId == l)).ToList();

            // Scout all unchecked locations (they're already known, but this time we scout for hints.)
            // If this recepion is visible because of a hint, don't scout.
            if (locationsWithHints.Count == 0 && SettingsManager.SendHintsOnReceptionScout.Value)
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



        // Make game show current keter realization stage enemies when selecting keter realization
        [HarmonyPatch(typeof(UIEnemyCharacterListPanel), nameof(UIEnemyCharacterListPanel.SetEnemyInfo))]
        [HarmonyPrefix]
        static bool SetEnemyListKeterStage(UIEnemyCharacterListPanel __instance, ref StageClassInfo data)
        {
            if (data?._id == 210009)
                data = StageClassInfoList.Instance.GetData(210005 + PlaythruManager.KeterRealizationStage);

            return true;
        }



        // Make chapter goto buttons work with new separators
        [HarmonyPatch(typeof(UIStoryProgressPanel), nameof(UIStoryProgressPanel.MoveChapterTarget), typeof(int), typeof(bool))]
        [HarmonyPrefix]
        static bool SetEnemyListKeterStage(UIStoryProgressPanel __instance, int chapter, bool immediately)
        {
            __instance.scroll_viewPort.inertia = false;

            __instance.SetRectSize(.5f, immediately);
            __instance.defaultZoom = .5f;
            __instance.SetRectPos(new Vector2(0, -ContentManager.ChapterSeparators[chapter - 1].transform.localPosition.y - 1200f));

            __instance.scroll_viewPort.inertia = true;

            return false;
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



        // Start the floor stage from map
        [HarmonyPatch(typeof(UIInvitationRightMainPanel), nameof(UIInvitationRightMainPanel.ConfirmSendInvitation))]
        [HarmonyPrefix]
        static bool StartFloorStage(UIInvitationRightMainPanel __instance)
        {
            StageClassInfo stage = __instance.GetBookRecipe();

            if (stage == null)
            {
                MessagePopup.Instance.ShowMessage($"Stage is null.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            // If it's reverb ensemble stage, check if player has the floor for it
            if (stage._id >= 70001 && stage._id <= 70010 && !stage.floorOnlyList[0].IsOpen())
            {
                MessagePopup.Instance.ShowMessage($"{stage.floorOnlyList[0].FloorName()} is not unlocked.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            BattleNode battleNode = SlotDataManager.BattleTree?.GetNodeById(stage._id);
            if (battleNode == null || battleNode.Kind != BattleNodeKind.Stage)
                return true;

            if (!battleNode.AreBattleParentsComplete())
            {
                MessagePopup.Instance.ShowMessage($"You haven't reached this stage yet.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            if (!battleNode.AssignedFloor.IsOpen())
            {
                MessagePopup.Instance.ShowMessage($"{battleNode.AssignedFloor.FloorName()} is not unlocked.");

                UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Cancel);

                return false;
            }

            __instance.confirmAreaRoot.SetActive(false);
            UISoundManager.instance.PlayEffectSound(UISoundType.Ui_Invite);

            UI.UIController UIController = UI.UIController.Instance;
            StageController StageController = StageController.Instance;

            UI.UIController.Instance.SetCurrentSephirah(battleNode.AssignedFloor);

            // If it's one of the stages of keter realization, we start it differently
            if (stage._id == 210009)
            {
                // Get actual keter stage the player should be at
                stage = StageClassInfoList.Instance.GetData(210005 + PlaythruManager.KeterRealizationStage);

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



        // Make reception map regain inertia after zooming
        [HarmonyPatch(typeof(UIStoryProgressPanel), nameof(UIStoryProgressPanel.ZoomProcess))]
        [HarmonyPostfix]
        static IEnumerator FixMapZoomInertia(IEnumerator __result, UIStoryProgressPanel __instance)
        {
            while (__result.MoveNext())
                yield return __result.Current;

            __instance.scroll_viewPort.inertia = true;
        }
    }
}
