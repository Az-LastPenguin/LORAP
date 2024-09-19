using HarmonyLib;
using static StageController;
using UI;
using GameSave;
using System.Collections.Generic;
using System.Linq;

namespace LORAP.Patches
{
    [HarmonyPatch(typeof(StageController))]
    internal class AbnoChecks
    {
        // On Abno Suppression or Floor Realization end, send the checks
        [HarmonyPatch("EndBattlePhase_creature")]
        [HarmonyPrefix]
        static bool AbnoChecksAndProgress(StageController __instance)
        {
            StageModel stageModel = __instance.GetStageModel();

            StageWaveModel wave = stageModel.GetWave(__instance.CurrentWave);
            StageLibraryFloorModel floor = stageModel.GetFloor(__instance.CurrentFloor);
            if (stageModel.GetFrontAvailableWave() == null || stageModel.GetFrontAvailableFloor() == null)
            {
                bool flag = stageModel.GetFrontAvailableWave() == null;
                if (!__instance.IsRebattle && flag)
                {
                    if (stageModel.ClassInfo.id == 210005 || stageModel.ClassInfo.id == 210006 || stageModel.ClassInfo.id == 210007 || stageModel.ClassInfo.id == 210008)
                    {
                        LatestDataModel latestDataModel = new LatestDataModel();
                        Singleton<SaveManager>.Instance.LoadLatestData(latestDataModel);
                        latestDataModel.LatestStorychapter = 100;
                        latestDataModel.LatestStorygroup = 10;
                        latestDataModel.LatestStoryepisode = 4;
                        Singleton<SaveManager>.Instance.SaveLatestData(latestDataModel);
                        Traverse.Create(__instance).Field<EnemyTeamStageManager>("_enemyStageManager").Value.OnStageClear();
                        LibraryModel.Instance.ClearInfo.SetClearCountForEndContents(stageModel.ClassInfo.id);
                        Singleton<SaveManager>.Instance.SavePlayData(1);
                        SingletonBehavior<BattleManagerUI>.Instance.ui_TargetArrow.ActiveTargetParent(on: false);
                        if (SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.autoCardButton != null)
                        {
                            SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.autoCardButton.SetActivate(on: false);
                        }
                        if (SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.unequipcardallButton != null)
                        {
                            SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.unequipcardallButton.SetActivate(on: false);
                        }
                        SingletonBehavior<BattleManagerUI>.Instance.ui_TargetArrow.ClearCloneArrows();
                        SingletonBehavior<BattleManagerUI>.Instance.ui_emotionInfoBar.targetingToggle.SetDefault();
                        __instance.firstStartState = false;
                        int num = 210005;
                        int keterCompleteOpenPhase = LibraryModel.Instance.GetKeterCompleteOpenPhase();
                        switch (keterCompleteOpenPhase)
                        {
                            case 2:
                                num = 210006;
                                break;
                            case 3:
                                num = 210007;
                                break;
                            case 4:
                                num = 210008;
                                break;
                            case 5:
                                num = 210009;
                                break;
                            default:
                                break;
                            case 1:
                                break;
                        }
                        StageClassInfo data = Singleton<StageClassInfoList>.Instance.GetData(num);
                        if (data != null)
                        {
                            __instance.InitStageForKeterCompleteOpen(data);

                            GlobalGameManager.Instance.LoadBattleScene();
                        }
                        return false;
                    }
                    if (stageModel.ClassInfo.id == 210009 && floor.Sephirah == SephirahType.Keter && !APPlaythruManager.EndGameBattlesBeaten.Contains(stageModel.ClassInfo.id.id) && APPlaythruManager.Goals.Contains(APPlaythruManager.GoalType.KeterRealization))
                    {
                        APPlaythruManager.AddAbnoProgress(floor.Sephirah);

                        APPlaythruManager.CheckEndConditions();

                        Singleton<SaveManager>.Instance.SavePlayData(1);

                        return false;
                    }
                }

                __instance.battleState = BattleState.None;
                GameSceneManager.Instance.ActivateUIController();
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
                if (!flag)
                {
                    return false;
                }
                LibraryFloorModel floor2 = LibraryModel.Instance.GetFloor(floor.Sephirah);
                if (floor2 != null)
                {
                    ItemLocationManager.SendAbnoChecks(floor.Sephirah);

                    APPlaythruManager.AddAbnoProgress(floor.Sephirah);

                    Singleton<SaveManager>.Instance.SavePlayData(1);
                }
            }
            else
            {
                __instance.battleState = BattleState.Setting;
                if (wave.IsUnavailable())
                {
                    Singleton<LibraryQuestManager>.Instance.OnWinWave(floor);
                    __instance.SetCurrentWave(__instance.CurrentWave + 1);
                    Traverse.Create(__instance).Field("_prevDefeatFloor").SetValue(SephirahType.None);
                }
                if (floor.IsUnavailable())
                {
                    __instance.GameOver(iswin: false);
                    UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
                }
            }

            return false;
        }
    }

    [HarmonyPatch(typeof(UI.UIController))]
    internal class AbnoSuppression1
    {
        // Force current Abno Suppressions and Realizations. Next patch is related
        [HarmonyPatch(nameof(UI.UIController.OnClickStartCreatureStage))]
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

    [HarmonyPatch(typeof(UIMainPanel))]
    internal class AbnoSuppression2
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
                    UI.UIController.Instance.SetCurrentSephirah(targetSephirah);
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
                                break;
                        }
                    }
                    else
                    {
                        UI.UIController.Instance.OnClickStartCreatureStage(targetSephirah);
                    }
                }
            }, param);

            return false;
        }
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class AbnoSuppression3
    {
        // Check if player can suppress an abno or realize the floor. Next patchs is related
        [HarmonyPatch(nameof(LibraryModel.CanLevelUpSephirah))]
        [HarmonyPrefix]
        static bool CanLevelUpSephirahPrefix(LibraryModel __instance, SephirahType sep, ref bool __result)
        {
            if (!Traverse.Create(__instance).Field<HashSet<SephirahType>>("_openedSephirah").Value.Contains(sep))
            {
                __result = false;
                return false;
            }

            if (sep == SephirahType.Keter)
            {
                if (APPlaythruManager.Goals.Contains(APPlaythruManager.GoalType.KeterRealization))
                    __result = APPlaythruManager.AbnoProgress[sep] < 6;
                else
                    __result = APPlaythruManager.AbnoProgress[sep] < 5;

                return false;
            }

            if (sep == SephirahType.Binah || sep == SephirahType.Hokma)
            {
                __result = APPlaythruManager.AbnoProgress[sep] < 5;

                return false;
            }

            __result = APPlaythruManager.AbnoProgress[sep] < 6;

            return false;
        }
    }

    [HarmonyPatch(typeof(LibraryModel))]
    internal class AbnoSuppression4
    {
        [HarmonyPatch(nameof(LibraryModel.CheckCreatureBossBattle))]
        [HarmonyPrefix]
        static bool CheckCreatureBossBattlePrefix(LibraryModel __instance, LibraryFloorModel floor, ref bool __result)
        {
            __result = false;

            switch (floor.Sephirah)
            {
                case SephirahType.Binah:
                    if (APPlaythruManager.AbnoProgress[floor.Sephirah] == 4)
                    {
                        __result = true;
                    }
                    break;
                case SephirahType.Hokma:
                    if (APPlaythruManager.AbnoProgress[floor.Sephirah] == 4)
                    {
                        __result = true;
                    }
                    break;
                default:
                    if (APPlaythruManager.AbnoProgress[floor.Sephirah] == 5)
                    {
                        __result = true;
                    }
                    break;
            }

            return false;
        }
    }
}
