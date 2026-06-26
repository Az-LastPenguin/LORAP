using LORAP.Playthru;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
        Pages
    }

    internal enum AbnoPageRandomization
    {
        None,
        Guarantee,
        Unbound
    }

    internal enum BookContentsRandomization
    {
        BookChapter,
        StageChapter,
        Chaotic
    }

    internal enum ProgressionMode
    {
        BookRequirements,
        BoESpheres
    }

    internal enum BattleNodeKind
    {
        Reception,
        Stage
    }

    internal enum DeathlinkAction
    {
        UnitDeath,
        FloorWipe,
        StageLoss
    }

    internal class BattleNode
    {
        public string Key;
        public int Id;
        public string Name;
        public BattleNodeKind Kind;
        public int Chapter;
        public int Sphere;
        public int SphereLayer;
        public int RequiredLibrarians;
        public SephirahType AssignedFloor = SephirahType.None;
        public List<string> Next = new List<string>();
        //public int VisualX;
        //public int VisualY;

        public bool AreBattleParentsComplete()
        {
            List<BattleNode> parents = SlotDataManager.BattleTree?.GetPrevNodes(Key) ?? new List<BattleNode>();
            if (parents.Count == 0)
                return true;

            foreach (BattleNode parent in parents)
            {
                if (!PlaythruManager.IsStageComplete(parent.Id))
                    continue;

                if (!SlotDataManager.BoESpheresEnabled || parent.Sphere == Sphere)
                    return true;

                if (parent.Sphere + 1 == Sphere && SlotDataManager.IsSphereClearEnough(parent.Sphere))
                    return true;
            }

            return false;
        }
    }

    internal class BattleTree
    {
        public Dictionary<string, BattleNode> Nodes = new Dictionary<string, BattleNode>();

        public BattleNode GetNode(string key) => Nodes.ContainsKey(key) ? Nodes[key] : null;

        public BattleNode GetNodeById(int id) => Nodes.Values.FirstOrDefault(n => n.Id == id);

        public List<BattleNode> GetPrevNodes(string key) => Nodes.Values.Where(n => n.Next.Contains(key)).ToList();

        public List<BattleNode> GetPrevNodesById(int id)
        {
            BattleNode node = GetNodeById(id);
            return node == null ? new List<BattleNode>() : GetPrevNodes(node.Key);
        }
    }

    internal static class SlotDataManager
    {
        /* ENDGOAL-RELATED */
        internal static List<Endgoal> Endgoals;

        internal static long EnsembleBattles;

        /* RANDOMIZATION */
        internal static int ClientSeed;
        internal static string EffectiveLORAPSeed;

        internal static AbnoPageShuffle AbnoPageShuffle;

        internal static AbnoPageRandomization AbnoPageRandomization;

        internal static bool ExodiaGuarantee;

        internal static bool EgoPageShuffle;

        // Page randomization here (someday)

        internal static bool RandomizeBlackSilencePage;

        internal static BookContentsRandomization BookContentsRandomization;

        internal static ProgressionMode ProgressionMode;

        internal static bool BoESpheresEnabled => ProgressionMode == ProgressionMode.BoESpheres;

        internal static int SphereClearPercentage;

        internal static bool ShuffleAbnos;

        internal static bool ShuffleRealizations;

        internal static bool ShuffleEnsembleFloors;

        /* PROGRESSION */
        internal static bool ReceptionsRequireBooks;

        internal static bool FloorsRequireBooks;

        internal static bool EnemiesTurnIntoChecks;

        internal static bool EndgoalsAlwaysUnlocked;

        /* OTHER */
        internal static bool Deathlink;

        internal static DeathlinkAction OutgoingDeathlink;

        internal static DeathlinkAction IncomingDeathlink;

        /* SLOT DATA */
        internal static Dictionary<int, List<int>> ReceptionBookRequirements;

        internal static BattleTree BattleTree;

        internal static bool HasBattleTree => BattleTree != null && BattleTree.Nodes.Count > 0;

        internal static Dictionary<SephirahType, List<List<int>>> AbnoBookRequirements;

        internal static Dictionary<SephirahType, List<int>> AbnoFightOrder;

        internal static Dictionary<int, int> AbnoStageChapters;

        internal static List<int> BoEBundlesPerSphere;

        internal static List<int> BoEBundlesCumulative;

        internal static int FirstReception;

        internal static int LastReception;

        internal static int GetBoEBundlesRequiredThroughSphere(int sphere)
        {
            if (BoEBundlesCumulative == null || sphere <= 0)
                return 0;

            int index = Math.Min(sphere, BoEBundlesCumulative.Count) - 1;
            return index >= 0 ? BoEBundlesCumulative[index] : 0;
        }

        internal static bool IsSphereClearEnough(int sphere)
        {
            if (!BoESpheresEnabled || BattleTree == null)
                return true;

            List<BattleNode> sphereNodes = BattleTree.Nodes.Values.Where(n => n.Sphere == sphere).ToList();
            if (sphereNodes.Count == 0)
                return true;

            int percentage = Math.Max(0, Math.Min(100, SphereClearPercentage));
            int required = (int)Math.Ceiling(sphereNodes.Count * percentage / 100.0);
            if (required <= 0)
                return true;

            int cleared = sphereNodes.Count(n => PlaythruManager.IsStageComplete(n.Id));
            return cleared >= required;
        }

        internal static void Parse(Dictionary<string, object> slotData)
        {
            /* ENDGOAL-RELATED */
            Endgoals = ((JArray)slotData["endgoals"]).Select(i => (Endgoal)Enum.Parse(typeof(Endgoal), i.Value<string>().Replace(" ", ""))).ToList();

            EnsembleBattles = (long)slotData["ensemble_battles"];

            /* RANDOMIZATION */
            ClientSeed = Convert.ToInt32(slotData["lorap_client_seed"]);
            EffectiveLORAPSeed = slotData.ContainsKey("effective_lorap_seed") ? Convert.ToString(slotData["effective_lorap_seed"]) : "unknown";
            Debug.Log($"[LORAP] Effective LORAP seed: {EffectiveLORAPSeed}");

            AbnoPageShuffle = (AbnoPageShuffle)(long)slotData["abno_page_shuffle"];

            AbnoPageRandomization = (AbnoPageRandomization)(long)slotData["abno_page_randomization"];

            ExodiaGuarantee = (long)slotData["exodia_guaratnee"] == 1;

            EgoPageShuffle = (long)slotData["ego_page_shuffle"] == 1;

            // Page randomization here (someday)

            RandomizeBlackSilencePage = (long)slotData["randomize_black_silence_page"] == 1;

            BookContentsRandomization = (BookContentsRandomization)(long)slotData["book_contents_randomization"];

            ProgressionMode = slotData.ContainsKey("progression_mode")
                ? (ProgressionMode)(long)slotData["progression_mode"]
                : ProgressionMode.BookRequirements;

            SphereClearPercentage = slotData.ContainsKey("sphere_clear_percentage")
                ? Convert.ToInt32(slotData["sphere_clear_percentage"])
                : 70;

            ShuffleAbnos = (long)slotData["shuffle_abnos"] == 1;

            ShuffleRealizations = (long)slotData["shuffle_realizations"] == 1;

            ShuffleEnsembleFloors = (long)slotData["shuffle_ensemble_floor"] == 1;

            /* PROGRESSION */
            ReceptionsRequireBooks = (long)slotData["receptions_require_books"] == 1;

            FloorsRequireBooks = (long)slotData["floors_require_books"] == 1;

            EnemiesTurnIntoChecks = (long)slotData["enemies_turn_into_checks"] == 1;

            EndgoalsAlwaysUnlocked = (long)slotData["endgoals_always_unlocked"] == 1;

            /* OTHER */
            Deathlink = (long)slotData["deathlink"] == 1;

            IncomingDeathlink = (DeathlinkAction)(long)slotData["incoming_deathlink"];

            OutgoingDeathlink = (DeathlinkAction)(long)slotData["outgoing_deathlink"];

            /* SLOT DATA */
            FirstReception = Convert.ToInt32(slotData["first_reception"]);

            LastReception = Convert.ToInt32(slotData["last_reception"]);

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

            AbnoStageChapters = new Dictionary<int, int>();
            if (slotData.ContainsKey("abno_stage_chapters"))
            {
                foreach (var o in (JObject)slotData["abno_stage_chapters"])
                {
                    AbnoStageChapters[Int32.Parse(o.Key)] = o.Value.Value<int>();
                }
            }

            BoEBundlesPerSphere = slotData.ContainsKey("boe_bundles_per_sphere")
                ? ((JArray)slotData["boe_bundles_per_sphere"]).Select(i => i.Value<int>()).ToList()
                : Enumerable.Repeat(0, 7).ToList();

            BoEBundlesCumulative = slotData.ContainsKey("boe_bundles_cumulative")
                ? ((JArray)slotData["boe_bundles_cumulative"]).Select(i => i.Value<int>()).ToList()
                : Enumerable.Repeat(0, 7).ToList();

            if (!slotData.ContainsKey("battle_nodes") || !slotData.ContainsKey("battle_edges"))
                throw new Exception("LORAP slot data is missing battle tree data.");

            BattleTree = new BattleTree();

            foreach (var o in (JObject)slotData["battle_nodes"])
            {
                JObject data = (JObject)o.Value;
                BattleNodeKind kind = data["kind"].Value<string>() == "stage"
                    ? BattleNodeKind.Stage
                    : BattleNodeKind.Reception;

                //if (!data.ContainsKey("visual_x") || !data.ContainsKey("visual_y"))
                //    throw new Exception($"LORAP battle node {o.Key} is missing visual coordinates.");

                BattleNode node = new BattleNode
                {
                    Key = o.Key,
                    Id = data["id"].Value<int>(),
                    Name = data["name"].Value<string>(),
                    Kind = kind,
                    Chapter = data["chapter"].Value<int>(),
                    Sphere = data.ContainsKey("sphere") ? data["sphere"].Value<int>() : data["chapter"].Value<int>(),
                    SphereLayer = data.ContainsKey("sphere_layer") ? data["sphere_layer"].Value<int>() : 0,
                    RequiredLibrarians = data["req_librarians"].Value<int>(),
                    //VisualX = data["visual_x"].Value<int>(),
                    //VisualY = data["visual_y"].Value<int>(),
                };

                if (kind == BattleNodeKind.Stage && data.ContainsKey("assigned_floor"))
                    node.AssignedFloor = (SephirahType)data["assigned_floor"].Value<int>();

                BattleTree.Nodes[node.Key] = node;
            }

            foreach (JObject edge in (JArray)slotData["battle_edges"])
            {
                string source = edge["source"].Value<string>();
                string target = edge["target"].Value<string>();

                if (BattleTree.Nodes.ContainsKey(source) && BattleTree.Nodes.ContainsKey(target))
                    BattleTree.Nodes[source].Next.Add(target);
            }
        }
    }
}
