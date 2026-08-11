using System;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using LORAP.Utils;
using UnityEngine;

namespace LORAP.Archipelago
{
    [Serializable]
    internal class SessionData
    {
        public string IP = "";

        public string SlotName = "";

        public string Password = "";

        public float Progress = 0f;
    }

    internal static class SessionManager // TODO: Make use of DataStore.TrackClientState()
    {
        private static ArchipelagoSession Session;

        private static SessionData SessionData;

        internal static bool IsConnected => Session != null && Session.Socket != null && Session.Socket.Connected;

        internal static int CurrentSlot => IsConnected ? Players.ActivePlayer.Slot : -1;

        internal static string RoomSeed => Session.RoomState.Seed;

        internal static IReceivedItemsHelper Items => Session.Items;

        internal static IDataStorageHelper DataStorage => Session.DataStorage;

        internal static ILocationCheckHelper Locations => Session.Locations;

        internal static IHintsHelper Hints => Session.Hints;

        internal static IConnectionInfoProvider ConnectionInfo => Session.ConnectionInfo;

        internal static IPlayerHelper Players => Session.Players;

        // Event for message receive
        internal delegate void MessageHandler(string message);
        internal static event MessageHandler MessageEvent;

        internal static string GetPlayerName(int player) => Players.GetPlayerName(player);

        internal static void SetGoalAchieved()
        {
            if (IsConnected)
                Session.SetGoalAchieved();
        }

        internal static void CreateSession()
        {
            // If we already have an active session, don't create a new one
            if (Session != null && Session.Socket.Connected)
                return;

            // If we have don't an active session, or do but somehow it's not connected, create a new one
            Session = ArchipelagoSessionFactory.CreateSession(SessionData.IP);

            // Also connect message & item receiving methods
            Session.MessageLog.OnMessageReceived += OnMessageRecieved;
        }

        internal static void EndSession()
        {
            if (Session != null && Session.Socket.Connected)
                Session.Socket.DisconnectAsync();

            Session = null;
            SessionData = null;
        }

        internal static void TryConnect(string IP, string SlotName, string Password)
        {
            IP = (IP ?? "").Trim();
            SlotName = (SlotName ?? "").Trim();
            Password = (Password ?? "").Trim();

            // Set the Session Data
            SessionData = new SessionData()
            {
                IP = IP,
                SlotName = SlotName,
                Password = Password,
            };

            // Create AP Session
            CreateSession();

            // Connect to AP
            var result = Session.TryConnectAndLogin("Library of Ruina", SlotName, ItemsHandlingFlags.AllItems, password: Password, version: new Version(0, 6, 7));

            // If not successful, show error
            if (!result.Successful)
            {
                string text = "";
                foreach (var err in ((LoginFailure)result).Errors)
                {
                    text += $"{err}\n";
                }

                throw new Exception(text.TrimEnd());
            }

            // Parse Slot Data
            var slotData = ((LoginSuccessful)result).SlotData;

            SettingsManager.ParseSlotData(slotData);
            SlotDataManager.ParseSlotData(slotData);
        }

        private static void OnMessageRecieved(LogMessage message)
        {
            string text = "";
            foreach (var part in message.Parts)
            {
                var hex = part.Color.R.ToString("X2") + part.Color.G.ToString("X2") + part.Color.B.ToString("X2");

                if (hex != "FFFFFF")
                    text += $"<color=#{hex}>{part}</color>";
                else
                    text += part;
            }

            // Why this crashes the game if not using SPECIFICALLY COROUTINES is still a mysetry to this day for me.
            // I tried Threads, i tried Tasks, i tried async/await. Nothing works but Coroutines. Why.
            Timing.Instance.InvokeDelayed(() => MessageEvent.Invoke(text), 0);
        }

        internal static void SayMessage(string message)
        {
            Session.Say(message);
        }

        internal static SessionData GetLastSessionData()
        {
            SessionData Data = GameUtils.DeseriallizeFromFile<SessionData>($"{Application.persistentDataPath}/Archipelago", "LastSession");

            if (Data == null)
                Data = new SessionData();

            return Data;
        }

        internal static void SaveLastSessionData()
        {
            SessionData data = SessionData ?? new SessionData();
            data.Password = "";

            int allLocations = LocationManager.AllLocations.Count;
            int checkedLocations = LocationManager.CheckedLocations.Count;
            data.Progress = allLocations > 0 ? (float)checkedLocations / allLocations : data.Progress;

            GameUtils.SerializeToFile(data, $"{Application.persistentDataPath}/Archipelago", "LastSession");
        }
    }
}
