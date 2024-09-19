using GameSave;
using HarmonyLib;
using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UI;
using UnityEngine;

namespace LORAP
{
    internal static class APSaveManager
    {
        internal static string CurrentSaveFile;

        internal static SaveData lastSaveData;

        internal static void SaveGame()
        {
            SaveData saveData = new SaveData();
            saveData.AddData("inventory", Singleton<InventoryModel>.Instance.GetSaveData());
            saveData.AddData("bookInventory", Singleton<BookInventoryModel>.Instance.GetSaveData());
            saveData.AddData("bookDropInventory", GetDropBookData());
            saveData.AddData("deckList", Singleton<DeckListModel>.Instance.GetSaveData());
            saveData.AddData("archipelago", APPlaythruManager.GetSaveData());
            saveData.AddData("floorData", GetFloorData());
            saveData.AddData("openedFloorData", GetOpenedFloorData());

            lastSaveData = saveData;

            string SaveFilePath = $"{Application.persistentDataPath}/Archipelago/{CurrentSaveFile}";
            object serializedData = saveData.GetSerializedData();

            if (!Directory.Exists($"{Application.persistentDataPath}/Archipelago"))
                Directory.CreateDirectory($"{Application.persistentDataPath}/Archipelago");

            using (FileStream serializationStream = File.Create(SaveFilePath))
            {
                new BinaryFormatter().Serialize(serializationStream, serializedData);
            }
        }

        internal static void LoadGame()
        {
            // GlobalGameManager.ContinueGame
            Traverse.Create(GlobalGameManager.Instance).Field("_gamePlayInitialized").SetValue(false);
            if ((bool)Traverse.Create(GlobalGameManager.Instance).Field("_initialized").GetValue() == false)
            {
                if (UIAlarmPopup.instance != null)
                {
                    UIAlarmPopup.instance.SetAlarmText("The game is not initialized. cannot start game");
                }

                return;
            }

            if (PlatformManager.Instance.IsProccessing)
            {
                return;
            }

            Singleton<AssetBundleManagerRemake>.Instance.Init();


            // Now to the actual load
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

            PlayHistoryModel model = Traverse.Create(LibraryModel.Instance).Field("_playHistory").GetValue() as PlayHistoryModel;
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
            model.Clear_EndcontentsAllStage = 0;
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
            
            Traverse.Create(LibraryModel.Instance).Field("_playHistory").SetValue(model); // Check

            Traverse.Create(LibraryModel.Instance).Field("_currentChapter").SetValue(7);

            LibraryModel.Instance.ClearInfo.AddClearCount(2);

            // Put the player in the game, loading is done
            GameSceneManager.Instance.ActivateUIController(initUIScene: true);

            Traverse.Create(GlobalGameManager.Instance).Field("_gamePlayInitialized").SetValue(true);
        }

        private static void LoadFromSave(SaveData saveData)
        {
            LibraryModel.Instance.Init();

            lastSaveData = saveData;

            foreach (var floor in Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>())
            {
                LibraryModel.Instance.OpenSephirah(floor.Sephirah);

                if (floor.Sephirah == SephirahType.Binah)
                {
                    floor.SetOpenedUnitCount(2);
                }

                Traverse.Create(floor).Field("_level").SetValue(6);
            }

            Singleton<InventoryModel>.Instance.LoadFromSaveData(saveData.GetData("inventory"));
            Singleton<BookInventoryModel>.Instance.LoadFromSaveData(saveData.GetData("bookInventory"));
            Singleton<DeckListModel>.Instance.LoadFromSaveData(saveData.GetData("deckList"));
            APPlaythruManager.LoadFromSaveData(saveData.GetData("archipelago"));

            // Drop Books
            Traverse.Create(Singleton<DropBookInventoryModel>.Instance).Field<List<OwnDropBookModel>>("_bookList").Value.Clear();
            Traverse.Create(Singleton<DropBookInventoryModel>.Instance).Field<Dictionary<LorId, OwnDropBookModel>>("_bookDictionary").Value.Clear();
            SaveData data = saveData.GetData("bookDropInventory");
            if (data != null)
            {
                foreach (SaveData d in data.GetData("bookList"))
                {
                    SaveData data2 = d.GetData("id");
                    LorId bookId = LorId.None;
                    if (data2 != null)
                    {
                        bookId = LorId.LoadFromSaveData(data2);
                    }
                    int @int = d.GetInt("num");
                    Singleton<DropBookInventoryModel>.Instance.AddBook(bookId, @int);
                }
            }

            // Opened Floor Info
            data = saveData.GetData("openedFloorData");
            foreach (SaveData dat in data)
            {
                var sephs = Traverse.Create(LibraryModel.Instance).Field<HashSet<SephirahType>>("_openedSephirah").Value;

                SephirahType sephirahTypeByName = SephirahName.GetSephirahTypeByName(dat.GetStringSelf());
                if (sephirahTypeByName != 0 && !sephs.Contains(sephirahTypeByName))
                {
                    sephs.Add(sephirahTypeByName);
                }
            }

            // Floor unit info
            data = saveData.GetData("floorData");
            foreach (LibraryFloorModel floor in Traverse.Create(LibraryModel.Instance).Field<List<LibraryFloorModel>>("_floorList").Value)
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

            foreach (var floor in Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>())
            {
                LibraryModel.Instance.OpenSephirah(floor.Sephirah);

                if (floor.Sephirah == SephirahType.Binah)
                {
                    floor.SetOpenedUnitCount(2);
                }

                Traverse.Create(floor).Field("_level").SetValue(6);
            }

            APPlaythruManager.OpenedReceptions = new List<int>();
            APPlaythruManager.FoundBooks = new List<int>();

            APPlaythruManager.AbnoPageAmounts = new Dictionary<SephirahType, int>()
            {
                [SephirahType.Keter] = 0,
                [SephirahType.Malkuth] = 0,
                [SephirahType.Yesod] = 0,
                [SephirahType.Hod] = 0,
                [SephirahType.Netzach] = 0,
                [SephirahType.Tiphereth] = 0,
                [SephirahType.Gebura] = 0,
                [SephirahType.Chesed] = 0,
                [SephirahType.Binah] = 0,
                [SephirahType.Hokma] = 0,
            };

            APPlaythruManager.EGOAmounts = new Dictionary<SephirahType, int>()
            {
                [SephirahType.Keter] = 0,
                [SephirahType.Malkuth] = 0,
                [SephirahType.Yesod] = 0,
                [SephirahType.Hod] = 0,
                [SephirahType.Netzach] = 0,
                [SephirahType.Tiphereth] = 0,
                [SephirahType.Gebura] = 0,
                [SephirahType.Chesed] = 0,
                [SephirahType.Binah] = 0,
                [SephirahType.Hokma] = 0,
            };

            APPlaythruManager.AbnoProgress = new Dictionary<SephirahType, int>()
            {
                [SephirahType.Keter] = 1,
                [SephirahType.Malkuth] = 1,
                [SephirahType.Yesod] = 1,
                [SephirahType.Hod] = 1,
                [SephirahType.Netzach] = 1,
                [SephirahType.Tiphereth] = 1,
                [SephirahType.Gebura] = 1,
                [SephirahType.Chesed] = 1,
                [SephirahType.Binah] = 1,
                [SephirahType.Hokma] = 1,
            };

            APPlaythruManager.BinahUnlocked = false;
            APPlaythruManager.BlackSilenceUnlocked = false;
            APPlaythruManager.MaxPassiveCost = 8;

            APPlaythruManager.ItemsReceived = 0;

            APPlaythruManager.RarityDrops[Rarity.Common].notFoundItems = CustomContentManager.CustomBooks[0].DropItemList.ConvertAll(i => i.id.id);
            APPlaythruManager.RarityDrops[Rarity.Uncommon].notFoundItems = CustomContentManager.CustomBooks[1].DropItemList.ConvertAll(i => i.id.id);
            APPlaythruManager.RarityDrops[Rarity.Rare].notFoundItems = CustomContentManager.CustomBooks[2].DropItemList.ConvertAll(i => i.id.id);
            APPlaythruManager.RarityDrops[Rarity.Unique].notFoundItems = CustomContentManager.CustomBooks[3].DropItemList.ConvertAll(i => i.id.id);
        }

        private static SaveData GetDropBookData()
        {
            SaveData saveData = new SaveData();
            SaveData saveData2 = new SaveData();
            foreach (OwnDropBookModel book in Traverse.Create(Singleton<DropBookInventoryModel>.Instance).Field<List<OwnDropBookModel>>("_bookList").Value)
            {
                SaveData saveData3 = new SaveData();
                saveData3.AddData("id", new SaveData(book.XmlInfo.id.id));
                saveData3.AddData("num", new SaveData(book.num));
                saveData2.AddToList(saveData3);
            }
            saveData.AddData("bookList", saveData2);

            return saveData;
        }

        private static SaveData GetFloorData()
        {
            SaveData saveData = new SaveData();
            foreach (LibraryFloorModel floor in Traverse.Create(LibraryModel.Instance).Field<List<LibraryFloorModel>>("_floorList").Value)
            {
                SaveData saveData2 = new SaveData();
                SaveData saveData3 = new SaveData();
                foreach (UnitDataModel unitData in Traverse.Create(floor).Field<List<UnitDataModel>>("_unitDataList").Value)
                {
                    saveData3.AddToList(unitData.GetSaveData());
                }
                saveData2.AddData("unitInfo", saveData3);
                saveData2.AddData("unitsOpened", new SaveData(floor.GetOpendUnitCount()));

                saveData.AddData(SephirahName.GetSephirahNameByType(floor.Sephirah), saveData2);
            }

            return saveData;
        }

        private static SaveData GetOpenedFloorData()
        {
            SaveData saveData = new SaveData();
            
            foreach (var seph in LibraryModel.Instance.GetOpenedSephirahList())
            {
                saveData.AddToList(new SaveData(SephirahName.GetSephirahNameByType(seph)));
            }

            return saveData;
        }
    }
}
