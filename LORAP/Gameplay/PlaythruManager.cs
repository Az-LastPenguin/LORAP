using GameSave;
using LORAP.Archipelago;
using LORAP.CustomUI;
using LORAP.Gameplay;
using LORAP.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace LORAP.Playthru
{
    internal class FloorInfo
    {
        public bool Open = false; // Gotten from items on load
        public int AbnoPages = 0; // Gotten from items on load
        public int EGO = 0; // Gotten from items on load
        public int Librarians = 1; // Gotten from items on load
        public int AbnoStage = 1; // Saved in SaveData
    }

    internal static class PlaythruManager
    {
        // Gameplay stuff
        internal static Dictionary<SephirahType, FloorInfo> Floors = Enum.GetValues(typeof(SephirahType)).Cast<SephirahType>().ToDictionary(k => k, v => new FloorInfo());
        internal static List<int> ReceptionsCompleted = new List<int>();

        internal static bool CanAttributePassives = false;
        internal static int MaxAttributionPoints = 0;
        internal static int MaxPassives = 0;

        internal static int MaxEmotionLevel = 0;

        internal static bool BinahUnlocked = false;
        internal static bool BlackSilenceUnlocked = false;

        internal static void StartGame()
        {
            Debug.Log("[LORAP] Starting Game");

            // Create list of floor infos to keep track of every floors state by our own
            Floors = Enum.GetValues(typeof(SephirahType)).Cast<SephirahType>().ToDictionary(k => k, v => new FloorInfo());

            foreach (var floor in LibraryModel.Instance._floorList)
            {
                Floors[floor.Sephirah].Open = LibraryModel.Instance.IsOpenedSephirah(floor.Sephirah);
            }

            // Init AP Managers
            ItemManager.Init();
            LocationManager.Init();

            // Start Game
            //-- GlobalGameManager.ContinueGame
            GlobalGameManager.Instance._gamePlayInitialized = false;
            if (!GlobalGameManager.Instance._initialized && UIAlarmPopup.instance != null)
            {
                throw new Exception("The game is not initialized. Cannot start game.");
            }

            if (PlatformManager.Instance.IsProccessing)
                throw new Exception("Couldn't start game.");

            AssetBundleManagerRemake.Instance.Init();
            //--

            // Init floors & Set all floors levels to max internally, makes life easier
            LibraryModel.Instance.Init();
            LibraryModel.Instance._floorList.ForEach(f => f._level = 6); // TODO: Make it a patch

            // Setup run content
            ContentManager.SetupRunContent();

            // Init book dropp manager
            BookDropManager.Init();

            // Load Save
            Gameplay.SaveManager.LoadGame();

            // Set some flags so that game doesn't spam tutorial and shit
            PlayHistoryModel model = LibraryModel.Instance._playHistory;
            model.prologueOpenInvtationManual = 1;
            model.tutorial_keterOpenbyratsClear = 1;
            model.tutorialInteractUI_HighlightedInvitaionButton = 1;
            model.tutorial_SelectOneBook = 1;
            model.tutorial_EnterBattleSetting = 1;
            model.tutorial_EnterBattleResult = 1;
            model.tutorial_EnterUIScene = 1;
            model.tutorial_FloorFeedBookButtonClick = 1;
            model.tutorial_FloorFeedBookFirstClick = 1;
            model.tutorial_EnterResultFloorFeedBook = 1;
            model.tutorial_SelectLibrarianSlot = 1;
            model.tutorial_EnterBattlePagePanel = 1;
            model.tutorial_EnterEquipPagePanel = 1;
            model.tutorial_EnterLibrarianInfo = 1;
            model.tutorial_EnterCustomizeButton = 1;
            model.tutorial_EnterStoryArchives = 1;
            model.tutorial_firstCreatureBattleStart = 1;
            model.tutorial_EnterUISceneAfterYunOffice = 1;
            model.tutorial_EnterBattleSettingAfterYunOfficeWaveClear = 1;
            model.tutorial_PossibleFloorAlarm = 1;
            model.tutorial_EnterInvtationAfterHookOffice = 1;
            model.tutorial_OpenPassiveSuccessionAlarm = 1;
            model.tutorial_NightmareCostUpPassiveSuccessionAlarm = 1;
            model.tutorial_StarCostUpPassiveSuccessionAlarm = 1;
            model.tutorial_ImpurityCostUpPassiveSuccessionAlarm = 1;
            model.tutorial_Alarm_CanUsebinahInMain = 0;
            model.tutorial_Alarm_CanUseBlackSilence = 0;
            model.currentclearStoryid = 1;
            model.currentchapterLevel = 7;
            model.prologueOpenInvtationManual = 1;
            model.Tutorial_GetFirstCoreBook = 1;
            model.first_creaturebattle = 1;
            model.Start_TheBlueReverberationPrimaryBattle = 0;
            model.first_TheBluePrimary_keterXmark = 0;
            model.first_ThrBluePrimary_RewardAlarm = 0;
            model.story_BlackSilence_progress = 0;
            model.Start_EndContents = 0;
            model.Clear_TwistedBluePrevUpdate = 0;
            model.Clear_EndcontentsAllStage = 1;
            model.ResetSecondRewardClearEndContents = 0;
            model.tutorial_EnterBattle = 1;
            model.tutorial_EnterBattleSpaceDice = 1;
            model.tutorial_EnterBattle_StartBattleTutorial = 1;
            model.tutorial_CharacterEmotionCoinManual = 1;
            model.tutorial_FirstRevealCardRangeManual = 1;
            model.tutorial_PossibleEmotionCard = 1;
            model.tutorial_EnterBattlePuppet = 1;
            model.tutorial_FirstRevealWideCard = 1;
            model.tutorial_FirstRevealEgoCard = 1;
            model.tutorial_EnemyUnit_Break = 1;
            model.tutorial_EnemyUnit_Dead = 1;
            model.tutorial_CreatureBattle_StartTutorial = 1;
            model.feedBookCount = 1;
            model.furiosoKill1 = 1;
            model.furiosoKill2 = 1;

            LibraryModel.Instance._currentChapter = 7;

            // Put the player in the game, loading is done
            GameSceneManager.Instance.ActivateUIController(initUIScene: true);
            GlobalGameManager.Instance._gamePlayInitialized = true;

            // Can now start item pop coroutine
            ItemManager.Start();

            APConnectWindow.Close();
            APLog.Show();

            Debug.Log("[LORAP] Game Started");
        }

        // Saving/loading game state
        internal static SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();

            // Save completed receptions
            saveData.AddData("receptionsCompleted", new SaveData(ReceptionsCompleted));

            // Save floors data about current stage
            saveData.AddData("floorStages", new SaveData(Floors.Select(p => p.Value.AbnoStage).ToList()));

            saveData.AddData("maxAttributionPoints", new SaveData(MaxAttributionPoints)); // TODO: Make those values depend on items received and not saved.
            saveData.AddData("maxPassives", new SaveData(MaxPassives));
            saveData.AddData("maxEmotionLevel", new SaveData(MaxEmotionLevel));
            saveData.AddData("binahUnlocked", new SaveData(BinahUnlocked ? 1 : 0));
            saveData.AddData("blackSilenceUnlocked", new SaveData(BlackSilenceUnlocked ? 1 : 0));

            return saveData;
        }

        internal static void LoadFromSaveData(SaveData saveData)
        {
            ReceptionsCompleted = saveData.GetData("receptionsCompleted")._list.Select(d => d.GetIntSelf()).ToList();

            ReceptionsCompleted = ReceptionsCompleted.Where(IsStageCompleteForProgression).Distinct().ToList();

            var floorData = saveData.GetData("floorStages");
            for (int i = 0; i < Floors.Count; i++)
            {
                var data = floorData._list[i];
                var pair = Floors.ElementAt(i);

                pair.Value.AbnoStage = data.GetIntSelf();
            }

            var maxAttrib = saveData.GetData("maxAttributionPoints");
            if (maxAttrib != null)
                MaxAttributionPoints = maxAttrib.GetIntSelf();

            var maxPass = saveData.GetData("maxPassives");
            if (maxPass != null)
                MaxPassives = maxPass.GetIntSelf();

            var maxEmotion = saveData.GetData("maxEmotionLevel");
            if (maxEmotion != null)
                MaxEmotionLevel = maxEmotion.GetIntSelf();

            var binah = saveData.GetData("binahUnlocked");
            if (binah != null)
                BinahUnlocked = binah.GetIntSelf() != 0;

            var blackSilence = saveData.GetData("blackSilenceUnlocked");
            if (blackSilence != null)
                BlackSilenceUnlocked = blackSilence.GetIntSelf() != 0;
        }


        //------ Utils
        private static void ChangeUIToFloor(SephirahType seph)
        {
            if (UI.UIController.Instance.CurrentUIPhase != UIPhase.Sephirah || !LibraryModel.Instance.IsOpenedSephirah(seph))
                return;

            //GameSceneManager.Instance.ActivateUIController();
            UI.UIController.Instance.SetCurrentSephirah(seph);
            UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
        }

        internal static bool IsLocationlessEndgoalStage(int stageId)
        {
            if (SlotDataManager.Endgoals == null)
                return false;

            if (stageId >= 70001 && stageId <= 70010)
                return SlotDataManager.Endgoals.Contains(Endgoal.ReverberationEnsemble);

            switch (stageId)
            {
                case 60003:
                    return SlotDataManager.Endgoals.Contains(Endgoal.BlackSilence);
                case 210009:
                    return SlotDataManager.Endgoals.Contains(Endgoal.KeterRealization);
                case 60004:
                    return SlotDataManager.Endgoals.Contains(Endgoal.DistortedEnsemble);
                default:
                    return false;
            }
        }

        internal static bool IsStageCompleteForProgression(int stageId)
        {
            int totalLocations = LocationManager.GetReceptionLocations(stageId).Count;
            if (totalLocations > 0)
                return LocationManager.GetUncheckedReceptionLocations(stageId).Count == 0;

            return IsLocationlessEndgoalStage(stageId) && ReceptionsCompleted.Contains(stageId);
        }

        internal static bool MarkReceptionCompletedIfReady(int stageId, bool checkedAllLocationsByThisClear = false, bool wonLocationlessGoal = false)
        {
            bool hasLocations = LocationManager.GetReceptionLocations(stageId).Count > 0;
            bool isComplete = hasLocations
                ? checkedAllLocationsByThisClear || LocationManager.GetUncheckedReceptionLocations(stageId).Count == 0
                : wonLocationlessGoal && IsLocationlessEndgoalStage(stageId);

            if (!isComplete)
                return false;

            if (!ReceptionsCompleted.Contains(stageId))
                ReceptionsCompleted.Add(stageId);

            return true;
        }

        internal static bool IsReceptionCompleted(int stageId)
        {
            if (ReceptionsCompleted.Contains(stageId))
                return true;

            int totalLocations = LocationManager.GetReceptionLocations(stageId).Count;
            return totalLocations > 0 && MarkReceptionCompletedIfReady(stageId);
        }

        internal static void CheckEndConditions()
        {
            bool allGoalsComplete = true;

            foreach (Endgoal goal in SlotDataManager.Endgoals)
            {
                switch (goal)
                {
                    case Endgoal.ReverberationEnsemble:
                        int completedEnsembleBattles = Enumerable.Range(70001, 10).Count(IsReceptionCompleted);
                        if (completedEnsembleBattles < SlotDataManager.EnsembleBattles)
                            allGoalsComplete = false;
                        break;

                    case Endgoal.BlackSilence:
                        if (!IsReceptionCompleted(60003))
                            allGoalsComplete = false;
                        break;

                    case Endgoal.KeterRealization:
                        if (!IsReceptionCompleted(210009))
                            allGoalsComplete = false;
                        break;

                    case Endgoal.DistortedEnsemble:
                        if (!IsReceptionCompleted(60004))
                            allGoalsComplete = false;
                        break;
                }

                if (!allGoalsComplete)
                    break;
            }

            if (allGoalsComplete)
                LocationManager.AchieveGoal();
        }

        internal static void OpenFloor(SephirahType seph, bool silent = false)
        {
            if (LibraryModel.Instance.IsOpenedSephirah(seph))
            {
                Floors[seph].Open = true;

                if (seph == SephirahType.Binah && Floors[seph].Librarians < 2)
                {
                    seph.FloorModel().SetOpenedUnitCount(2);
                    Floors[seph].Librarians = 2;
                }

                return;
            }

            LibraryModel.Instance.OpenSephirah(seph);
            Floors[seph].Open = true;

            if (seph == SephirahType.Binah)
            {
                seph.FloorModel().SetOpenedUnitCount(2);
                Floors[seph].Librarians = 2;
            }

            ChangeUIToFloor(seph);
            if (!silent)
                MessagePopup.ShowMessage($"{seph.FloorName()} was opened!");
        }

        internal static void GiveAbnoPages(SephirahType seph, bool silent = false)
        {
            if (Floors[seph].AbnoPages >= 5) return;

            Floors[seph].AbnoPages++;

            if (!silent)
            {
                ChangeUIToFloor(seph);
                AbnoEgoPagePopup.ShowPages(seph.FloorModel(), Floors[seph].AbnoPages);
            }
            else if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
            {
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
            }
        }

        internal static void GiveEGOPage(SephirahType seph, bool silent = false)
        {
            if (Floors[seph].EGO >= 5) return;

            Floors[seph].EGO++;

            if (!silent)
            {
                ChangeUIToFloor(seph);
                AbnoEgoPagePopup.ShowPages(seph.FloorModel(), Floors[seph].EGO, true);
            }
            else if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
            {
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
            }
        }

        internal static void GiveLibrarian(SephirahType seph, bool silent = false)
        {
            if (Floors[seph].Librarians >= 5)
                return;

            Floors[seph].Librarians++;

            var floor = seph.FloorModel();
            floor.SetOpenedUnitCount(Floors[seph].Librarians);

            if (!silent)
            {
                ChangeUIToFloor(seph);
                MessagePopup.ShowMessage($"{seph.FloorName()} awoken a Librarian!");
            }
            else if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah)
            {
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
            }
        }

        internal static void GiveBook(int id, int num = 1, bool silent = false)
        {
            LorId lid = id == 123456 || id == 123457 ? new LorId("lorap", id) : new LorId(id);

            DropBookInventoryModel.Instance.AddBook(lid, num);

            if (!silent)
                MessagePopup.ShowMessage($"You received {DropBookXmlList.Instance.GetData(lid).Name}!");
        }

        internal static void UpMaxAttributionPoints(bool silent = false)
        {
            MaxAttributionPoints += 2;
            if (!silent)
                MessagePopup.ShowMessage($"Max Attribution points +2!");
        }

        internal static void UpMaxPassives(bool silent = false)
        {
            MaxPassives++;
            if (!silent)
                MessagePopup.ShowMessage($"Max Attributed Passives +1!");
        }

        internal static void UpMaxEmotion(bool silent = false)
        {
            MaxEmotionLevel++;
            if (!silent)
                MessagePopup.ShowMessage($"Max emotion level +1!");
        }

        internal static void UnlockBinah(bool silent = false)
        {
            if (BinahUnlocked) return;

            BinahUnlocked = true;

            if (!silent)
            {
                ChangeUIToFloor(SephirahType.Binah);
                MessagePopup.ShowMessage($"Binah has been unlocked!");
            }
        }

        internal static void UnlockBlackSilence(bool silent = false)
        {
            if (BlackSilenceUnlocked) return;

            BlackSilenceUnlocked = true;
            LibraryModel.Instance.GetFloor(SephirahType.Keter).GetUnitDataList().Find((x) => x.isSephirah)?.ResetForBlackSilence();

            if (!silent)
            {
                ChangeUIToFloor(SephirahType.Keter);
                MessagePopup.ShowMessage($"The Black Silence's Page has been unlocked!");
            }
        }

        // Progression
        internal static void ProgressSuppression(SephirahType seph)
        {
            Floors[seph].AbnoStage++;
        }
    }
}