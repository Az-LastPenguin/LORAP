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
using System.Linq;
using UnityEngine;

namespace LORAP
{
    [BepInPlugin(PluginInfo.PLUGIN_GUID, PluginInfo.PLUGIN_NAME, PluginInfo.PLUGIN_VERSION)]
    public class LORAP : BaseUnityPlugin
    {
        internal static Harmony harmony;

        internal static LORAP Instance { get; private set; }

        private static ArchipelagoSession session;

        private static List<long> ItemsToReceive = new List<long>();
        private static Coroutine _itemReceiveCoroutine;

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

        internal static void Log(object message)
        {
            Instance.Logger.LogInfo(message);
        }

        internal int GetTotalLocationAmount()
        {
            return session.Locations.AllLocations.Count;
        }

        internal int GetFoundLocationAmount()
        {
            return session.Locations.AllLocationsChecked.Count;
        }

        internal void SendCheck(int id)
        {
            session.Locations.CompleteLocationChecks(id);
        }

        internal void SendGoalReached()
        {
            session.SetGoalAchieved();
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

            var result = session.TryConnectAndLogin("Library of Ruina", SlotName, ItemsHandlingFlags.AllItems, password: Password, version: new Version(0,5,0));

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

            APPlaythruManager.Seed = Convert.ToInt32(successResult.SlotData.GetValueSafe("seed"));
            APPlaythruManager.Fillers = Convert.ToInt32(successResult.SlotData.GetValueSafe("fillers"));
            APPlaythruManager.Traps = Convert.ToInt32(successResult.SlotData.GetValueSafe("traps"));

            APPlaythruManager.RarityDrops[Rarity.Common].totalPacks = Convert.ToInt32(successResult.SlotData.GetValueSafe("total_common"));
            APPlaythruManager.RarityDrops[Rarity.Uncommon].totalPacks = Convert.ToInt32(successResult.SlotData.GetValueSafe("total_uncommon"));
            APPlaythruManager.RarityDrops[Rarity.Rare].totalPacks = Convert.ToInt32(successResult.SlotData.GetValueSafe("total_rare"));
            APPlaythruManager.RarityDrops[Rarity.Unique].totalPacks = Convert.ToInt32(successResult.SlotData.GetValueSafe("total_unique"));

            APPlaythruManager.AbnoPageBalance = (APPlaythruManager.AbnoPageBalanceType)Convert.ToInt32(successResult.SlotData.GetValueSafe("abno_page_balance"));

            APPlaythruManager.Goals = Convert.ToString(successResult.SlotData.GetValueSafe("end_goals")).Split(',').ToList().ConvertAll(g => 
            { 
                switch (g) 
                {
                    case "Reverberation Ensemble":
                        return APPlaythruManager.GoalType.ReverbEnsemble;
                    case "Black Silence":
                        return APPlaythruManager.GoalType.BlackSilence;
                    case "Keter Realization":
                        return APPlaythruManager.GoalType.KeterRealization;
                    case "Distorted Ensemble":
                        return APPlaythruManager.GoalType.DistortedEnsemble;
                }

                return APPlaythruManager.GoalType.ReverbEnsemble;
            });

            APPlaythruManager.EnsembleBattles = Convert.ToInt32(successResult.SlotData.GetValueSafe("ensemble_battles"));

            APConnectWindow.Close();

            // Add Custom Content
            CustomContentManager.AddCustomContent();

            // Create new game / Continue Game
            APSaveManager.CurrentSaveFile = session.RoomState.Seed;
            APSaveManager.LoadGame();

            // Recieve any items sent while not playing the game
            Log($"Items Received: {APPlaythruManager.ItemsReceived}");
            Log($"AllItems: {session.Items.AllItemsReceived.Count}");
            for (int i = 0; i < session.Items.AllItemsReceived.Count; i++)
            {
                var item = session.Items.DequeueItem();
                if (i < APPlaythruManager.ItemsReceived) continue;

                ItemsToReceive.Add(item.ItemId);
                APPlaythruManager.ItemsReceived++;

                if (_itemReceiveCoroutine == null)
                    _itemReceiveCoroutine = StartCoroutine(ItemReceivingCoroutine());
            }

            // Connect the item receive packet
            session.Items.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                long id = item.ItemId;

                ItemsToReceive.Add(item.ItemId);
                APPlaythruManager.ItemsReceived++;

                if (_itemReceiveCoroutine == null)
                    _itemReceiveCoroutine = StartCoroutine(ItemReceivingCoroutine());
            };
        }

        internal void APDisconnect()
        {
            if (session != null && session.Socket.Connected)
                session.Socket.DisconnectAsync();
        }
        private IEnumerator ItemReceivingCoroutine()
        {
            while (ItemsToReceive.Count > 0)
            {
                yield return new WaitForSeconds(0.2f);
                var item = ItemsToReceive.First();
                ItemsToReceive.RemoveAt(0);

                ItemLocationManager.ReceiveItem(item);
            }

            _itemReceiveCoroutine = null;

            Singleton<SaveManager>.Instance.SavePlayData(1);
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

            StartCoroutine(WeirdFuckingFixOfCrash(text));
        }
        
        private IEnumerator WeirdFuckingFixOfCrash(string message)
        {
            yield return new WaitForSeconds(0);
            APLog.AddLog(message);
        }
    }
}
