using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx;
using GameSave;
using HarmonyLib;
using LORAP.CustomUI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using UI;
using UnityEngine;

namespace LORAP
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class LORAP : BaseUnityPlugin
    {
        internal static Harmony harmony;

        internal static int Seed = 143;

        internal static LORAP Instance { get; private set; }

        private static ArchipelagoSession session;

        private void Start()
        {

        }
        
        private void Awake()
        {
            Instance = this;

            harmony = new Harmony($"com.MuhichAdditions.LastPenguin-{DateTime.Now.Ticks}");

            harmony.PatchAll();

            Logger.LogInfo($"Plugin {PluginInfo.PLUGIN_GUID} is loaded!");
        }

        internal void LogInfo(object message)
        {
            Logger.LogInfo(message);
        }

        internal int GetTotalLocationAmount()
        {
            return session.Locations.AllLocations.Count;
        }

        internal int GetFoundLocationAmount()
        {
            return session.Locations.AllLocationsChecked.Count;
        }

        internal void TryAPConnect(string IP, string SlotName, string Password)
        {
            APLog.Show();

            // Connect to AP
            if (session != null && session.Socket.Connected)
                session.Socket.DisconnectAsync();

            try
            {
                session = ArchipelagoSessionFactory.CreateSession(IP);
            }
            catch (Exception e)
            {
                APConnectWindow.SetInfoText(e.Message);
                return;
            }

            session.MessageLog.OnMessageReceived += OnMessageRecieved;

            var result = session.TryConnectAndLogin("Library of Ruina", SlotName, ItemsHandlingFlags.AllItems, password: Password);

            if (!result.Successful)
            {
                string text = "";
                foreach (var err in ((LoginFailure)result).Errors)
                {
                    text += $"{err}\n";
                }
                APConnectWindow.SetInfoText(text);

                return;
            }

            var successResult = (LoginSuccessful)result;
            if (successResult.SlotData.TryGetValue("seed", out var seed))
            {
                Seed = int.Parse((string)seed);
            }

            APConnectWindow.Close();

            LoadAPSaveData($"{IP}_{SlotName}");
        }

        private void OnMessageRecieved(LogMessage message)
        {
            string text = "";
            foreach (var part in message.Parts)
            {
                var hex = part.Color.R.ToString("X2") + part.Color.G.ToString("X2") + part.Color.B.ToString("X2");

                if (hex != "FFFFFF")
                    text += $"<color=#{hex}>" + part + "</color>";
                else
                    text += part;
            }

            StartCoroutine(Test(text));
        }
        
        private IEnumerator Test(string message)
        {
            yield return new WaitForSeconds(0);
            APLog.AddLog(message);
        }

        private void LoadAPSaveData(string SaveFile)
        {
            // Before data load
            APPlaythruManager.SetupGame();

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
            string SaveFilePath = $"{Application.persistentDataPath}/{SaveFile}";

            // If there is save like that, we load it, else we just create and empty one
            if (File.Exists(SaveFilePath))
            {
                // PlatformCore_steam.LoadPlayData
                FileStream fileStream = null;
                BinaryFormatter binaryFormatter = new BinaryFormatter();
                SaveData saveData = new SaveData();
                try
                {
                    if (File.Exists(SaveFilePath))
                    {
                        object obj;
                        using (fileStream = File.Open(SaveFilePath, FileMode.Open))
                        {
                            obj = binaryFormatter.Deserialize(fileStream);
                        }
                        if (obj == null)
                        {
                            throw new Exception();
                        }

                        saveData.LoadFromSerializedData(obj);
                    }
                    else throw new FileNotFoundException();
                }
                catch (Exception)
                {
                    return;
                }

                // Now fill game data with the loaded save
                LibraryModel.Instance.Init();
                LibraryModel.Instance.LoadFromSaveData(saveData);
            }
            else
            {
                LibraryModel.Instance.Init();

                // Set a bunch of stuff because we need to unlock and change stuff unlike in original game
                
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

            // Open each floor for debug reasons
            List<SephirahType> sephirahs = new List<SephirahType>() { SephirahType.Malkuth, SephirahType.Yesod, SephirahType.Hod, SephirahType.Netzach, SephirahType.Tiphereth, SephirahType.Gebura, SephirahType.Chesed, SephirahType.Binah, SephirahType.Hokma };
            foreach (var floor in Traverse.Create(LibraryModel.Instance).Field("_floorList").GetValue<List<LibraryFloorModel>>())
            {
                Traverse.Create(floor).Field("_level").SetValue(6);
            }

            // Add General receptions to the map
            AddReceptionToMap(100001, UIStoryLine.Chapter2, new Vector3(-260, 1880, 0));
            AddReceptionToMap(100002, UIStoryLine.Chapter2, new Vector3(-520, 1880, 0));
            AddReceptionToMap(100003, UIStoryLine.Chapter2, new Vector3(260, 1880, 0));

            AddReceptionToMap(100004, UIStoryLine.Chapter3, new Vector3(-450, 2900, 0));
            AddReceptionToMap(100006, UIStoryLine.Chapter3, new Vector3(450, 2900, 0));
            AddReceptionToMap(100005, UIStoryLine.Chapter3, new Vector3(0, 2900, 0));
            AddReceptionToMap(100007, UIStoryLine.Chapter3, new Vector3(-900, 2900, 0));
            AddReceptionToMap(100008, UIStoryLine.Chapter3, new Vector3(900, 2900, 0));

            AddReceptionToMap(100009, UIStoryLine.Chapter4, new Vector3(-450, 3610, 0));
            AddReceptionToMap(100010, UIStoryLine.Chapter4, new Vector3(0, 3610, 0));
            AddReceptionToMap(100014, UIStoryLine.Chapter4, new Vector3(450, 3610, 0));

            AddReceptionToMap(100011, UIStoryLine.Chapter5, new Vector3(-450, 4520, 0));
            AddReceptionToMap(100012, UIStoryLine.Chapter5, new Vector3(450, 4520, 0));

            AddReceptionToMap(100013, UIStoryLine.Chapter3, new Vector3(-450, 5550, 0));
            AddReceptionToMap(100015, UIStoryLine.Chapter3, new Vector3(450, 5550, 0));
            AddReceptionToMap(100016, UIStoryLine.Chapter3, new Vector3(0, 5690, 0));
            AddReceptionToMap(100017, UIStoryLine.Chapter3, new Vector3(0, 5420, 0));
            AddReceptionToMap(100018, UIStoryLine.Chapter3, new Vector3(-900, 5550, 0));
            AddReceptionToMap(100019, UIStoryLine.Chapter3, new Vector3(900, 5550, 0));

            // Put the player in the game, loading is done
            GameSceneManager.Instance.ActivateUIController(initUIScene: true);

            Traverse.Create(GlobalGameManager.Instance).Field("_gamePlayInitialized").SetValue(false); //TODO: Change to true
        }

        private void AddReceptionToMap(int id, UIStoryLine story, Vector3 position)
        {
            UIStoryProgressPanel MapPanel = (UI.UIController.Instance.GetUIPanel(UIPanelType.Invitation) as UIInvitationPanel).InvCenterStoryPanel;

            var original = Traverse.Create(MapPanel).Field<List<UIStoryProgressIconSlot>>("iconList").Value.First();
            var copy = Instantiate(original, Traverse.Create(MapPanel).Field<List<GameObject>>("chapterList").Value.First().transform);
            copy.transform.localPosition = position;
            Traverse.Create(copy).Field("StoryProgressPanel").SetValue(MapPanel);
            Traverse.Create(copy).Field("connectLineList").SetValue(new List<GameObject>());
            Traverse.Create(copy).Field("storyData").SetValue(new List<StageClassInfo>() { StageClassInfoList.Instance.GetData(id) });
            Traverse.Create(copy).Field("currentStory").SetValue(story);
            Traverse.Create(MapPanel).Field<List<UIStoryProgressIconSlot>>("iconList").Value.Add(copy);
        }
    }
}
