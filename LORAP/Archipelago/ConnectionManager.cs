using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using LORAP.CustomUI;
using LORAP.Gameplay;
using LORAP.Playthru;
using LORAP.Utils;
using UnityEngine;

namespace LORAP.Archipelago
{
    [Serializable]
    internal class SessionData
    {
        public string IP;

        public string SlotName;

        public float Progress;
    }

    internal static class ConnectionManager
    {
        private static ArchipelagoSession session;

        internal static SessionData currentSessionData;

        private static List<long> ItemsToReceive = new List<long>();

        private static Coroutine _itemReceiveCoroutine;

        internal static int TotalLocations => session.Locations.AllLocations.Count;

        internal static int FoundLocations => session.Locations.AllLocationsChecked.Count;

        internal static void Check(long id)
        {
            session.Locations.CompleteLocationChecks(id);
        }

        // Idk it just doesnt work.
        /*internal static void Checks(long[] id) 
        {
            session.Locations.CompleteLocationChecks(id);
        }*/

        internal static void AchieveGoal()
        {
            session.SetGoalAchieved();
        }

        internal static void TryConnect(string IP, string SlotName, string Password)
        {
            // Disconnect from AP if needed before connecting
            APDisconnect();

            // Create the session
            try
            {
                session = ArchipelagoSessionFactory.CreateSession(IP);
            }
            catch (Exception e)
            {
                APConnectWindow.SetInfoText(e.Message);
                return;
            }

            // To Show AP server messages to client
            session.MessageLog.OnMessageReceived += OnMessageRecieved;

            // Connect to AP
            var result = session.TryConnectAndLogin("Library of Ruina", SlotName, ItemsHandlingFlags.AllItems, password: Password, version: new Version(0, 5, 0));

            // If not successful, show error
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

            // Close Connection Window
            APConnectWindow.Close();


            // Setup a new run / Continue run
            PlaythruManager.SetupRun(((LoginSuccessful)result).SlotData, session.RoomState.Seed);


            // Recieve any items sent while not playing the game
            Debug.Log($"Items Received: {PlaythruManager.ItemsReceived}");
            Debug.Log($"AllItems: {session.Items.AllItemsReceived.Count}");

            // Coroutine that receives items
            IEnumerator ReceiveItems()
            {
                while (ItemsToReceive.Count > 0)
                {
                    yield return new WaitForSeconds(0.2f);
                    var item = ItemsToReceive.First();
                    ItemsToReceive.RemoveAt(0);

                    CheckManager.ReceiveItem(item);
                }

                _itemReceiveCoroutine = null;

                Singleton<GameSave.SaveManager>.Instance.SavePlayData(1);
            }

            for (int i = 0; i < session.Items.AllItemsReceived.Count; i++)
            {
                var item = session.Items.DequeueItem();
                if (i < PlaythruManager.ItemsReceived) continue;

                ItemsToReceive.Add(item.ItemId);
                PlaythruManager.ItemsReceived++;

                if (_itemReceiveCoroutine == null)
                    _itemReceiveCoroutine = Timing.Coroutine(ReceiveItems());
            }

            // Connect the item receive event
            session.Items.ItemReceived += (helper) =>
            {
                var item = helper.DequeueItem();
                long id = item.ItemId;

                ItemsToReceive.Add(item.ItemId);
                PlaythruManager.ItemsReceived++;

                if (_itemReceiveCoroutine == null)
                    _itemReceiveCoroutine = Timing.Coroutine(ReceiveItems());
            };

            APLog.Show();

            currentSessionData = new SessionData();
            currentSessionData.IP = IP;
            currentSessionData.SlotName = SlotName;

            SaveManager.SaveLastSessionData();
        }

        internal static void APDisconnect()
        {
            if (session != null && session.Socket.Connected)
                session.Socket.DisconnectAsync();
        }

        private static void OnMessageRecieved(LogMessage message)
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

            Debug.Log($"MESSAGE: {message}");

            IEnumerator AddLog() // a.k.a. WeirdFuckingFixOfCrash
            {
                yield return new WaitForSeconds(0);
                APLog.AddLog(text);
            }

            Timing.Coroutine(AddLog());
        }

        internal static SessionData GetSessionData()
        {
            currentSessionData.Progress = (float)FoundLocations / TotalLocations;

            return currentSessionData;
        }
    }
}
