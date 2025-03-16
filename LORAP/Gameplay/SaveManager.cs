using GameSave;
using HarmonyLib;
using LORAP.Archipelago;
using LORAP.Playthru;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UI;
using UnityEngine;

namespace LORAP.Gameplay
{
    internal static class SaveManager
    {
        internal static string CurrentSaveFile;

        internal static SessionData LoadLastSessionData()
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            if (!File.Exists($"{Application.persistentDataPath}/Archipelago/LastSession"))
            {
                ConnectionManager.currentSessionData = new SessionData();
                return ConnectionManager.currentSessionData;
            }

            BinaryFormatter binaryFormatter = new BinaryFormatter();
            try
            {
                SessionData Data;
                using (FileStream fileStream = File.Open($"{Application.persistentDataPath}/Archipelago/LastSession", FileMode.Open))
                {
                    Data = binaryFormatter.Deserialize(fileStream) as SessionData;
                }
                if (Data == null)
                {
                    throw new Exception();
                }

                ConnectionManager.currentSessionData = Data;

                return ConnectionManager.currentSessionData;
            }
            catch (Exception)
            {
                ConnectionManager.currentSessionData = new SessionData();

                return ConnectionManager.currentSessionData;
            }
        }

        internal static void SaveLastSessionData()
        {
            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            using (FileStream serializationStream = File.Create($"{Application.persistentDataPath}/Archipelago/LastSession"))
            {
                new BinaryFormatter().Serialize(serializationStream, ConnectionManager.GetSessionData());
            }
        }

        internal static void SaveGame()
        {
            Debug.Log("Saving the game...");

            SaveLastSessionData();

            SaveData saveData = new SaveData();
            saveData.AddData("inventory", InventoryModel.Instance.GetSaveData());
            saveData.AddData("bookInventory", BookInventoryModel.Instance.GetSaveData());
            saveData.AddData("bookDropInventory", GetDropBookData());
            saveData.AddData("deckList", DeckListModel.Instance.GetSaveData());
            saveData.AddData("archipelago", PlaythruManager.GetSaveData());
            saveData.AddData("floorData", GetFloorData());

            string SaveFilePath = $"{Application.persistentDataPath}/Archipelago/{CurrentSaveFile}";
            object serializedData = saveData.GetSerializedData();

            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            using (FileStream serializationStream = File.Create(SaveFilePath))
            {
                new BinaryFormatter().Serialize(serializationStream, serializedData);
            }
        }

        internal static void LoadGame(string seed)
        {
            Debug.Log("Loading the game...");

            // GlobalGameManager.ContinueGame
            GlobalGameManager.Instance._gamePlayInitialized = false;
            if (!GlobalGameManager.Instance._initialized && UIAlarmPopup.instance != null)
            {
                UIAlarmPopup.instance.SetAlarmText("The game is not initialized. cannot start game");
                return;
            }

            if (PlatformManager.Instance.IsProccessing)
                return;

            AssetBundleManagerRemake.Instance.Init();


            // Now to the actual load
            CurrentSaveFile = seed;
            string SaveFilePath = $"{Application.persistentDataPath}/Archipelago/{CurrentSaveFile}";

            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            // If there is save like that, we load it, else we just create and empty one
            if (File.Exists(SaveFilePath))
            {
                // PlatformCore_steam.LoadPlayData
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                SaveData saveData = new SaveData();
                try
                {
                    object obj;
                    using (FileStream fileStream = File.Open(SaveFilePath, FileMode.Open))
                    {
                        obj = binaryFormatter.Deserialize(fileStream);
                    }
                    if (obj == null)
                    {
                        throw new Exception();
                    }

                    saveData.LoadFromSerializedData(obj);
                }
                catch (Exception)
                {
                    return;
                }

                // Now fill game data with the loaded save
                LoadFromSave(saveData);
            }
            else
            {
                LoadNew();
            }

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

            //LibraryModel.Instance.ClearInfo.AddClearCount(2);

            // Put the player in the game, loading is done
            GameSceneManager.Instance.ActivateUIController(initUIScene: true);

            GlobalGameManager.Instance._gamePlayInitialized = true;
        }

        private static void LoadFromSave(SaveData saveData)
        {
            LibraryModel.Instance.Init();
            LibraryModel.Instance._floorList.ForEach(f => f._level = 6);

            // Load base game stuff
            InventoryModel.Instance.LoadFromSaveData(saveData.GetData("inventory"));
            BookInventoryModel.Instance.LoadFromSaveData(saveData.GetData("bookInventory"));
            DeckListModel.Instance.LoadFromSaveData(saveData.GetData("deckList"));
            // Drop Books
            DropBookInventoryModel.Instance._bookList.Clear();
            DropBookInventoryModel.Instance._bookDictionary.Clear();

            SaveData data = saveData.GetData("bookDropInventory");
            if (data != null)
            {
                foreach (SaveData d in data.GetData("bookList"))
                {
                    SaveData id = d.GetData("id");
                    SaveData pkg = d.GetData("pkg");

                    LorId bookId = new LorId(pkg.GetStringSelf(), id.GetIntSelf());
                    //Debug.Log($"Loading book: {bookId}");

                    int num = d.GetInt("num");
                    DropBookInventoryModel.Instance.AddBook(bookId, num);
                }
            }

            // Load PlaythruManager
            PlaythruManager.LoadFromSaveData(saveData.GetData("archipelago"));

            // Set Floors Opened
            foreach (var f in PlaythruManager.Floors)
            {
                if (f.Value.Open)
                    LibraryModel.Instance._openedSephirah.Add(f.Key);
            }

            // Floor unit info
            data = saveData.GetData("floorData");
            foreach (LibraryFloorModel floor in LibraryModel.Instance._floorList)
            {
                SaveData _data = data.GetData(SephirahName.GetSephirahNameByType(floor.Sephirah));
                if (_data == null) continue;

                int num = 0;
                foreach (SaveData dat in _data.GetData("unitInfo"))
                {
                    Traverse.Create(floor).Field<List<UnitDataModel>>("_unitDataList").Value[num].LoadFromSaveData(dat);
                    num++;
                }

                floor.SetOpenedUnitCount(_data.GetInt("unitsOpened"));
            }
        }

        private static void LoadNew()
        {
            LibraryModel.Instance.Init();

            foreach (var floor in LibraryModel.Instance._floorList)
            {
                LibraryModel.Instance.OpenSephirah(floor.Sephirah);

                if (floor.Sephirah == SephirahType.Binah)
                {
                    floor.SetOpenedUnitCount(2);
                }

                floor._level = 6;
            }

            PlaythruManager.FoundBooks = new List<int>();

            PlaythruManager.Floors = Enum.GetValues(typeof(SephirahType)).Cast<SephirahType>().ToDictionary(k => k, v => new FloorInfo());

            PlaythruManager.BinahUnlocked = false;
            PlaythruManager.BlackSilenceUnlocked = false;
            PlaythruManager.MaxPassiveCost = 8;

            PlaythruManager.ItemsReceived = 0;
        }

        private static SaveData GetDropBookData()
        {
            SaveData saveData = new SaveData();
            SaveData saveData2 = new SaveData();
            foreach (OwnDropBookModel book in DropBookInventoryModel.Instance._bookList)
            {
                //Debug.Log($"Saving book: {book.XmlInfo.id}");
                SaveData saveData3 = new SaveData();
                saveData3.AddData("id", new SaveData(book.XmlInfo.id.id));
                saveData3.AddData("pkg", new SaveData(book.XmlInfo.id.packageId));
                saveData3.AddData("num", new SaveData(book.num));
                saveData2.AddToList(saveData3);
            }
            saveData.AddData("bookList", saveData2);

            return saveData;
        }

        private static SaveData GetFloorData()
        {
            SaveData saveData = new SaveData();
            foreach (LibraryFloorModel floor in LibraryModel.Instance._floorList)
            {
                SaveData saveData2 = new SaveData();
                SaveData saveData3 = new SaveData();
                foreach (UnitDataModel unitData in floor._unitDataList)
                {
                    saveData3.AddToList(unitData.GetSaveData());
                }
                saveData2.AddData("unitInfo", saveData3);
                saveData2.AddData("unitsOpened", new SaveData(floor.GetOpendUnitCount()));

                saveData.AddData(SephirahName.GetSephirahNameByType(floor.Sephirah), saveData2);
            }

            return saveData;
        }
    }
}
