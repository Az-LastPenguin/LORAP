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

        private static readonly ReadOnlyCollection<long> EmptyLocations = new ReadOnlyCollection<long>(new List<long>());

        internal static ReadOnlyCollection<long> AllLocations => SessionManager.IsConnected ? SessionManager.Locations.AllLocations : EmptyLocations;

        internal static ReadOnlyCollection<long> CheckedLocations => SessionManager.IsConnected ? SessionManager.Locations.AllLocationsChecked : EmptyLocations;

        internal static ReadOnlyCollection<long> UncheckedLocations => SessionManager.IsConnected ? SessionManager.Locations.AllMissingLocations : EmptyLocations;


        // All Scouted/Hinted Location-Item pairs
        internal static List<ItemLocationPair> KnownPairs = new List<ItemLocationPair>();

        // All Known Hints
        internal static List<Hint> KnownHints = new List<Hint>();


        internal static int GetReceptionIdFromLocationId(long id) => (int)(id & 0x0FFFFFFF);
        internal static List<long> GetUncheckedReceptionLocations(int id) => UncheckedLocations.Where(l => GetReceptionIdFromLocationId(l) == id).ToList();
        internal static List<long> GetReceptionLocations(int id) => AllLocations.Where(l => GetReceptionIdFromLocationId(l) == id).ToList();


        internal static void Init()
        {
            Debug.Log("[LORAP] Initializing AP Location Manager");

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
                            Id = hint.LocationId,
                            Name = SessionManager.Locations.GetLocationNameFromId(hint.LocationId, LocationGame),
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
            SessionManager.SetGoalAchieved();
        }

        internal static void SendStageChecks(int id)
        {
            List<long> checks = UncheckedLocations.Where(l => (int)(l & 0x0FFFFFFF) == id).ToList();

            CompleteLocations(checks);
        }

        internal static string SendRandomReceptionCheck(int id)
        {
            List<long> checks = UncheckedLocations.Where(l => (int)(l & 0x0FFFFFFF) == id).ToList();

            if (checks.Count == 0)
                return "";

            System.Random random = SlotDataManager.CreateRandom("reception_checks", id * 397 ^ checks.Count);

            long check = checks.ElementAt(random.Next(checks.Count));

            CompleteLocation(check);

            ItemLocationPair pair = GetLocationPair(check);

            return pair.Slot == SessionManager.CurrentSlot
                ? $"Found {pair.Item.Name}!"
                : $"Sent {pair.Item.Name} to {SessionManager.Players.GetPlayerName(pair.Slot)}!";
        }

        internal static List<ItemLocationPair> GetPairsWithItemAndHint(long id, bool ignoreFound = false)
        {
            return KnownPairs.Where(p => p.Item.Id == id && KnownHints.Any(h => h.LocationId == p.Location.Id && (!ignoreFound || !h.Found))).ToList();
        }

        internal static List<ItemLocationPair> GetPairsWithItem(long id)
        {
            return KnownPairs.Where(p => p.Item.Id == id).ToList();
        }

        internal static ItemLocationPair GetLocationPair(long id)
        {
            var pair = KnownPairs.FirstOrDefault(p => p.Location.Id == id);

            if (pair.Location.Id == 0 && pair.Item.Id == 0)
            {
                return new ItemLocationPair
                {
                    Slot = SessionManager.CurrentSlot,
                    Location = new APLocationInfo
                    {
                        Id = id,
                        Name = SessionManager.Locations.GetLocationNameFromId(id, "Library of Ruina"),
                        Game = "Library of Ruina",
                    },
                    Item = new APItemInfo
                    {
                        Id = 0,
                        Name = "Unknown Item",
                        Game = "",
                        Flags = ItemFlags.None,
                    }
                };
            }

            return pair;
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
