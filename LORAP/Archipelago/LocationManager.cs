using Archipelago.MultiClient.Net;
using Archipelago.MultiClient.Net.Enums;
using Archipelago.MultiClient.Net.Models;
using Archipelago.MultiClient.Net.Packets;
using HarmonyLib;
using LORAP.Playthru;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;

namespace LORAP.Archipelago
{
    internal struct APItemInfo
    {
        public long Id;
        public string Name;
        public string Game;
        public ItemFlags Flags;
    }

    internal struct APLocationInfo
    {
        public long Id;
        public string Name;
        public string Game;
    }

    internal struct ItemLocationPair
    {
        public int Slot; // Player Slot ID
        public APLocationInfo Location;
        public APItemInfo Item;
    }

    internal static class LocationManager
    {
        // This class manages 3-in-1: Locations, Hints, Scouting.

        internal static ReadOnlyCollection<long> AllLocations => SessionManager.Locations.AllLocations;

        internal static ReadOnlyCollection<long> CheckedLocations => SessionManager.Locations.AllLocationsChecked;

        internal static ReadOnlyCollection<long> UncheckedLocations => SessionManager.Locations.AllMissingLocations;


        // All Scouted/Hinted Location-Item pairs
        internal static List<ItemLocationPair> KnownPairs = new List<ItemLocationPair>();

        // All Known Hints
        internal static List<Hint> KnownHints = new List<Hint>();


        internal static int GetReceptionIdFromLocationId(long id) => (int)(id & 0x0FFFFFFF);
        internal static List<long> GetUncheckedReceptionLocations(int id) => UncheckedLocations.Where(l => GetReceptionIdFromLocationId(l) == id).ToList();
        internal static List<long> GetReceptionLocations(int id) => AllLocations.Where(l => GetReceptionIdFromLocationId(l) == id).ToList();


        internal static void Init()
        {
            KnownPairs.Clear();
            KnownHints.Clear();

            // Scout every unchecked location & add them as location-item pairs
            SessionManager.Locations.ScoutLocationsAsync(HintCreationPolicy.None, UncheckedLocations.ToArray()).ContinueWith(t =>
            {
                foreach (KeyValuePair<long, ScoutedItemInfo> pair in t.Result)
                {
                    if (KnownPairs.Any(p => p.Location.Id == pair.Key && p.Item.Id == pair.Value.ItemId))
                        continue;

                    KnownPairs.Add(new ItemLocationPair
                    {
                        Slot = pair.Value.Player.Slot,
                        Location = new APLocationInfo
                        {
                            Id = pair.Key,
                            Name = pair.Value.LocationName,
                            Game = pair.Value.LocationGame,
                        },
                        Item = new APItemInfo
                        {
                            Id = pair.Value.ItemId,
                            Name = pair.Value.ItemName,
                            Game = pair.Value.ItemGame,
                            Flags = pair.Value.Flags,
                        }
                    });
                }
            });

            // Start tracking hints for this slot (Also retreives all already known hints)
            SessionManager.DataStorage.TrackHints(hints =>
            {
                KnownHints = hints.ToList();

                // Add info about not reached location-item pairs
                foreach (Hint hint in hints)
                {
                    if (hint.Found || KnownPairs.Any(p => p.Location.Id == hint.LocationId && p.Item.Id == hint.ItemId))
                        continue;

                    string LocationGame = SessionManager.Players.GetPlayerInfo(hint.FindingPlayer).Game;
                    string ItemGame = SessionManager.Players.GetPlayerInfo(hint.ReceivingPlayer).Game;

                    KnownPairs.Add(new ItemLocationPair
                    {
                        Slot = hint.FindingPlayer,
                        Location = new APLocationInfo
                        {
                            Id = hint.ItemId,
                            Name = SessionManager.Locations.GetLocationNameFromId(hint.ItemId, LocationGame),
                            Game = LocationGame,
                        },
                        Item = new APItemInfo
                        {
                            Id = hint.ItemId,
                            Name = SessionManager.Items.GetItemName(hint.ItemId, ItemGame),
                            Game = ItemGame,
                            Flags = hint.ItemFlags,
                        }
                    });
                }
            });
        }

        internal static void CompleteLocation(long id) // TODO: Make a queue so that if connection to the server is unstable, the checks aren't lost
        {
            SessionManager.Locations.CompleteLocationChecks(id);
        }

        internal static void CompleteLocations(List<long> ids)
        {
            SessionManager.Locations.CompleteLocationChecks(ids.ToArray());
        }

        internal static void AchieveGoal()
        {
            //SessionManager.SetGoalAchieved();
        }


        internal static void SendReceptionChecks(int id)
        {
            List<long> checks = LocationManager.UncheckedLocations.Where(l => (int)(l & 0x0FFFFFFF) == id).ToList();

            CompleteLocations(checks);
        }

        internal static string SendRandomReceptionCheck(int id)
        {
            List<long> checks = LocationManager.UncheckedLocations.Where(l => (int)(l & 0x0FFFFFFF) == id).ToList();

            if (checks.Count == 0)
                return "";

            System.Random random = new System.Random(SlotDataManager.Seed/2 + checks.Count);

            long check = checks.ElementAt(random.Next(checks.Count));

            CompleteLocation(check);

            ItemLocationPair pair = GetLocationPair(id);

            return pair.Slot == SessionManager.CurrentSlot ? $"Found {pair.Item.Name}!" : $"Sent {pair.Item.Name} to {SessionManager.Players.GetPlayerName(pair.Slot)}!";
        }

        internal static List<ItemLocationPair> GetPairsWithItemAndHint(long id, bool ignoreFound = false)
        {
            return KnownPairs.Where(p => p.Item.Id == id && KnownHints.Any(h => h.LocationId == p.Location.Id && (!ignoreFound || !h.Found))).ToList();
        }

        /*internal static ItemLocationPair GetPairWithItemAndHint(long id, bool ignoreFound = false)
        {
            return GetPairsWithItemAndHint(id, ignoreFound).First();
        }*/

        internal static List<ItemLocationPair> GetPairsWithItem(long id)
        {
            return KnownPairs.Where(p => p.Item.Id == id).ToList();
        }

        /*internal static ItemLocationPair GetPairWithItem(long id)
        {
            return GetPairsWithItem(id).First();
        }*/

        internal static ItemLocationPair GetLocationPair(long id)
        {
            return KnownPairs.Where(p => p.Location.Id == id).First();
        }


        internal static string FormatPairLocation(ItemLocationPair pair)
        {
            return $"{(pair.Slot == SessionManager.CurrentSlot ? "" : $"{SessionManager.Players.GetPlayerName(pair.Slot)}'s ")}{pair.Location.Name}";
        }

        internal static string FormatPairItem(ItemLocationPair pair)
        {
            return $"{(pair.Slot == SessionManager.CurrentSlot ? "" : $"{SessionManager.Players.GetPlayerName(pair.Slot)}'s ")}{pair.Item.Name}";
        }
    }
}
