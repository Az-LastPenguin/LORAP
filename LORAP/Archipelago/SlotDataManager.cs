using System;
using System.Collections.Generic;
using System.Linq;
using LORAP.Gameplay;
using Newtonsoft.Json.Linq;

namespace LORAP.Archipelago
{
    internal enum Endgoal
    {
        ReverberationEnsemble,
        BlackSilence,
        KeterRealization,
        DistortedEnsemble
    }

    internal enum AbnoPageShuffle
    {
        None,
        InFloor,
        Sets,
        Pages,
    }

    internal enum AbnoPageRandomization
    {
        None,
        Guarantee,
        Unbound
    }

    internal static class SlotDataManager
    {
        internal static int Seed;

        internal static List<Endgoal> Endgoals;

        internal static long EnsembleBattles;

        internal static AbnoPageShuffle AbnoPageShuffle;

        internal static AbnoPageRandomization AbnoPageRandomization;

        internal static bool ExodiaGuarantee;

        internal static bool EgoPageShuffle;

        internal static bool RandomizeReceptionTree;

        internal static bool ReceptionsRequireBooks;

        internal static bool EnemiesTurnIntoChecks;

        internal static bool ShuffleAbnos;

        internal static bool ShuffleRealizations;

        internal static bool FloorsRequireBooks;

        internal static bool RandomizeBlackSilencePage;

        internal static bool BalanceBookContents;

        internal static Dictionary<int, List<int>> ReceptionBookRequirements;

        internal static ReceptionTree ReceptionTree;

        internal static Dictionary<SephirahType, List<List<int>>> AbnoBookRequirements;

        internal static Dictionary<SephirahType, List<int>> AbnoFightOrder;

        internal static void Parse(Dictionary<string, object> slotData) // TODO: Generalize the code & if slot data doesn't match, warn the player
        {
            Seed = (int)(long)slotData["random_seed"]; // TODO: Somehow add verification for APWorld version and everything so runs don't brick in case of a version mismatch

            Endgoals = ((JArray)slotData["endgoals"]).Select(i => (Endgoal)Enum.Parse(typeof(Endgoal), i.Value<string>().Replace(" ", ""))).ToList();

            EnsembleBattles = (long)slotData["ensemble_battles"];

            AbnoPageShuffle = (AbnoPageShuffle)(long)slotData["abno_page_shuffle"];

            AbnoPageRandomization = (AbnoPageRandomization)(long)slotData["abno_page_randomization"];

            ExodiaGuarantee = (long)slotData["exodia_guaratnee"] == 1;

            EgoPageShuffle = (long)slotData["ego_page_shuffle"] == 1;

            RandomizeReceptionTree = (long)slotData["randomize_reception_tree"] == 1;

            ReceptionsRequireBooks = (long)slotData["receptions_require_books"] == 1;

            EnemiesTurnIntoChecks = (long)slotData["enemies_turn_into_checks"] == 1;

            ShuffleAbnos = (long)slotData["shuffle_abnos"] == 1;

            ShuffleRealizations = (long)slotData["shuffle_realizations"] == 1;

            FloorsRequireBooks = (long)slotData["floors_require_books"] == 1;

            RandomizeBlackSilencePage = (long)slotData["randomize_black_silence_page"] == 1;

            BalanceBookContents = (long)slotData["balance_book_contents"] == 1;

            ReceptionBookRequirements = new Dictionary<int, List<int>>();
            foreach (var o in (JObject)slotData["reception_book_requirements"])
            {
                ReceptionBookRequirements[Int32.Parse(o.Key)] = o.Value.Select(v => (int)v.Value<long>()).ToList();
            }

            AbnoBookRequirements = new Dictionary<SephirahType, List<List<int>>>();
            var abnoBooks = (JArray)slotData["abno_book_requirements"];
            for (int i = 0; i < 10; i++)
            {
                AbnoBookRequirements[(SephirahType)(i + 1)] = abnoBooks[i].Select(j => j.Select(k => (int)k.Value<long>()).ToList()).ToList();
            }

            AbnoFightOrder = new Dictionary<SephirahType, List<int>>();
            var abnoOrder = (JArray)slotData["abno_fight_order"];
            for (int i = 0; i < 10; i++)
            {
                AbnoFightOrder[(SephirahType)(i + 1)] = abnoOrder[i].Select(j => (int)j.Value<long>()).ToList();
            }



            ReceptionTree = new ReceptionTree();
            ReceptionTree.First = Convert.ToInt32(slotData["first_reception"]);
            ReceptionTree.Last = Convert.ToInt32(slotData["last_reception"]);

            foreach (var o in (JObject)slotData["reception_tree"])
            {
                ReceptionNode Node = new ReceptionNode()
                {
                    id = Int32.Parse(o.Key),
                    next = o.Value["next"].Select(v => v.Value<int>()).ToList(),
                    y = o.Value["y"].Value<int>()
                };

                ReceptionTree.Nodes.Add(Node);
            }
        }
    }
}
