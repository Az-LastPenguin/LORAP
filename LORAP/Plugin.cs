using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using BepInEx;
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

        internal void SendCheck(int id)
        {
            session.Locations.CompleteLocationChecks(id);
        }

        internal void SendCheck(long[] ids)
        {
            session.Locations.CompleteLocationChecks(ids);
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
            if (successResult.SlotData.TryGetValue("seed", out var seed))
            {
                APPlaythruManager.Seed = int.Parse((string)seed);
            }

            if (successResult.SlotData.TryGetValue("fillers", out var fillers))
            {
                APPlaythruManager.Fillers = Convert.ToInt32(fillers);
            }
            if (successResult.SlotData.TryGetValue("traps", out var traps))
            {
                APPlaythruManager.Traps = Convert.ToInt32(traps);
            }

            if (successResult.SlotData.TryGetValue("total_common", out var totalCommon))
            {
                APPlaythruManager.RarityDrops[Rarity.Common].totalPacks = Convert.ToInt32(totalCommon);
            }
            if (successResult.SlotData.TryGetValue("total_uncommon", out var totalUncommon))
            {
                APPlaythruManager.RarityDrops[Rarity.Uncommon].totalPacks = Convert.ToInt32(totalUncommon);
            }
            if (successResult.SlotData.TryGetValue("total_rare", out var totalRare))
            {
                APPlaythruManager.RarityDrops[Rarity.Rare].totalPacks = Convert.ToInt32(totalRare);
            }
            if (successResult.SlotData.TryGetValue("total_unique", out var totalUnique))
            {
                APPlaythruManager.RarityDrops[Rarity.Unique].totalPacks = Convert.ToInt32(totalUnique);
            }

            APConnectWindow.Close();

            // Add Custom Content
            CustomContentManager.AddCustomContent();

            // Create new game / Continue Game
            APSaveManager.CurrentSaveFile = $"{IP}_{SlotName}_{session.RoomState.Seed}";
            APSaveManager.LoadGame();

            // Recieve any items sent while not playing the game
            foreach (var item in session.Items.AllItemsReceived.Skip(APPlaythruManager.ItemsReceived))
            {
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

        private IEnumerator ItemReceivingCoroutine()
        {
            while (ItemsToReceive.Count > 0)
            {
                var item = ItemsToReceive.First();
                ItemsToReceive.RemoveAt(0);

                ItemLocationManager.ReceiveItem(item);

                yield return new WaitForSeconds(0.2f);
            }

            _itemReceiveCoroutine = null;
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
