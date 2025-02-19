using GameSave;
using HarmonyLib;
using LORAP.Archipelago;
using LORAP.CustomUI;
using LORAP.Gameplay;
using LORAP.Gameplay.Mechanics.Drops;
using LORAP.Gameplay.Mechanics.Goals;
using LORAP.Gameplay.Systems.Goals;
using System;
using System.Collections.Generic;
using System.Linq;
using UI;
using UnityEngine;

namespace LORAP.Playthru
{
    // Settings enums and other
    internal enum AbnoPagesBalance
    {
        Unbalanced,
        Balanced,
        Vanilla
    }

    internal enum TrapsDifficulty
    {
        Easy,
        Medium,
        Hard
    }

    internal enum DropSystem
    {
        BookOfEverything,
        BookOfEverythingBalanced
    }

    internal enum PageRandomization
    {
        None,
        Basic,
        More,
        Havoc
    }

    internal class FloorInfo
    {
        public bool Open = true;
        public int AbnoPages = 0;
        public int EGO = 0;
        public int CurrentAbno = 1;
    }

    internal struct SlotDataStruct
    {
        // Seed
        public int Seed;

        // Settings
        public int Fillers; // NOT IMPLEMENTED

        public int Traps; // NOT IMPLEMENTED

        public TrapsDifficulty TrapsDifficulty; // NOT IMPLEMENTED

        public bool LockedFloors; // NOT IMPLEMENTED

        public bool RandomFirstFloor; // NOT IMPLEMENTED

        public List<string> EndGoals;

        public int EnsembleBattles;

        public AbnoPagesBalance AbnoPagesBalance;

        public DropSystem DropSystem;

        public PageRandomization PageRandomization; // NOT IMPLEMENTED

        // Data
        public int TotalBooksOfEverything;


        public SlotDataStruct(Dictionary<string, object> slotData)
        {
            // Seed
            Seed = Convert.ToInt32(slotData["seed"]);

            // Settings
            Fillers = Convert.ToInt32(slotData["fillers"]);

            Traps = Convert.ToInt32(slotData["traps"]);

            TrapsDifficulty = (TrapsDifficulty)Convert.ToInt32(slotData["traps_difficulty"]); 

            LockedFloors = Convert.ToBoolean(slotData["locked_floors"]);

            RandomFirstFloor = Convert.ToBoolean(slotData["random_first_floor"]);

            EndGoals = slotData["end_goals"].ToString().Split(',').ToList();

            EnsembleBattles = Convert.ToInt32(slotData["ensemble_battles"]);

            AbnoPagesBalance = (AbnoPagesBalance)Convert.ToInt32(slotData["abno_page_balance"]);

            DropSystem = (DropSystem)Convert.ToInt32(slotData["drop_system"]);

            PageRandomization = (PageRandomization)Convert.ToInt32(slotData["randomize_pages"]);

            // Data
            TotalBooksOfEverything = Convert.ToInt32(slotData["books_of_everything"]);
        }
    }

    internal static class PlaythruManager
    {
        // Current Run Settings
        internal static int Fillers = 10; // NOT IMPLEMENTED

        internal static int Traps = 10; // NOT IMPLEMENTED

        internal static TrapsDifficulty TrapsDifficulty = TrapsDifficulty.Hard; // NOT IMPLEMENTED

        internal static bool LockedFloors = false; // NOT IMPLEMENTED

        internal static bool RandomFirstFloor = true; // NOT IMPLEMENTED

        internal static AbnoPagesBalance AbnoPageBalance = AbnoPagesBalance.Balanced;

        internal static PageRandomization PageRandomization = PageRandomization.None; // NOT IMPLEMENTED


        // Current Run State
        internal static int ItemsReceived = 0;

        internal static int Seed = 143;


        // Gameplay stuff
        internal static List<int> ReceptionsCompleted = new List<int>();

        internal static List<int> OpenedReceptions = new List<int>();

        internal static List<int> FoundBooks = new List<int>();

        internal static Dictionary<SephirahType, FloorInfo> Floors = Enum.GetValues(typeof(SephirahType)).Cast<SephirahType>().ToDictionary(k => k, v => new FloorInfo());

        internal static bool BinahUnlocked = false;

        internal static bool BlackSilenceUnlocked = false;

        internal static int MaxPassiveCost = 8;

        internal static void SetupRun(Dictionary<string, object> slotData, string seed)
        {
            SlotDataStruct SlotData = new SlotDataStruct(slotData);

            // Set Run's Seed and Settings
            Seed = SlotData.Seed;

            Fillers = SlotData.Fillers;
            Traps = SlotData.Traps;
            AbnoPageBalance = SlotData.AbnoPagesBalance;


            // Randomize Content
            ContentManager.RandomizeContent();

            // Setup stuff
            GoalsManager.Setup(SlotData);

            DropsManager.Setup(SlotData, Seed);

            // Load Save / Create new Save
            Gameplay.SaveManager.LoadGame(seed);
        }

        // Saving/loading game state
        internal static void LoadFromSaveData(SaveData saveData)
        {
            ItemsReceived = saveData.GetData("itemsReceived").GetIntSelf();
            ReceptionsCompleted = saveData.GetData("receptionsCompleted")._list.Select(d => d.GetIntSelf()).ToList();
            OpenedReceptions = saveData.GetData("openedReceptions")._list.Select(d => d.GetIntSelf()).ToList();
            FoundBooks = saveData.GetData("foundBooks")._list.Select(d => d.GetIntSelf()).ToList();

            var floorData = saveData.GetData("floors");
            for (int i = 0; i < Floors.Count; i++)
            {
                var data = floorData._list[i];
                var pair = Floors.ElementAt(i);

                pair.Value.Open = data.GetInt("Open") == 1;
                pair.Value.AbnoPages = data.GetInt("AbnoPages");
                pair.Value.EGO = data.GetInt("EGO");
                pair.Value.CurrentAbno = data.GetInt("CurrentAbno");
            }

            BinahUnlocked = saveData.GetData("binahUnlocked").GetIntSelf() == 1;
            BlackSilenceUnlocked = saveData.GetData("blackSilenceUnlocked").GetIntSelf() == 1;
            MaxPassiveCost = saveData.GetData("maxPassiveCost").GetIntSelf();

            DropsManager.LoadSaveData(saveData.GetData("dropSystem"));

            //string dropData = saveData.GetString("dropWeights");
            //dropData.Split('|').Do(d => {var i = d.Split(';'); DropWeights[Convert.ToInt32(i[0])] = Convert.ToInt32(i[1]); });

            /*num = 0;
            foreach (var d in saveData.GetData("rarityDrops"))
            {
                RarityDrops[RarityDrops.Keys.ElementAt(num)].packsUsed = d.GetInt("packsUsed");
                foreach (var dt in d.GetData("notFoundItems"))
                {
                    RarityDrops[RarityDrops.Keys.ElementAt(num)].notFoundItems.Add(dt.GetIntSelf());
                }

                num++;
            }*/
        }

        internal static SaveData GetSaveData()
        {
            SaveData saveData = new SaveData();

            saveData.AddData("receptionsCompleted", new SaveData(ReceptionsCompleted));
            saveData.AddData("itemsReceived", new SaveData(ItemsReceived));
            saveData.AddData("openedReceptions", new SaveData(OpenedReceptions));
            saveData.AddData("foundBooks", new SaveData(FoundBooks));

            SaveData floors = new SaveData();
            foreach (var info in Floors.Values)
            {
                SaveData floorData = new SaveData();
                floorData.AddData("Open", new SaveData(info.Open ? 1 : 0));
                floorData.AddData("AbnoPages", new SaveData(info.AbnoPages));
                floorData.AddData("EGO", new SaveData(info.EGO));
                floorData.AddData("CurrentAbno", new SaveData(info.CurrentAbno));
                floors.AddToList(floorData);
            }
            saveData.AddData("floors", floors);

            saveData.AddData("binahUnlocked", new SaveData(BinahUnlocked ? 1 : 0));
            saveData.AddData("blackSilenceUnlocked", new SaveData(BlackSilenceUnlocked ? 1 : 0));
            saveData.AddData("maxPassiveCost", new SaveData(MaxPassiveCost));

            // Drop system data
            saveData.AddData("dropSystem", DropsManager.GetSaveData());

            return saveData;
        }

        // Utils
        private static void FloorUpgradePopup(string text, SephirahType seph = SephirahType.None)
        {
            if (UI.UIController.Instance.CurrentUIPhase == UIPhase.Sephirah && seph != SephirahType.None && LibraryModel.Instance.IsOpenedSephirah(seph))
            {
                GameSceneManager.Instance.ActivateUIController();
                UI.UIController.Instance.SetCurrentSephirah(seph);
                UI.UIController.Instance.CallUIPhase(UIPhase.Sephirah);
            }

            MessagePopup.ShowMessage(text);
        }

        internal static void CheckEndConditions()
        {
            // Check goals
            if (GoalsManager.GoalsAchieved())
                ConnectionManager.AchieveGoal();
        }

        internal static bool IsReceptionOpened(int id)
        {
            return OpenedReceptions.Contains(id) || GoalsManager.Goals.SelectMany(g => g.Active ? (g as ClearGoal).Stages : new List<int>()).Contains(id);
        }

        // Progression
        internal static void ProgressAbno(SephirahType seph)
        {
            if (Floors[seph].CurrentAbno > 5) return;

            Floors[seph].CurrentAbno++;
        }

        internal static void OpenReception(int id)
        {
            OpenedReceptions.Add(id);

            (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel.SetStoryLine();

            FloorUpgradePopup($"Reception of {StageNameXmlList.Instance.GetName(id)} was unlocked!");
        }

        internal static void OpenFloor(SephirahType seph)
        {
            if (LibraryModel.Instance.IsOpenedSephirah(seph)) return;

            LibraryModel.Instance.OpenSephirah(seph);

            if (seph == SephirahType.Binah)
                seph.FloorModel().SetOpenedUnitCount(2);


            FloorUpgradePopup($"{seph.FloorName()} was Opened!", seph);
        }

        internal static void AddAnboPages(SephirahType seph)
        {
            if (Floors[seph].AbnoPages > 4) return;

            Floors[seph].AbnoPages++;

            MessagePopup.ShowPages(LibraryModel.Instance.GetFloor(seph), Floors[seph].AbnoPages);
        }

        internal static void AddEGO(SephirahType seph)
        {
            if (Floors[seph].EGO > 4) return;

            Floors[seph].EGO++;

            MessagePopup.ShowPages(LibraryModel.Instance.GetFloor(seph), Floors[seph].EGO, true);
        }

        internal static void AddLibrarian(SephirahType seph)
        {
            var floor = seph.FloorModel();
            
            if (floor.GetOpendUnitCount() > 4) return;

            floor.SetOpenedUnitCount(Math.Min(5, floor.GetOpendUnitCount() + 1));
            FloorUpgradePopup($"{seph.FloorName()} upgrade! +1 Librarian!", seph);
        }

        internal static void GiveBook(int i, int num = 1)
        {
            DropBookInventoryModel.Instance.AddBook(new LorId("lorap", i), num);

            MessagePopup.ShowMessage($"You received {DropBookXmlList.Instance.GetData(new LorId("lorap", i)).Name}!");
        }

        internal static void UpMaxPassiveCost(bool silent = false)
        {
            MaxPassiveCost++;
            FloorUpgradePopup($"Library upgrade! +1 Max Passive Point!");
        }

        internal static void UnlockBinah(bool silent = false)
        {
            if (BinahUnlocked) return;

            BinahUnlocked = true;
            FloorUpgradePopup($"Library upgrade! Binah unlocked!", SephirahType.Binah);
        }

        internal static void UnlockBlackSilence(bool silent = false)
        {
            if (BlackSilenceUnlocked) return;

            BlackSilenceUnlocked = true;
            LibraryModel.Instance.GetFloor(SephirahType.Keter).GetUnitDataList().Find((x) => x.isSephirah)?.ResetForBlackSilence();
            FloorUpgradePopup($"Library upgrade! Black Silence unlocked!", SephirahType.Keter);
        }
    }

    internal static class LORClassExtensions
    {
        internal static int GetCurrentAbno(this LibraryFloorModel floor)
        {
            return PlaythruManager.Floors[floor.Sephirah].CurrentAbno;
        }

        internal static int GetEGOAmount(this LibraryFloorModel floor)
        {
            return PlaythruManager.Floors[floor.Sephirah].EGO;
        }

        internal static int GetAbnoPageAmount(this LibraryFloorModel floor)
        {
            return PlaythruManager.Floors[floor.Sephirah].AbnoPages;
        }

        internal static string FloorName(this SephirahType seph)
        {
            string floor = "";
            switch (seph)
            {
                case SephirahType.None:
                    floor = "";
                    break;
                case SephirahType.Keter:
                    floor = "Floor of General Works";
                    break;
                case SephirahType.Malkuth:
                    floor = "Floor of History";
                    break;
                case SephirahType.Yesod:
                    floor = "Floor of Technological Sciences";
                    break;
                case SephirahType.Hod:
                    floor = "Floor of Literature";
                    break;
                case SephirahType.Netzach:
                    floor = "Floor of Art";
                    break;
                case SephirahType.Tiphereth:
                    floor = "Floor of Natural Sciences";
                    break;
                case SephirahType.Chesed:
                    floor = "Floor of Social Sciences";
                    break;
                case SephirahType.Gebura:
                    floor = "Floor of Language";
                    break;
                case SephirahType.Hokma:
                    floor = "Floor of Religion";
                    break;
                case SephirahType.Binah:
                    floor = "Floor of Philosophy";
                    break;
            }
            return floor;
        }

        internal static LibraryFloorModel FloorModel(this SephirahType seph)
        {
            return LibraryModel.Instance._floorList.Find(f => f.Sephirah == seph);
        }
    }
}