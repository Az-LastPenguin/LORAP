using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Helpers;
using Archipelago.MultiClient.Net.MessageLog.Messages;
using Archipelago.MultiClient.Net.Models;
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

        public string Password;

        public float Progress;
    }

    internal static class SessionManager // TODO: Make use of DataStore.TrackClientState()
    {
        private static ArchipelagoSession session;

        internal static SessionData sessionData;

        internal static IReceivedItemsHelper Items => session.Items;

        internal static IDataStorageHelper DataStorage => session.DataStorage;

        internal static ILocationCheckHelper Locations => session.Locations;

        internal static IHintsHelper Hints => session.Hints;

        internal static IConnectionInfoProvider ConnectionInfo => session.ConnectionInfo;

        internal static IPlayerHelper Players => session.Players;


        internal static int CurrentSlot => Players.ActivePlayer.Slot;


        internal static void CreateSession(string IP)
        {
            // If we already have an active session, don't create a new one
            if (session != null && session.Socket.Connected)
                return;
            // If we have don't an active session, or do but somehow it's not connected, create a new one
            session = ArchipelagoSessionFactory.CreateSession(IP);

            // Also connect message & item receiving methods
            session.MessageLog.OnMessageReceived += OnMessageRecieved;
        }

        internal static void EndSession()
        {
            if (session != null && session.Socket.Connected)
                session.Socket.DisconnectAsync();

            session = null;
        }

        internal static void TryConnect(string IP, string SlotName, string Password)
        {
            // Create AP session
            CreateSession(IP);

            // Disallow connecting if someone else is already playing on this slot
            if (session.Players.AllPlayers.Where(p => p.Name == SlotName).Count() > 0)
                throw new Exception("Can't connect as this slot is currently being used!");

            // Connect to AP
            var result = session.TryConnectAndLogin("Library of Ruina", SlotName, ItemsHandlingFlags.AllItems, password: Password, version: new Version(0, 6, 3));

            // If not successful, show error
            if (!result.Successful)
            {
                string text = "";
                foreach (var err in ((LoginFailure)result).Errors)
                {
                    text += $"{err}\n";
                }

                throw new Exception(text);
            }

            // Parse Slot Data
            SlotDataManager.Parse(((LoginSuccessful)result).SlotData);
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

            // TODO: Change to async?
            IEnumerator AddLog() // a.k.a. WeirdFuckingFixOfCrash
            {
                yield return new WaitForSeconds(0);
                APLog.AddLog(text);
            }

            Timing.Coroutine(AddLog());
        }
    }
}
