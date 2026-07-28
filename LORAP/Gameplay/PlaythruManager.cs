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
    }

    internal static class PlaythruManager
    {
        // Gameplay stuff
        internal static Dictionary<SephirahType, FloorInfo> Floors = Enum.GetValues(typeof(SephirahType)).Cast<SephirahType>().ToDictionary(k => k, v => new FloorInfo());
        internal static List<int> StagesCompleted = new List<int>();

        internal static int KeterRealizationStage = 0;

        internal static bool CanAttributePassives = false;
        internal static int MaxAttributionPoints = 0;
        internal static int MaxPassives = 0;

        internal static int MaxEmotionLevel = 0;

        internal static bool BinahUnlocked = false;
        internal static bool BlackSilenceUnlocked = false;
        internal static int BoEBundlesReceived = 0;

        internal static void StartGame()
        {
            Debug.Log("[LORAP] Starting Game");

            // Clear PlaythruManager
            StagesCompleted.Clear();
            KeterRealizationStage = 0;
            CanAttributePassives = false;
            MaxAttributionPoints = 0;
            MaxPassives = 0;
            MaxEmotionLevel = 0;
            BinahUnlocked = false;
            BlackSilenceUnlocked = false;
            BoEBundlesReceived = 0;

            // Create list of floor infos to keep track of every floors state by our own
            Floors = Enum.GetValues(typeof(SephirahType)).Cast<SephirahType>().ToDictionary(k => k, v => new FloorInfo());

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

            // Init book drop manager
            BookDropManager.PrepareDrops();

            // Load Save
            SaveManager.LoadGame();

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
            GameSceneManager.Instance.ActivateUIController(true);
            GlobalGameManager.Instance._gamePlayInitialized = true;

            // Can now start item pop coroutine
            ItemManager.Start();

            APConnectWindow.Close();
            APLog.Show();

            Debug.Log("[LORAP] Game Started");
        }

        // Saving/loading game state
        internal static GameSave.SaveData GetSaveData()
        {
            GameSave.SaveData saveData = new GameSave.SaveData();

            // Save completed receptions
            saveData.AddData("stagesCompleted", new GameSave.SaveData(StagesCompleted));

            saveData.AddData("keterRealizationStage", new GameSave.SaveData(KeterRealizationStage));
            saveData.AddData("boeBundlesReceived", new GameSave.SaveData(BoEBundlesReceived));

            return saveData;
        }

        internal static void LoadFromSaveData(GameSave.SaveData saveData)
        {
            StagesCompleted = saveData.GetData("stagesCompleted")._list.Select(d => d.GetIntSelf()).ToList();

            KeterRealizationStage = saveData.GetData("keterRealizationStage").GetIntSelf();
            BoEBundlesReceived = saveData.GetData("boeBundlesReceived").GetIntSelf();
        }


        //------ Utils
        private static void ChangeUIToFloor(SephirahType seph)
        {
            if (UI.UIController.Instance.CurrentUIPhase != UIPhase.Sephirah || !LibraryModel.Instance.IsOpenedSephirah(seph))
                return;

            UI.UIController.Instance.SetCurrentSephirah(seph);
            UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
        }

        internal static void MarkStageCompleted(int stageId)
        {
            if (!StagesCompleted.Contains(stageId))
                StagesCompleted.Add(stageId);
        }

        internal static bool IsStageComplete(int stageId)
        {
            return StagesCompleted.Contains(stageId);
        }

        internal static void CheckEndConditions()
        {
            bool allGoalsComplete = true;

            foreach (Endgoal goal in SettingsManager.Endgoals.GetValue())
            {
                switch (goal)
                {
                    case Endgoal.ReverberationEnsemble:
                        int completedEnsembleBattles = Enumerable.Range(70001, 10).Count(IsStageComplete);
                        if (SettingsManager.EnsembleBattles > completedEnsembleBattles)
                            allGoalsComplete = false;
                        break;

                    case Endgoal.BlackSilence:
                        if (!IsStageComplete(60003))
                            allGoalsComplete = false;
                        break;

                    case Endgoal.KeterRealization:
                        if (!IsStageComplete(210009))
                            allGoalsComplete = false;
                        break;

                    case Endgoal.DistortedEnsemble:
                        if (!IsStageComplete(60004))
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
                Floors[seph].Librarians = Math.Min(5, Floors[seph].Librarians + 1);
                seph.FloorModel().SetOpenedUnitCount(Floors[seph].Librarians);
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

        internal static void GiveBookOfEverything(bool silent = false)
        {
            BoEBundlesReceived++;
            GiveBook(123456, 1, silent);
        }

        internal static int GetUnlockedBoEBundleLimit()
        {
            if (!SlotDataManager.BoESpheresEnabled || SlotDataManager.BattleTree == null)
                return int.MaxValue;

            int unlockedSphere = 1;

            for (int targetSphere = 2; targetSphere <= 7; targetSphere++)
            {
                bool sphereOpen = SlotDataManager.BattleTree.Nodes.Values
                    .Where(n => n.Sphere == targetSphere)
                    .Any(n => n.AreBattleParentsComplete());

                if (sphereOpen)
                    unlockedSphere = Math.Max(unlockedSphere, targetSphere);
            }

            BattleNode oliver = SlotDataManager.BattleTree.GetNodeById(SlotDataManager.LastReception);
            if (oliver != null && IsStageComplete(oliver.Id))
                unlockedSphere = 7;

            return SlotDataManager.GetBoEBundlesRequiredThroughSphere(unlockedSphere);
        }

        internal static bool CanOpenBookOfEverything()
        {
            return BookDropManager.BooksOfEverythingOpened < GetUnlockedBoEBundleLimit();
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

            // If received during the battle, make it work
            if (StageController.Instance.battleState == StageController.BattleState.Battle)
            {
                foreach (BattleUnitModel unit in StageController.Instance._librarianTeam.GetList())
                {
                    unit.emotionDetail.SetMaxEmotionLevel(MaxEmotionLevel);
                }
            }

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

        internal static void ProgressKeterRealization()
        {
            KeterRealizationStage++;

            if (KeterRealizationStage >= 5)
                KeterRealizationStage = 0;
        }
    }
}
